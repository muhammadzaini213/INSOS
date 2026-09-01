using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    public class UIShake : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private RectTransform target;

        [Header("Shake")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float strength = 10f;
        [SerializeField] private float frequency = 25f;
        [SerializeField] private bool loop = false;

        [Header("Options")]
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onShakeComplete;

        private Coroutine _routine;
        private Vector2 _originalPosition;

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;

            if (target != null)
                _originalPosition = target.anchoredPosition;
        }

        public void Shake()
        {
            if (target == null)
                return;

            if (_routine != null)
                StopCoroutine(_routine);

            _originalPosition = target.anchoredPosition;

            _routine = StartCoroutine(ShakeRoutine());
        }

        public void StopShake()
        {
            if (_routine == null)
                return;

            StopCoroutine(_routine);
            _routine = null;

            target.anchoredPosition = _originalPosition;
        }

        private IEnumerator ShakeRoutine()
        {
            do
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

                    // Strength berkurang menuju akhir setiap cycle.
                    float currentStrength =
                        strength * (1f - progress);

                    float angle =
                        elapsed * frequency * Mathf.PI * 2f;

                    float x = Mathf.Sin(angle);
                    float y = Mathf.Cos(angle);

                    target.anchoredPosition =
                        _originalPosition +
                        new Vector2(x, y) * currentStrength;

                    yield return null;
                }

            } while (loop);

            target.anchoredPosition = _originalPosition;

            _routine = null;

            onShakeComplete?.Invoke();
        }
    }
}