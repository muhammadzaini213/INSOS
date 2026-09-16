using System.Collections;
using UnityEngine;

namespace Slafurry.Game.TutorialSystem
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialClickUI : MonoBehaviour, ITutorialStep
    {
        [Header("Titik Klik (opsional, drag child kosong di canvas ke sini)")]
        [SerializeField]
        private RectTransform targetPoint;

        [Header("Pop Settings")]
        [SerializeField]
        private float fadeDuration = 0.2f;

        [SerializeField]
        private float popScale = 1.3f;

        [SerializeField]
        private float popDuration = 0.15f;

        [SerializeField]
        private float shrinkDuration = 0.15f;

        [SerializeField]
        private float holdDuration = 0.3f;

        [SerializeField]
        private float delayBeforeLoop = 0.4f;

        [SerializeField]
        private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Test")]
        [SerializeField]
        private bool autoPlay;

        [Header("Loop")]
        [SerializeField]
        private bool loop = true;

        private RectTransform _rt;
        private CanvasGroup _cg;
        private Coroutine _routine;
        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _cg = GetComponent<CanvasGroup>();
            _cg.alpha = 0f;
            _cg.blocksRaycasts = false;
            _cg.interactable = false;
        }

        private void Start()
        {
            if (autoPlay)
                Play();
        }

        public void Play()
        {
            Stop();
            gameObject.SetActive(true);
            if (targetPoint != null)
                _rt.anchoredPosition = targetPoint.anchoredPosition;
            _routine = StartCoroutine(PlayRoutine());
        }

        public void Stop()
        {
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = null;
            _cg.alpha = 0f;
            _rt.localScale = _baseScale;
        }

        private IEnumerator PlayRoutine()
        {
            do
            {
                _rt.localScale = _baseScale;
                yield return Fade(0f, 1f, fadeDuration);

                yield return ScaleTo(1f, popScale, popDuration);
                yield return ScaleTo(popScale, 1f, shrinkDuration);

                yield return new WaitForSeconds(holdDuration);
                yield return Fade(1f, 0f, fadeDuration);

                yield return new WaitForSeconds(delayBeforeLoop);
            } while (loop);
        }

        private IEnumerator ScaleTo(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = popCurve.Evaluate(Mathf.Clamp01(t / duration));
                float s = Mathf.Lerp(from, to, p);
                _rt.localScale = _baseScale * s;
                yield return null;
            }
            _rt.localScale = _baseScale * to;
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
