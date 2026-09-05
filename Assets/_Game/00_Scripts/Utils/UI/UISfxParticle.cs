using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Slafurry.Utils.UI
{
    [RequireComponent(typeof(Image))]
    public class UISfxParticle : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Image targetImage;

        [Header("Jump")]
        [SerializeField]
        private float jumpHeight = 100f;

        [SerializeField]
        private float horizontalRange = 50f;

        [SerializeField]
        private float jumpUpDuration = 0.2f;

        [SerializeField]
        private float fallDownDuration = 0.3f;

        [Header("Fade")]
        [SerializeField]
        private float fadeDuration = 0.3f;

        [SerializeField]
        private float fadeStartDelay;

        [Header("Options")]
        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onFinished;

        public event Action OnFinished;

        private Coroutine routine;

        private void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();
        }

        private void OnDisable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        public void Play()
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(PlayRoutine());
        }

        public void Stop()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            SetAlpha(0f);
        }

        private IEnumerator PlayRoutine()
        {
            targetImage.enabled = true;
            SetAlpha(1f);

            RectTransform rect = targetImage.rectTransform;
            Vector2 startPos = rect.anchoredPosition;
            float horizontalDir = UnityEngine.Random.Range(-1f, 1f);
            float horizontalDist = UnityEngine.Random.Range(0f, horizontalRange);
            float elapsed = 0f;

            while (elapsed < jumpUpDuration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / jumpUpDuration);
                float curve = 1f - (1f - t) * (1f - t);
                float h = horizontalDir * horizontalDist * t;
                float v = jumpHeight * curve;
                rect.anchoredPosition = startPos + new Vector2(h, v);

                yield return null;
            }

            Vector2 peakPos = startPos + new Vector2(horizontalDir * horizontalDist, jumpHeight);
            elapsed = 0f;

            while (elapsed < fallDownDuration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / fallDownDuration);
                float curve = t * t;
                rect.anchoredPosition = Vector2.LerpUnclamped(peakPos, startPos, curve);

                yield return null;
            }

            rect.anchoredPosition = startPos;

            if (fadeStartDelay > 0f)
                yield return new WaitForSeconds(fadeStartDelay);

            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / fadeDuration);
                SetAlpha(1f - t);

                yield return null;
            }

            SetAlpha(0f);
            targetImage.enabled = false;

            routine = null;

            OnFinished?.Invoke();
            onFinished?.Invoke();
        }

        private void SetAlpha(float alpha)
        {
            Color c = targetImage.color;
            c.a = alpha;
            targetImage.color = c;
        }
    }
}
