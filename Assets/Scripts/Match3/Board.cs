using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    public Tile[,] Tiles { get; private set; }
    private RectTransform[,] iconTransforms;

    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public Item nullItem;

    public Tile selectedTile;

    public Button resetButton;

    private bool canMove = true;

    List<Tile> tilesToRemove = new();

    private const float TweenDuration = 0.25f;

    public delegate void increaseScore(float scoreIncrease, ItemType type);
    public static event increaseScore OnIncreaseScore;

    private void Awake() => Instance = this;

    private void Start()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
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
        if (CheckBoard())
            ReinitializeBoard();
    }
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

    private void ReinitializeBoard()
    {
        Debug.Log("Board has been reinitialized");
        foreach(Tile tile in Tiles)
        {
            tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
        }
        if (CheckBoard())
            ReinitializeBoard();
    }

    public async Task ProcessTurnOnMatchedBoard()
    {
        canMove = false;
        foreach (Tile tileToRemove in tilesToRemove)
        {
            tileToRemove.isMatched = false;
            OnIncreaseScore?.Invoke(tileToRemove.Item.value, tileToRemove.Item.itemType);
        }
        await RemoveAndRefill(tilesToRemove);

        await Task.Delay(400); // Equivalent to WaitForSeconds(0.4f)

        while (CheckBoard())
        {
            await ProcessTurnOnMatchedBoard();
        }
        canMove = true;
    }

    #region Removing/Filling logic

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

    private async Task AnimateRemoval(List<Tile> tilesToRemove)
    {
        var inflateSequence = DOTween.Sequence();
        var deflateSequence = DOTween.Sequence();
        foreach (Tile tile in tilesToRemove)
        {
            inflateSequence.Append(tile.icon.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), TweenDuration));   
            deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, TweenDuration));
        }

        inflateSequence.Append(deflateSequence);

        await inflateSequence.Play().AsyncWaitForCompletion();

        foreach (Tile tile in tilesToRemove)
        {
            tile.icon.transform.localScale = Vector3.one; // Reset the scale to Vector3.one
            tile.icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Reset the anchored position to default
        }
    }

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
                        direction = MatchDirection.Super
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
                        direction = MatchDirection.Super
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
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedTiles.Count > 3)
        {
            Debug.Log("I have a long horizontal match, the color of my match is: " + connectedTiles[0].Item.itemType);
            return new MatchResult
            {
                connectedTiles = connectedTiles,
                direction = MatchDirection.LongHorizontal
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
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedTiles.Count > 3)
        {
            Debug.Log("I have a long vertical match, the color of my match is: " + connectedTiles[0].Item.itemType);
            return new MatchResult
            {
                connectedTiles = connectedTiles,
                direction = MatchDirection.LongVertical
            };
        }

        return new MatchResult
        {
            connectedTiles = new List<Tile> { tile },
            direction = MatchDirection.None
        };
    }

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

    private bool IsAdjacent(Tile currentTile, Tile targetTile)
    {
        return Mathf.Abs(currentTile.x - targetTile.x) + Mathf.Abs(currentTile.y - targetTile.y) == 1;
    }

    #endregion

}

public class MatchResult
{
    public List<Tile> connectedTiles;
    public MatchDirection direction;
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