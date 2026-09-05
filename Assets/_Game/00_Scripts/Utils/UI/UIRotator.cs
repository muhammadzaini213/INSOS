using System;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Utils.UI
{
    public class UIRotator : MonoBehaviour
    {
        public float rotationSpeed = 90f;

        [Header("Events")]
        [SerializeField] private UnityEvent onRotationStart;
        [SerializeField] private UnityEvent onRotationStop;
        [SerializeField] private bool PlayOnEnable = true;

        public event Action OnRotationStart;
        public event Action OnRotationStop;

        private RectTransform rectTransform;
        private bool isRotating;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!isRotating)
                return;

            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
        }

        private void OnEnable()
        {
            if (PlayOnEnable)
                StartRotation();
        }

        public void StartRotation()
        {
            if (isRotating)
                return;

            isRotating = true;
            OnRotationStart?.Invoke();
            onRotationStart?.Invoke();
        }

        public void StopRotation()
        {
            if (!isRotating)
                return;

            isRotating = false;
            OnRotationStop?.Invoke();
            onRotationStop?.Invoke();
        }

        public void SetSpeed(float speed)
        {
            rotationSpeed = speed;
        }
    }
}