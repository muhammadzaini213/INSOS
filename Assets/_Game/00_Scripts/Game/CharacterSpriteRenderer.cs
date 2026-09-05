using UnityEngine;
using Slafurry.System.Player;

namespace Slafurry.Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterSpriteRenderer : MonoBehaviour
    {
        [Header("Boy Sprite")]
        [SerializeField] private Sprite boySprite;

        [Header("Girl Sprite")]
        [SerializeField] private Sprite girlSprite;

        [Header("Options")]
        [SerializeField] private bool updateOnEnable = true;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = PlayerData.IsBoy ? boySprite : girlSprite;
        }

        public void SetSprites(Sprite boy, Sprite girl)
        {
            boySprite = boy;
            girlSprite = girl;
            ApplySprite();
        }
    }
}
