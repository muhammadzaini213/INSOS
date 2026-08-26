using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFade : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool disableInteractionWhileHidden = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onFadeInComplete;
        [SerializeField] private UnityEvent onFadeOutComplete;

        public event Action OnFadeInComplete;
        public event Action OnFadeOutComplete;

        private Coroutine _routine;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeIn()
        {
            StartFade(
                canvasGroup.alpha,
                1f,
                duration,
                () =>
                {
                    OnFadeInComplete?.Invoke();
                    onFadeInComplete?.Invoke();
                }
            );
        }

        public void FadeOut()
        {
            StartFade(
                canvasGroup.alpha,
                0f,
                duration,
                () =>
                {
                    OnFadeOutComplete?.Invoke();
                    onFadeOutComplete?.Invoke();
                }
            );
        }

        public void SetImmediate(float alpha)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = null;
            ApplyAlpha(alpha);
        }

        private void StartFade(float from, float to, float dur, Action onComplete)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(
                FadeRoutine(from, to, dur, onComplete)
            );
        }

        private IEnumerator FadeRoutine(
            float from,
            float to,
            float dur,
            Action onComplete)
        {
            float t = 0f;

            while (t < dur)
            {
                t += useUnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                float progress = dur > 0f
                    ? Mathf.Clamp01(t / dur)
                    : 1f;

                ApplyAlpha(Mathf.Lerp(from, to, progress));

                yield return null;
            }

            ApplyAlpha(to);

            _routine = null;
            onComplete?.Invoke();
        }

        private void ApplyAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;

            if (disableInteractionWhileHidden)
            {
                bool visible = alpha > 0.01f;

                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}