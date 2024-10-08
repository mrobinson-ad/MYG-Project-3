using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using DG.Tweening;
namespace FlowerProject
{
    /// <summary>
    /// In charge of controlling the progress sliders and triggering the corresponding actions when they reach their max value
    /// </summary>
    public class ProgressCounter : MonoBehaviour
    {

        #region PC and UGUI variables
        public static ProgressCounter Instance { get; private set; }
        public float maxConsonant = 100;
        public float maxVowel = 100;
        public float maxPower = 100;
        private float maxMinus = 5;

        private float currentConsonant = 0;
        private float currentVowel = 0;
        public float currentPower = 0; //public for testing
        public float currentMinus = 5; //public for testing

        [SerializeField] UnityEngine.UI.Slider sliderConsonant;
        [SerializeField] UnityEngine.UI.Slider sliderVowel;
        [SerializeField] UnityEngine.UI.Slider sliderPower;
        [SerializeField] UnityEngine.UI.Slider sliderMinus;

        public bool canUsePower = false;

        public UnityEngine.UI.Button powerUpButton; //change to list
        private const float TweenDuration = 0.5f;

        /// <summary>
        /// Property that tweens the value of the consonant slider to progressively fill it when it increases and calls the OnProgressFilled coroutine when it reaches max
        /// </summary>
        public float CurrentConsonant
        {
            get => currentConsonant;
            set
            {
                currentConsonant = value;
                Debug.Log(currentConsonant);
                sliderConsonant.DOValue(Remap.RemapScore(maxConsonant, currentConsonant), TweenDuration);
                if (currentConsonant >= maxConsonant)
                {
                    StartCoroutine(OnProgressFilled(ItemType.Consonant, currentConsonant - maxConsonant));
                }
            }
        }

        /// <summary>
        /// Property that tweens the value of the vowel slider to progressively fill it when it increases and calls the OnProgressFilled coroutine when it reaches max
        /// </summary>
        public float CurrentVowel
        {
            get => currentVowel;
            set
            {
                currentVowel = value;
                sliderVowel.DOValue(Remap.RemapScore(maxVowel, currentVowel), TweenDuration);
                if (currentVowel >= maxVowel)
                {
                    StartCoroutine(OnProgressFilled(ItemType.Vowel, currentVowel - maxVowel));
                }

            }
        }

        /// <summary>
        /// Property that tweens the value of the power slider to progressively fill it when it increases and sets canUsePower to true when it reaches max
        /// </summary>
        public float CurrentPower
        {
            get => currentPower;
            set
            {
                currentPower = value;
                sliderPower.DOValue(Remap.RemapScore(maxPower, currentPower), TweenDuration);
                if (currentPower >= maxPower)
                {
                    canUsePower = true;
                    powerUpButton.interactable = true; //have this in separate method
                }
                else
                {
                    canUsePower = false;
                    powerUpButton.interactable = false; //have this in separate method
                }
                // add animation
            }
        }

        /// <summary>
        /// Property that tweens the value of the health slider to progressively decrease and resets it to the max health when it reaches 0
        /// </summary>
        public float CurrentMinus
        {
            get => currentMinus;
            set
            {
                currentMinus = value;
                sliderMinus.DOValue(Remap.RemapScore(maxMinus, currentMinus), TweenDuration);
                if (currentMinus <= 0)
                {
                    CurrentMinus = maxMinus; //Add delay before this
                    Lives -= 1;
                    //LoseHealth function
                }
            }
        }

        #endregion

        #region Flower and UXML variables
        private VisualElement root;
        public UIDocument gameUIDocument;
        private List<VisualElement> petals;
        private List<Vector2> petalPos;

        private int lives = 7;

        /// <summary>
        /// Lives property updates the lives display on change and triggers the Lose event when = 0
        /// </summary>
        public int Lives
        {
            get => lives;
            set
            {
                if (lives > value)
                    LoseRandomPetal();
                lives = Mathf.Clamp(value, 0, 7);
                if (lives == 7)
                    ResetPetals();
                if (lives <= 0)
                {
                    //GameManager.Lose();
                    Debug.Log("Game Over");
                }
            }
        }
        #endregion
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            Board.OnIncreaseScore += OnIncreaseScore;
            root = gameUIDocument.rootVisualElement;
            petals = new List<VisualElement>();
            petals.AddRange(root.Query<VisualElement>(className: "petal").ToList());
            petalPos = new List<Vector2>();
            StartCoroutine(CaptureInitialPositions());
        }

        void OnDestroy()
        {
            Board.OnIncreaseScore -= OnIncreaseScore;
            StopAllCoroutines();
        }
        /// <summary>
        /// When the OnIncreaseScore event is triggered increase the value of the slider corresponding to the type by the float score
        /// </summary>
        /// <param name="score"></param>
        /// <param name="type"></param>
        private void OnIncreaseScore(float score, ItemType type)
        {
            switch (type)
            {
                case ItemType.Consonant:
                    CurrentConsonant += score;
                    break;
                case ItemType.Vowel:
                    CurrentVowel += score;
                    break;
                case ItemType.Power:
                    CurrentPower += score;
                    break;
                case ItemType.Minus:
                    CurrentMinus -= score;
                    break;
                default:
                    CurrentConsonant += score;
                    CurrentVowel += score;
                    CurrentPower += score / 2;
                    break;
            }
        }

        /// <summary>
        /// Coroutine that triggers the Reveal methods in WordRevealer when a value reaches max and sets the slider back to 0 then after a delay returns it to the surplus from current - max
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private IEnumerator OnProgressFilled(ItemType itemType, float progress)
        {
            var sequence = DOTween.Sequence();
            if (itemType == ItemType.Consonant)
            {
                sequence.Append(sliderConsonant.DOValue(0, 1f))
                        .JoinCallback(() => WordRevealer.Instance.RevealConsonant())
                        .JoinCallback(() => AudioManager.SFXPressed("SFXRight"))
                        .PrependInterval(TweenDuration)
                        .AppendInterval(TweenDuration);
                sequence.Play();
                yield return sequence.WaitForCompletion();
                maxConsonant = Mathf.Clamp(maxConsonant * 1.25f, 0, 250);
                CurrentConsonant = progress;
                yield break;
            }
            else if (itemType == ItemType.Vowel)
            {
                sequence.Append(sliderVowel.DOValue(0, 1f))
                        .AppendCallback(() => WordRevealer.Instance.RevealVowel())
                        .AppendCallback(() => AudioManager.SFXPressed("SFXRight"))
                        .PrependInterval(TweenDuration)
                        .AppendInterval(TweenDuration);
                sequence.Play();
                yield return sequence.WaitForCompletion();
                maxVowel = Mathf.Clamp(maxVowel * 1.25f, 0, 250);
                CurrentVowel = progress;
                yield break;
            }
        }

        #region Flower methods

        /// <summary>
        /// Gets the initial position of the petals to return them on restart
        /// </summary>
        /// <returns></returns>
        private IEnumerator CaptureInitialPositions()
        {
            yield return new WaitForEndOfFrame();

            foreach (var petal in petals)
            {
                petalPos.Add(new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top));
            }
        }

        /// <summary>
        /// Uses a DOTween.ToArray to animate a random petal falling with a sway
        /// </summary>
        private void LoseRandomPetal()
        {
            int randomIndex = Random.Range(0, petals.Count);
            VisualElement petal = petals[randomIndex];
            Vector2 startPos = new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top);

            Vector3[] endValues = new[]
            {
                new Vector3(startPos.x + Random.Range(-150, -50), startPos.y + 100),
                new Vector3(startPos.x + Random.Range(50, 150), startPos.y + 200),
                new Vector3(startPos.x + Random.Range(-150, -50), startPos.y + 300),
                new Vector3(startPos.x + Random.Range(-50, 50), startPos.y + 400)
            };

            float[] durations = new[]
            {
                Random.Range(1f, 1.5f),
                Random.Range(1f, 1.5f),
                Random.Range(1f, 1.5f),
                Random.Range(1f, 1.5f)
            };

            Tween t = DOTween.ToArray(
                () => new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top),
                x =>
                {
                    petal.style.left = x.x;
                    petal.style.top = x.y;
                },
                endValues,
                durations
            );


            t.SetEase(Ease.InOutSine);

            float rotationDuration = Random.Range(3f, 6f);
            float rotationEndValue = Random.Range(-45f, -10f);

            float currentRotation = 0f;
            DOTween.To(() => currentRotation, x =>
            {
                currentRotation = x;
                petal.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            }, rotationEndValue, rotationDuration).SetEase(Ease.InOutSine);

            t.SetDelay(Random.Range(0f, 0.5f));

            petals.RemoveAt(randomIndex);
        }

        /// <summary>
        /// Adds the petals back into the list and resets them to their original position and rotation
        /// </summary>
        private void ResetPetals()
        {
            petals.Clear();
            petals.AddRange(root.Query<VisualElement>(className: "petal").ToList());
            int i = 0;
            foreach (var petal in petals)
            {
                petal.style.left = petalPos[i].x;
                petal.style.top = petalPos[i].y;
                petal.transform.rotation = Quaternion.Euler(0, 0, 0);
                i++;
            }
        }
        #endregion
    }
}