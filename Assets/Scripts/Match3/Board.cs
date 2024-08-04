using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FlowerProject
{

    /// <summary>
    /// Class in charge of handling the match3 board
    /// </summary>
    public sealed class Board : MonoBehaviour
    {
        #region Variable Declaration
        public static Board Instance { get; private set; }

        public Row[] rows;

        public Tile[,] Tiles { get; private set; }
        private RectTransform[,] iconTransforms;


        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);

        public Item nullItem; // Null item to set when tiles are empty after a match and before being refilled

        public Tile selectedTile; // currently selected tile

        public Button resetButton;

        private bool canMove = true;
        private bool usingMinusDelete = false; // temporary boolean that when true makes the Minus items not count towards the progress bar when removed

        public bool isStarting = true; // temporary bool for temporary board scramble powerup usage

        List<Tile> tilesToRemove = new();

        private const float TweenDuration = 0.25f;

        public delegate void increaseScore(float scoreIncrease, ItemType type);
        public static event increaseScore OnIncreaseScore;

        #endregion
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Board Initialization
        private async void Start()
        {
            await InitializeBoard();
            isStarting = false;
        }

        /// <summary>
        /// Populates the Tiles array with the tiles contained in the rows and populate them with a random Item from ItemDatabase
        /// </summary>
        /// <returns></returns>
        private async Task InitializeBoard()
        {
            int width = rows.Max(row => row.tiles.Length);
            int height = rows.Length;

            Tiles = new Tile[width, height];
            iconTransforms = new RectTransform[width, height]; // Initialize the RectTransform array

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var tile = rows[y].tiles[x];

                    tile.x = x;
                    tile.y = height - 1 - y;

                    tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                    Tiles[x, height - 1 - y] = tile;
                    iconTransforms[x, height - 1 - y] = tile.icon.GetComponent<RectTransform>(); // Store the RectTransform
                }
            }
            if (CheckBoard()) // if there is a match reinitialize the board
                ReinitializeBoard();
        }

        /// <summary>
        /// Currently refills all tiles with a new random item and does so until there are no matches but if !isStarting reduces lives and processes any match found (this case is triggered by the shuffle power)
        /// </summary>
        public async void ReinitializeBoard()
        {
            if (!canMove)
                return;
            Debug.Log("Board has been reinitialized");
            foreach (Tile tile in Tiles)
            {
                tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
            }
            if (!isStarting) // this has to be moved when I properly implement powerups
            {
                ProgressCounter.Instance.CurrentMinus = 5;
                ProgressCounter.Instance.Lives -= 1;
                if (CheckBoard())
                    await ProcessTurnOnMatchedBoard();
                return;
            }
            if (CheckBoard())
            {
                ReinitializeBoard();
            }
        }

        /// <summary>
        /// Checks for matches
        /// </summary>
        /// <returns></returns>
        public bool CheckBoard()
        {
            Debug.Log("Checking Board");
            bool hasMatched = false;
            tilesToRemove.Clear();

            foreach (Tile tile in Tiles)
                tile.isMatched = false;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tile tile = Tiles[x, y];

                    if (!tile.isMatched && tile.Item.itemType != ItemType.Null)
                    {
                        MatchResult matchedTiles = IsConnected(tile);

                        if (matchedTiles.connectedTiles.Count >= 3)
                        {
                            MatchResult superMatchedTiles = SuperMatch(matchedTiles);

                            tilesToRemove.AddRange(superMatchedTiles.connectedTiles);

                            foreach (Tile til in superMatchedTiles.connectedTiles)
                                til.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }

            return hasMatched;
        }


        #endregion

        #region Removing/Filling logic

        /// <summary>
        /// Handles the tasks that remove tiles, makes them fall down and spawn new ones
        /// </summary>
        /// <param name="tilesToRemove"></param>
        /// <returns></returns>
        private async Task RemoveAndRefill(List<Tile> tilesToRemove)
        {
            await AnimateRemoval(tilesToRemove);

            foreach (Tile tile in tilesToRemove)
            {
                tile.Item = nullItem;
                tile.icon.transform.localScale = Vector3.one; // Reset the scale to Vector3.one
                tile.icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Reset the anchored position to default
            }

            while (CheckForEmptyTiles(out List<Tile> emptyTiles))
            {
                await AnimateFallingIcons();
                await SpawnItemsAtTop(emptyTiles);
            }
        }

        /// <summary>
        /// Uses a DOTween sequence to scale up icons before removing them and a dictionary to keep track of their score in order to update the value only once when the animations have completed
        /// </summary>
        /// <param name="tilesToRemove"></param>
        /// <returns></returns>
        private async Task AnimateRemoval(List<Tile> tilesToRemove)
        {
            // Dictionary to accumulate scores for each item type
            var scoreAccumulator = new Dictionary<ItemType, float>();

            var inflateSequence = DOTween.Sequence();
            var deflateSequence = DOTween.Sequence();
            foreach (Tile tile in tilesToRemove)
            {
                if (!usingMinusDelete)
                {
                    // Accumulate scores based on the item type
                    if (!scoreAccumulator.ContainsKey(tile.Item.itemType))
                    {
                        scoreAccumulator[tile.Item.itemType] = 0;
                    }
                    scoreAccumulator[tile.Item.itemType] += tile.Item.value;
                }
                inflateSequence.Append(tile.icon.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), TweenDuration));
                deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, TweenDuration));
            }

            inflateSequence.Append(deflateSequence);

            await inflateSequence.Play().AsyncWaitForCompletion();

            // Invoke OnIncreaseScore for each item type after the removal and refill
            foreach (var score in scoreAccumulator)
            {
                OnIncreaseScore?.Invoke(score.Value, score.Key);
            }


            foreach (Tile tile in tilesToRemove)
            {
                tile.icon.transform.localScale = Vector3.one; // Reset the scale to Vector3.one
                tile.icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Reset the anchored position to default
            }
        }

        /// <summary>
        /// Checks for tiles with null items in order to determine where to spawn new ones
        /// </summary>
        /// <param name="emptyTiles"></param>
        /// <returns></returns>
        private bool CheckForEmptyTiles(out List<Tile> emptyTiles)
        {
            emptyTiles = new List<Tile>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Tiles[x, y].Item.itemType == ItemType.Null)
                    {
                        emptyTiles.Add(Tiles[x, y]);
                    }
                }
            }

            return emptyTiles.Count > 0;
        }

    
        private async Task AnimateFallingIcons()
        {
            var moveTasks = new List<Task>();

            // Handle falling animations from bottom to top to avoid overwriting positions
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Tiles[x, y].Item.itemType == ItemType.Null)
                    {
                        for (int y2 = y + 1; y2 < Height; y2++)
                        {
                            if (Tiles[x, y2].Item.itemType != ItemType.Null)
                            {
                                Tile tileAbove = Tiles[x, y2];
                                Tile emptyTile = Tiles[x, y];

                                // Swap the items
                                emptyTile.Item = tileAbove.Item;
                                tileAbove.Item = nullItem;

                                // Add the move animation task
                                var moveTask = MoveIconToEmptyTile(tileAbove, emptyTile);
                                moveTasks.Add(moveTask);
                                break; // Stop searching in this column once the tile has been found and moved
                            }
                        }
                    }
                }
            }

            await Task.WhenAll(moveTasks);
        }

        /// <summary>
        /// Animates the icon moving falling from the tile above with a callback to ensure the position is reset correctly to the center of the tile when complete
        /// </summary>
        /// <param name="tileAbove"></param>
        /// <param name="emptyTile"></param>
        /// <returns></returns>
        private Task MoveIconToEmptyTile(Tile tileAbove, Tile emptyTile)
        {
            var iconAbove = tileAbove.icon.transform;
            var iconEmpty = emptyTile.icon.transform;


            var sequence = DOTween.Sequence();


            var targetPosition = iconEmpty.position;


            sequence.Append(iconAbove.DOMove(targetPosition, TweenDuration));

            // Add a callback to reset the anchoredPosition after the move completes
            sequence.AppendCallback(() =>
            {

                var rectTransform = iconAbove.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
            });


            return sequence.Play().AsyncWaitForCompletion();
        }

        /// <summary>
        /// Awaits SpawnItemAtTop for each empty tile
        /// </summary>
        /// <param name="emptyTiles"></param>
        /// <returns></returns>
        private async Task SpawnItemsAtTop(List<Tile> emptyTiles)
        {
            foreach (Tile emptyTile in emptyTiles)
            {
                if (emptyTile.Item.itemType == ItemType.Null)
                {
                    await SpawnItemAtTop(emptyTile.x, emptyTile.y);
                }
            }
        }

        /// <summary>
        /// Animates adding a new item at the target position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private async Task SpawnItemAtTop(int x, int y)
        {
            Tile newItem = Tiles[x, y];
            newItem.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

            newItem.icon.transform.position = new Vector3(x, Height, 0);
            newItem.icon.transform.localScale = Vector3.zero;

            var inflateSequence = DOTween.Sequence();
            inflateSequence.Join(newItem.icon.transform.DOMove(newItem.transform.position, TweenDuration));
            inflateSequence.Join(newItem.icon.transform.DOScale(Vector3.one, TweenDuration));
            await inflateSequence.Play().AsyncWaitForCompletion();

            // Ensure rectTransform is reset to default
            newItem.icon.transform.localScale = Vector3.one;
            newItem.icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        #endregion
        #region Matching logic
        /// <summary>
        /// Checks for superMatches (>= 5) and returns MatchResult containing the tiles matched, their type and their value
        /// </summary>
        /// <param name="_matchedResults"></param>
        /// <returns></returns>
        private MatchResult SuperMatch(MatchResult _matchedResults)
        {
            if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
            {
                foreach (Tile til in _matchedResults.connectedTiles)
                {
                    List<Tile> extraConnectedTiles = new();

                    CheckDirection(til, new Vector2Int(0, 1), extraConnectedTiles);
                    CheckDirection(til, new Vector2Int(0, -1), extraConnectedTiles);

                    if (extraConnectedTiles.Count >= 2)
                    {
                        Debug.Log("I have a super Horizontal Match");
                        extraConnectedTiles.AddRange(_matchedResults.connectedTiles);

                        return new MatchResult
                        {
                            connectedTiles = extraConnectedTiles,
                            direction = MatchDirection.Super,
                            matchType = _matchedResults.matchType,
                            matchValue = extraConnectedTiles.Count() * extraConnectedTiles[0].Item.value
                        };
                    }
                }
            }
            else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
            {
                foreach (Tile til in _matchedResults.connectedTiles)
                {
                    List<Tile> extraConnectedTiles = new();

                    CheckDirection(til, new Vector2Int(1, 0), extraConnectedTiles);
                    CheckDirection(til, new Vector2Int(-1, 0), extraConnectedTiles);

                    if (extraConnectedTiles.Count >= 2)
                    {
                        Debug.Log("I have a super Vertical Match");
                        extraConnectedTiles.AddRange(_matchedResults.connectedTiles);

                        return new MatchResult
                        {
                            connectedTiles = extraConnectedTiles,
                            direction = MatchDirection.Super,
                            matchType = _matchedResults.matchType,
                            matchValue = extraConnectedTiles.Count() * extraConnectedTiles[0].Item.value
                        };
                    }
                }
            }

            return new MatchResult
            {
                connectedTiles = _matchedResults.connectedTiles,
                direction = _matchedResults.direction
            };
        }

        /// <summary>
        /// Checks for matches in horizontal or vertical lines of 3 or more and returns a MatchResult
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        private MatchResult IsConnected(Tile tile)
        {
            List<Tile> connectedTiles = new();
            ItemType itemType = tile.Item.itemType;
            connectedTiles.Add(tile);

            CheckDirection(tile, new Vector2Int(1, 0), connectedTiles);
            CheckDirection(tile, new Vector2Int(-1, 0), connectedTiles);

            if (connectedTiles.Count == 3)
            {
                Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedTiles[0].Item.itemType);
                return new MatchResult
                {
                    connectedTiles = connectedTiles,
                    direction = MatchDirection.Horizontal,
                    matchType = connectedTiles[0].Item.itemType,
                    matchValue = connectedTiles.Count() * connectedTiles[0].Item.value
                };
            }
            else if (connectedTiles.Count > 3)
            {
                Debug.Log("I have a long horizontal match, the color of my match is: " + connectedTiles[0].Item.itemType);
                return new MatchResult
                {
                    connectedTiles = connectedTiles,
                    direction = MatchDirection.LongHorizontal,
                    matchType = connectedTiles[0].Item.itemType,
                    matchValue = connectedTiles.Count() * connectedTiles[0].Item.value
                };
            }

            connectedTiles.Clear();
            connectedTiles.Add(tile);

            CheckDirection(tile, new Vector2Int(0, 1), connectedTiles);
            CheckDirection(tile, new Vector2Int(0, -1), connectedTiles);

            if (connectedTiles.Count == 3)
            {
                Debug.Log("I have a normal vertical match, the color of my match is: " + connectedTiles[0].Item.itemType);
                return new MatchResult
                {
                    connectedTiles = connectedTiles,
                    direction = MatchDirection.Vertical,
                    matchType = connectedTiles[0].Item.itemType,
                    matchValue = connectedTiles.Count() * connectedTiles[0].Item.value
                };
            }
            else if (connectedTiles.Count > 3)
            {
                Debug.Log("I have a long vertical match, the color of my match is: " + connectedTiles[0].Item.itemType);
                return new MatchResult
                {
                    connectedTiles = connectedTiles,
                    direction = MatchDirection.LongVertical,
                    matchType = connectedTiles[0].Item.itemType,
                    matchValue = connectedTiles.Count() * connectedTiles[0].Item.value
                };
            }

            return new MatchResult
            {
                connectedTiles = new List<Tile> { tile },
                direction = MatchDirection.None
            };
        }

        /// <summary>
        /// Checks if neighbour tiles in the specified direction and inside the bounds of the board have a matching itemType and add them to the list and stops upon finding a different type
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="direction"></param>
        /// <param name="connectedTiles"></param>
        void CheckDirection(Tile tile, Vector2Int direction, List<Tile> connectedTiles)
        {
            ItemType itemType = tile.Item.itemType;

            if (itemType == ItemType.Null)
                return;

            int x = tile.x + direction.x;
            int y = tile.y + direction.y;

            while (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Tile neighbourTile = Tiles[x, y];

                if (!neighbourTile.isMatched && neighbourTile.Item.itemType == itemType)
                {
                    connectedTiles.Add(neighbourTile);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Swapping logic

        /// <summary>
        /// If canMove is true then select a tile, deselect it or swap it depending on the case
        /// </summary>
        /// <param name="tile"></param>
        public void Select(Tile tile)
        {
            if (!canMove)
                return;
            if (tile.IsSelected)
            {
                tile.IsSelected = false;
                selectedTile = null;
                return;
            }
            else if (tile.IsSelected == false && selectedTile == null)
            {
                tile.IsSelected = true;
                selectedTile = tile;
            }
            else if (selectedTile != tile)
            {
                Swap(selectedTile, tile);
                canMove = false;
                selectedTile.IsSelected = false;
                selectedTile = null;
            }
        }

        /// <summary>
        /// If the tiles are adjacent swap them and check for matched or else triggers WrongSwap
        /// </summary>
        /// <param name="tile1"></param>
        /// <param name="tile2"></param>
        private async void Swap(Tile tile1, Tile tile2)
        {
            if (!IsAdjacent(tile1, tile2))
            {
                await WrongSwap(tile1, tile2);
                canMove = true;
                return;
            }

            await DoSwap(tile1, tile2);

            await ProcessMatches(tile1, tile2);
        }

        /// <summary>
        /// DOTween animation to swap icons and tupple to swap their content
        /// </summary>
        /// <param name="tile1"></param>
        /// <param name="tile2"></param>
        /// <returns></returns>
        public async Task DoSwap(Tile tile1, Tile tile2)
        {
            var icon1 = tile1.icon;
            var icon2 = tile2.icon;

            var icon1Transform = icon1.transform;
            var icon2Transform = icon2.transform;

            var sequence = DOTween.Sequence();

            sequence.Join(icon1Transform.DOMove(icon2Transform.position, 0.4f))
                    .Join(icon2Transform.DOMove(icon1Transform.position, 0.4f))
                    .AppendInterval(TweenDuration);

            await sequence.Play().AsyncWaitForCompletion();

            icon1Transform.SetParent(tile2.transform);
            icon2Transform.SetParent(tile1.transform);

            tile1.icon = icon2;
            tile2.icon = icon1;

            (tile2.Item, tile1.Item) = (tile1.Item, tile2.Item);
        }

        /// <summary>
        /// Shakes the icons when an invalid swap was attempted
        /// </summary>
        /// <param name="tile1"></param>
        /// <param name="tile2"></param>
        /// <returns></returns>
        public async Task WrongSwap(Tile tile1, Tile tile2)
        {
            var sequence = DOTween.Sequence();

            sequence.Join(tile1.icon.rectTransform.DOShakeAnchorPos(0.4f, 2, 10, 90, false, true, ShakeRandomnessMode.Harmonic))
                    .Join(tile2.icon.rectTransform.DOShakeAnchorPos(0.4f, 2, 10, 90, false, true, ShakeRandomnessMode.Harmonic))
                    .AppendInterval(TweenDuration);
            await sequence.Play().AsyncWaitForCompletion();
            canMove = true;
        }

        private async Task ProcessMatches(Tile tile1, Tile tile2)
        {
            if (CheckBoard())
                await ProcessTurnOnMatchedBoard();
            else
            {
                await WrongSwap(tile1, tile2);
                await DoSwap(tile1, tile2);
            }
        }

        /// <summary>
        /// Sets isMatched to false from the tiles to remove then await their removal, after a small delay check the board for matches again, only makes canMove true at the end
        /// </summary>
        /// <returns></returns>
        private async Task ProcessTurnOnMatchedBoard()
        {
            canMove = false;


            foreach (Tile tileToRemove in tilesToRemove)
            {
                tileToRemove.isMatched = false;
            }

            await RemoveAndRefill(tilesToRemove);

            await Task.Delay(400); // Equivalent to WaitForSeconds(0.4f)

            // Repeat the process if there are more matches
            while (CheckBoard())
            {
                await ProcessTurnOnMatchedBoard();
            }
            canMove = true;
        }

        /// <summary>
        /// Checks if the input tiles are adjacent
        /// </summary>
        /// <param name="currentTile"></param>
        /// <param name="targetTile"></param>
        /// <returns></returns>
        private bool IsAdjacent(Tile currentTile, Tile targetTile)
        {
            return Mathf.Abs(currentTile.x - targetTile.x) + Mathf.Abs(currentTile.y - targetTile.y) == 1;
        }

        #endregion

        #region PowerUps Logic

        /// <summary>
        /// Removes Minus items (negative health) without affecting the progress bar and sets the power progress back to 0
        /// </summary>
        public async void DeleteMinus()
        {
            usingMinusDelete = true;
            if (!canMove || !ProgressCounter.Instance.canUsePower)
                return;
            canMove = false;
            ProgressCounter.Instance.CurrentPower = 0;
            ProgressCounter.Instance.maxPower = ProgressCounter.Instance.maxPower * 1.5f;
            tilesToRemove.Clear();
            foreach (Tile tile in Tiles)
                if (tile.Item.itemType == ItemType.Minus)
                    tilesToRemove.Add(tile);

            await RemoveAndRefill(tilesToRemove);

            await Task.Delay(400);

            usingMinusDelete = false;
            while (CheckBoard())
            {
                await ProcessTurnOnMatchedBoard();
            }
            canMove = true;

        }
        #endregion

    }

    /// <summary>
    /// Class that stores a list of connected tiles, the direction(type) of match, the item type and the value of the match
    /// </summary>
    public class MatchResult
    {
        public List<Tile> connectedTiles;
        public MatchDirection direction;

        public ItemType matchType;

        public float matchValue;
    }

    public enum MatchDirection
    {
        Vertical,
        Horizontal,
        LongVertical,
        LongHorizontal,
        Super,
        None
    }
}