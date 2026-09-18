using Slafurry.System.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Slafurry.UI.Menu
{
    public class GenderSelector : MonoBehaviour
    {
        [Header("Images")]
        [SerializeField]
        private Image boyImage;

        [SerializeField]
        private Image girlImage;

        [Header("Dim Settings")]
        [SerializeField]
        private float dimAlpha = 0.35f;

        [SerializeField]
        private float selectedAlpha = 1f;

        private void OnEnable()
        {
            PlayerData.OnGenderChanged += OnGenderChanged;
            ApplySelection(PlayerData.CurrentGender);
        }

        private void OnDisable()
        {
            PlayerData.OnGenderChanged -= OnGenderChanged;
        }

        public void SelectBoy()
        {
            PlayerData.SetBoy();
        }

        public void SelectGirl()
        {
            PlayerData.SetGirl();
        }

        private void OnGenderChanged(Gender gender)
        {
            ApplySelection(gender);
        }

        private void ApplySelection(Gender gender)
        {
            if (boyImage == null || girlImage == null)
                return;

            bool isBoy = gender == Gender.Boy;

            SetImageAlpha(boyImage, isBoy ? selectedAlpha : dimAlpha);
            SetImageAlpha(girlImage, isBoy ? dimAlpha : selectedAlpha);
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
