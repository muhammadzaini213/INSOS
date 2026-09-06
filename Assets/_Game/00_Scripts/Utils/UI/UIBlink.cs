using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIBlink : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Blink Settings")]
        [SerializeField] private float minAlpha = 0.2f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float blinkSpeed = 2f;

        [Header("Options")]
        [SerializeField] private bool infinite = true;
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool disableInteractionWhileDim = false;

        [Header("Random Blink")]
        [SerializeField] private bool randomBlink = false;

        [Range(0f, 1f)]
        [SerializeField] private float randomBlinkChance = 0.5f;

        [SerializeField] private float minRandomDelay = 0.5f;
        [SerializeField] private float maxRandomDelay = 2f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBlinkComplete;

        public event Action OnBlinkComplete;

        private Coroutine _routine;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(BlinkRoutine());
        }

        public void Stop()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            SetAlpha(maxAlpha);
        }

        private IEnumerator BlinkRoutine()
        {
            int currentBlink = 0;

            while (infinite || currentBlink < blinkCount)
            {
                // Random delay sebelum blink
                if (randomBlink)
                {
                    float delay = UnityEngine.Random.Range(
                        minRandomDelay,
                        maxRandomDelay
                    );

                    yield return WaitForSeconds(delay);

                    // Random boolean
                    bool shouldBlink =
                        UnityEngine.Random.value <= randomBlinkChance;

                    if (!shouldBlink)
                        continue;
                }

                // Blink
                yield return FadeAlpha(maxAlpha, minAlpha);
                yield return FadeAlpha(minAlpha, maxAlpha);

                currentBlink++;

                // Event setiap selesai satu blink
                OnBlinkComplete?.Invoke();
                onBlinkComplete?.Invoke();

                // Kalau bukan infinite dan sudah selesai
                if (!infinite && currentBlink >= blinkCount)
                    break;

                // Kalau random mode aktif, randomize lagi pada iterasi berikutnya
            }

            SetAlpha(maxAlpha);
            _routine = null;
        }

        private IEnumerator FadeAlpha(float from, float to)
        {
            float duration = 1f / Mathf.Max(blinkSpeed, 0.01f);
            float t = 0f;

            while (t < duration)
            {
                t += GetDeltaTime();

                float progress = Mathf.Clamp01(t / duration);
                float alpha = Mathf.Lerp(from, to, progress);

                SetAlpha(alpha);

                yield return null;
            }

            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;

            if (disableInteractionWhileDim)
            {
                bool isVisibleEnough = alpha > 0.5f;

                canvasGroup.interactable = isVisibleEnough;
                canvasGroup.blocksRaycasts = isVisibleEnough;
            }
        }

        private IEnumerator WaitForSeconds(float seconds)
        {
            float timer = 0f;

            while (timer < seconds)
            {
                timer += GetDeltaTime();
                yield return null;
            }
        }

        private float GetDeltaTime()
        {
            return useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
        }

        // --------------------------------------------------
        // Public API
        // --------------------------------------------------

        public void SetRandomBlink(bool value)
        {
            randomBlink = value;
        }

        public bool GetRandomBlink()
        {
            return randomBlink;
        }

        public void SetRandomBlinkChance(float chance)
        {
            randomBlinkChance = Mathf.Clamp01(chance);
        }

        public float GetRandomBlinkChance()
        {
            return randomBlinkChance;
        }

        public bool IsPlaying()
        {
            return _routine != null;
        }
    }
}