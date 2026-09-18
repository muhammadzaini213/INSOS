using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileKeyboardTrigger : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    private TMP_InputField inputField;

    private TouchScreenKeyboard _keyboard;

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
    }

    private void Update()
    {
        if (_keyboard == null || !_keyboard.active)
            return;

        inputField.text = _keyboard.text;
        inputField.caretPosition = _keyboard.text.Length;

        if (_keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            inputField.DeactivateInputField();
            _keyboard = null;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (inputField == null)
            return;

        inputField.ActivateInputField();
        _keyboard = TouchScreenKeyboard.Open(
            inputField.text,
            TouchScreenKeyboardType.Default,
            false,
            false,
            false,
            false,
            "Name"
        );
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_keyboard != null && _keyboard.active)
        {
            inputField.text = _keyboard.text;
            _keyboard = null;
        }
    }
}
