using UnityEngine;
using UnityEngine.UI;
using Slafurry.System.Player;

namespace Slafurry.Game.Character
{
    [RequireComponent(typeof(Image))]
    public class CharacterSprite : MonoBehaviour
    {
        [Header("Boy Sprite")]
        [SerializeField] private Sprite boySprite;

        [Header("Girl Sprite")]
        [SerializeField] private Sprite girlSprite;

        [Header("Options")]
        [SerializeField] private bool updateOnEnable = true;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (updateOnEnable)
                ApplySprite();

            PlayerData.OnGenderChanged += OnGenderChanged;
        }

        private void OnDisable()
        {
            PlayerData.OnGenderChanged -= OnGenderChanged;
        }

        private void OnGenderChanged(Gender gender)
        {
            ApplySprite();
        }

        public void ApplySprite()
        {
            if (image == null)
                image = GetComponent<Image>();

            image.sprite = PlayerData.IsBoy ? boySprite : girlSprite;
        }

        public void SetSprites(Sprite boy, Sprite girl)
        {
            boySprite = boy;
            girlSprite = girl;
            ApplySprite();
        }
    }
}
