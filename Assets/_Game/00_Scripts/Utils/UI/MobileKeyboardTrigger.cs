using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileKeyboardTrigger : MonoBehaviour, IPointerClickHandler
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
            inputField.text = _keyboard.text;
            inputField.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
            _keyboard = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
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
}
