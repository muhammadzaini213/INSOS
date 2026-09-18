using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileKeyboardTrigger : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    private TMP_InputField inputField;

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (inputField != null)
            inputField.ActivateInputField();

        TouchScreenKeyboard.Open(
            inputField != null ? inputField.text : "",
            TouchScreenKeyboardType.Default
        );
    }
}
