using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Slafurry.Game.TutorialSystem
{
    public class IdleWatcher : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Berapa detik tanpa input sebelum dianggap idle")]
        [SerializeField]
        private float idleThreshold = 5f;

        [Header("Events")]
        public UnityEvent OnIdle;
        public UnityEvent OnActive;

        private float _timer;
        private bool _isIdle;
        private Vector2 _lastPointerPos;

        public bool IsIdle => _isIdle;

        private void Start()
        {
            _lastPointerPos = GetPointerPosition();
            _timer = 0f;
            _isIdle = false;
        }

        private void Update()
        {
            if (HasInput())
            {
                ResetTimer();
            }
            else
            {
                _timer += Time.unscaledDeltaTime;
                if (!_isIdle && _timer >= idleThreshold)
                {
                    _isIdle = true;
                    OnIdle?.Invoke();
                }
            }
        }

        private bool HasInput()
        {
            Vector2 currentPos = GetPointerPosition();
            bool pointerMoved = (currentPos - _lastPointerPos).sqrMagnitude > 0.01f;
            _lastPointerPos = currentPos;

            bool mouseClicked =
                Mouse.current != null
                && (
                    Mouse.current.leftButton.wasPressedThisFrame
                    || Mouse.current.rightButton.wasPressedThisFrame
                    || Mouse.current.scroll.ReadValue().sqrMagnitude > 0.01f
                );

            bool keyPressed =
                Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

            bool touched = false;
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (
                        touch.press.wasPressedThisFrame
                        || touch.delta.ReadValue().sqrMagnitude > 0.01f
                    )
                    {
                        touched = true;
                        break;
                    }
                }
            }

            bool gamepadPressed =
                Gamepad.current != null
                && (
                    Gamepad.current.buttonSouth.wasPressedThisFrame
                    || Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f
                    || Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f
                );

            return pointerMoved || mouseClicked || keyPressed || touched || gamepadPressed;
        }

        private Vector2 GetPointerPosition()
        {
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            if (Touchscreen.current != null)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            return Vector2.zero;
        }

        private void ResetTimer()
        {
            _timer = 0f;
            if (_isIdle)
            {
                _isIdle = false;
                OnActive?.Invoke();
            }
        }

        public void ResetIdle()
        {
            ResetTimer();
        }
    }
}
