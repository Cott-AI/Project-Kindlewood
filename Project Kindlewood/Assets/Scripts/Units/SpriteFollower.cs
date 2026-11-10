using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFollower : MonoBehaviour
    {
        [Header("The Main Body")]
        public SpriteRenderer bodyRenderer;
    
        [Header("The Outfit Sprites")]
        public Sprite[] outfitSprites;

        private SpriteRenderer _myRenderer;

        private void Awake()
        {
            _myRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (bodyRenderer.sprite == null || outfitSprites.Length == 0) return;

            string bodySpriteName = bodyRenderer.sprite.name;
            int underscoreIndex = bodySpriteName.LastIndexOf('_');
        
            if (underscoreIndex != -1)
            {
                string numberString = bodySpriteName.Substring(underscoreIndex + 1);
                if (int.TryParse(numberString, out int frameIndex))
                {
                    // Show the exact same frame number from the outfit array
                    if (frameIndex >= 0 && frameIndex < outfitSprites.Length)
                    {
                        _myRenderer.sprite = outfitSprites[frameIndex];
                    }
                }
            }
        }
    }
}