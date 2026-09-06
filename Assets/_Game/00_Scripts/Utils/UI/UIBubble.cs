using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIBubble : MonoBehaviour
    {
        [Header("Float")]
        [SerializeField]
        private float floatAmplitude = 20f;

        [SerializeField]
        private float floatSpeed = 1f;

        [Header("Sway")]
        [SerializeField]
        private float swayAmplitude = 10f;

        [SerializeField]
        private float swaySpeed = 0.7f;

        [Header("Tilt")]
        [SerializeField]
        private bool enableTilt = true;

        [SerializeField]
        private float tiltAngle = 5f;

        [SerializeField]
        private float tiltSpeed = 0.5f;

        [Header("Options")]
        [SerializeField]
        private bool playOnEnable = true;

        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onFloatStart;

        [SerializeField]
        private UnityEvent onFloatStop;

        private RectTransform rectTransform;
        private Vector2 originPosition;
        private float timeOffset;
        private bool isFloating;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            originPosition = rectTransform.anchoredPosition;
            timeOffset = Random.Range(0f, Mathf.PI * 2f);

            if (playOnEnable)
                StartFloat();
        }

        private void OnDisable()
        {
            StopFloat();
        }

        private void Update()
        {
            if (!isFloating)
                return;

            float time = GetTime() + timeOffset;

            float yOffset = Mathf.Sin(time * floatSpeed) * floatAmplitude;
            float xOffset = Mathf.Cos(time * swaySpeed) * swayAmplitude;

            rectTransform.anchoredPosition = originPosition + new Vector2(xOffset, yOffset);

            if (enableTilt)
            {
                float tilt = Mathf.Sin(time * tiltSpeed) * tiltAngle;
                rectTransform.rotation = Quaternion.Euler(0f, 0f, tilt);
            }
        }

        public void StartFloat()
        {
            if (isFloating)
                return;

            isFloating = true;
            onFloatStart?.Invoke();
        }

        public void StopFloat()
        {
            if (!isFloating)
                return;

            isFloating = false;
            rectTransform.anchoredPosition = originPosition;
            rectTransform.rotation = Quaternion.identity;
            onFloatStop?.Invoke();
        }

        public void SetOrigin(Vector2 position)
        {
            originPosition = position;
        }

        public bool IsFloating()
        {
            return isFloating;
        }

        private float GetTime()
        {
            return useUnscaledTime ? Time.unscaledTime : Time.time;
        }
    }
}
