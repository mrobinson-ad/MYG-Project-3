using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

namespace FlowerProject
{
    /// <summary>
    /// Handles lives, lose state, and flower animations in the future
    /// </summary>
    public class Flower : MonoBehaviour
    {
        private VisualElement root;
        public UIDocument gameUIDocument;
        private List<VisualElement> petals;
        private List<Vector2> petalPos;
        private VisualElement sunshine;

        private int lives = 7;
        public int Lives // Lives property updates the lives display on change and triggers the Lose event when = 0
        {
            get => lives;
            set
            {
                if (lives > value)
                    LoseRandomPetal();
                lives = Mathf.Clamp(value, 0, 7);
                if (lives == 7)
                    ResetPetals();
                if (lives == 0)
                {
                    StartCoroutine(CallLose());
                }
            }
        }

        private void Awake()
        {
            root = gameUIDocument.rootVisualElement;
            petals = new List<VisualElement>();
            petals.AddRange(root.Query<VisualElement>(className: "petal").ToList());
            petalPos = new List<Vector2>();
            sunshine = root.Q<VisualElement>("sunshine");
            StartCoroutine(CaptureInitialPositions());
        }

         /// <summary>
         /// Get each petal's initial position in order to reset them when restarting
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
        /// Uses DOTween.ToArray to make a random petal fall using some amount of randomness for a better effect then removes the petal from the List
        /// </summary>
        private void LoseRandomPetal() 
        {
            int randomIndex = Random.Range(0, petals.Count);
            VisualElement petal = petals[randomIndex];
            Vector2 startPos = new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top);

            Vector3[] endValues = new[]
            {
                new Vector3(startPos.x + Random.Range(-100, -50), startPos.y + 100),
                new Vector3(startPos.x + Random.Range(50, 100), startPos.y + 200),
                new Vector3(startPos.x + Random.Range(-100, -50), startPos.y + 300),
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
        /// Sets the petals to their initial position and rotation
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

        /// <summary>
        /// DOTween sequence using custom DOTween extension, shown when guessing a correct letter (bool is there for a plan to play a modified version on win)
        /// </summary>
        /// <param name="isWin"></param>
        public void SunshineAnimation(bool isWin) 
        {
            float targetRotation = (Random.value < 0.5f) ? -90f : 90f;
            var sequence = DOTween.Sequence();

            sequence.Join(sunshine.DORotate(0, targetRotation, 1.5f))
                    .Join(sunshine.DOAlpha(0, 1, 2f))
                    .Join(sunshine.DOScale(1f, 1f))
                    .Append(sunshine.DORotate(sunshine.transform.rotation.z, -targetRotation, 1.5f))
                    .Join(sunshine.DOAlpha(1, 0, 1f))
                    .Join(sunshine.DOScale(0f, 1f));

            sequence.Play();
        }

        /// <summary>
        /// Called when Lives == 0 to instantly disable the keyboard but leave a delay before starting the Lose event
        /// </summary>
        /// <returns></returns>
        private IEnumerator CallLose() 
        {
            WordManager.Instance.DisableKeyboard();
            yield return new WaitForSeconds(3f);
            GameManager.Lose();
        }
    }
}