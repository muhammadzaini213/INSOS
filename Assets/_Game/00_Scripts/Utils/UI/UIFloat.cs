using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Simple floating animation for UI elements.
    /// Moves the UI smoothly up and down around its original position.
    /// </summary>
    public class UIFloat : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private RectTransform target;

        [Header("Float")]
        [SerializeField]
        private float height = 10f;

        [SerializeField]
        private float speed = 2f;

        [SerializeField]
        private bool loop = true;

        [Header("Options")]
        [SerializeField]
        private bool useUnscaledTime = true;

        [SerializeField]
        private bool playOnEnable = false;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onFloatStart;

        [SerializeField]
        private UnityEvent onFloatStop;

        private Coroutine _routine;
        private Vector2 _originalPosition;
        private bool _positionCaptured;
        private bool _pendingPlay;

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;
        }

        private void OnEnable()
        {
            if (playOnEnable)
                _pendingPlay = true;
        }

        private void LateUpdate()
        {
            if (_pendingPlay)
            {
                _pendingPlay = false;
                Float();
            }
        }

        private void OnDisable()
        {
            _pendingPlay = false;

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        public void Float()
        {
            if (target == null)
                return;

            if (_routine != null)
                StopCoroutine(_routine);

            if (!_positionCaptured)
            {
                _originalPosition = target.anchoredPosition;
                _positionCaptured = true;
            }

            _routine = StartCoroutine(FloatRoutine());

            onFloatStart?.Invoke();
        }

        public void StopFloat()
        {
            if (_routine == null)
                return;

            StopCoroutine(_routine);
            _routine = null;

            target.anchoredPosition = _originalPosition;

            onFloatStop?.Invoke();
        }

        public void SetPosition(Vector2 position)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            _originalPosition = position;
            _positionCaptured = true;
            target.anchoredPosition = position;
        }

        private IEnumerator FloatRoutine()
        {
            float time = 0f;

            do
            {
                time = 0f;

                while (time < Mathf.PI * 2f)
                {
                    float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                    time += deltaTime * speed;

                    float offset = Mathf.Sin(time) * height;

                    target.anchoredPosition = _originalPosition + Vector2.up * offset;

                    yield return null;
                }
            } while (loop);

            target.anchoredPosition = _originalPosition;

            _routine = null;

            onFloatStop?.Invoke();
        }
    }
}
