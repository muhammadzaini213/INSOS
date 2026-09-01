using UnityEngine;

namespace Slafurry.Utils.UI
{
    public class UIRotator : MonoBehaviour
    {
        public float rotationSpeed = 90f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}