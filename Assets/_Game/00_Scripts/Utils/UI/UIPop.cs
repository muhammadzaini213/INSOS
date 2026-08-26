using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Simple one-shot pop in/out animation for UI elements.
    /// Scales the target transform with an ease-out animation.
    /// </summary>
    public class UIPop : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private RectTransform target;

        [Header("Pop In")]
        [SerializeField] private float popInDuration = 0.25f;
        [SerializeField] private float popInStartScale = 0f;
        [SerializeField] private float popInTargetScale = 1f;

        [Header("Pop Out")]
        [SerializeField] private float popOutDuration = 0.2f;
        [SerializeField] private float popOutStartScale = 1f;
        [SerializeField] private float popOutTargetScale = 0f;

        [Header("Options")]
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onPopInComplete;
        [SerializeField] private UnityEvent onPopOutComplete;

        private Coroutine _routine;

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;
        }

        public void PopIn()
        {
            StartPop(
                popInStartScale,
                popInTargetScale,
                popInDuration,
                onPopInComplete
            );
        }

        public void PopOut()
        {
            StartPop(
                popOutStartScale,
                popOutTargetScale,
                popOutDuration,
                onPopOutComplete
            );
        }

        public void SetScale(float scale)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = null;
            target.localScale = Vector3.one * scale;
        }

        private void StartPop(
            float from,
            float to,
            float duration,
            UnityEvent onComplete)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(
                PopRoutine(from, to, duration, onComplete)
            );
        }

        private IEnumerator PopRoutine(
            float from,
            float to,
            float duration,
            UnityEvent onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                float progress = duration > 0f
                    ? Mathf.Clamp01(elapsed / duration)
                    : 1f;

                // Ease Out Back
                float eased = EaseOutBack(progress);

                float scale = Mathf.LerpUnclamped(from, to, eased);

                target.localScale = Vector3.one * scale;

                yield return null;
            }

            target.localScale = Vector3.one * to;

            _routine = null;

            onComplete?.Invoke();
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f +
                   c3 * Mathf.Pow(t - 1f, 3f) +
                   c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}