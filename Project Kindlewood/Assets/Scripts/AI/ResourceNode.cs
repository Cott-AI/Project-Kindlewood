using UnityEngine;

namespace AI
{
    public class ResourceNode : MonoBehaviour
    {
        [Header("Resource settings")] 
        [SerializeField] private int maxResources = 20;

        private int _currentResources;
        
        [Header("Visual settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite depletedSprite;

        private bool _isDepleted;

        private void Awake()
        {
            _currentResources =  maxResources;

            if (!spriteRenderer)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public bool IsAvailable()
        {
            return !_isDepleted;
        }

        public void Gather(int amount = 1)
        {
            if (_isDepleted) return;

            _currentResources -= amount;
            // If node runs out of resource, run the depletion func
            if (_currentResources <= 0)
            {
                DepleteNode();
            }
        }

        private void DepleteNode()
        {
            _isDepleted = true;
            
            // Swap to empty resource node sprite
            if (depletedSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = depletedSprite;
            }
            
            // remove collider, so villager doesn't try to gather empty node
            var col = GetComponent<Collider2D>();
            if (col)
            {
                col.enabled = false;
            }
        }
    }
}
