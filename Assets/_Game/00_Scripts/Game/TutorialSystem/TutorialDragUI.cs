using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Slafurry.Game.TutorialSystem
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialDragUI : MonoBehaviour, ITutorialStep
    {
        [Header("Titik Awal & Akhir (drag child kosong di canvas ke sini)")]
        [SerializeField]
        private RectTransform pointA;

        [SerializeField]
        private RectTransform pointB;

        [Header("Timing")]
        [SerializeField]
        private float moveDuration = 1f;

        [SerializeField]
        private float fadeDuration = 0.3f;

        [SerializeField]
        private float holdAtStart = 0.2f;

        [SerializeField]
        private float holdAtEnd = 0.2f;

        [SerializeField]
        private float delayBeforeLoop = 0.5f;

        [SerializeField]
        private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Loop")]
        [SerializeField]
        private bool loop = true;

        private RectTransform _rt;
        private CanvasGroup _cg;
        private Coroutine _routine;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _cg = GetComponent<CanvasGroup>();
            _cg.alpha = 0f;
            _cg.blocksRaycasts = false;
            _cg.interactable = false;
        }

        public void Play()
        {
            if (pointA == null || pointB == null)
            {
                Debug.LogWarning(
                    $"[TutorialDragUI] pointA/pointB belum di-assign di {gameObject.name}, skip play."
                );
                return;
            }

            Stop();
            gameObject.SetActive(true);
            _routine = StartCoroutine(PlayRoutine());
        }

        public void Stop()
        {
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = null;
            _cg.alpha = 0f;
        }

        private IEnumerator PlayRoutine()
        {
            do
            {
                _rt.anchoredPosition = pointA.anchoredPosition;

                yield return Fade(0f, 1f, fadeDuration);
                yield return new WaitForSeconds(holdAtStart);

                yield return MoveTo(pointA.anchoredPosition, pointB.anchoredPosition, moveDuration);

                yield return new WaitForSeconds(holdAtEnd);
                yield return Fade(1f, 0f, fadeDuration);

                yield return new WaitForSeconds(delayBeforeLoop);
            } while (loop);
        }

        private IEnumerator MoveTo(Vector2 from, Vector2 to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = moveCurve.Evaluate(Mathf.Clamp01(t / duration));
                _rt.anchoredPosition = Vector2.Lerp(from, to, p);
                yield return null;
            }
            _rt.anchoredPosition = to;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            _cg.alpha = to;
        }
    }
}
