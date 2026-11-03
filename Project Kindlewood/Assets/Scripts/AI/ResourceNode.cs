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
        [SerializeField] private Animator animator;

        private bool _isDepleted;
        private static readonly int HitTrigger = Animator.StringToHash("Hit");

        private void Awake()
        {
            _currentResources =  maxResources;

            if (!spriteRenderer)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
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

            // Poistettu huutomerkki (!), jotta tämä suoritetaan kun Animator on olemassa
            if (animator)
            {
                Debug.Log($"[ResourceNode] Laukaistaan Hit-animaatio Animatorille.");
                animator.SetTrigger(HitTrigger);
            }
            
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
            if (depletedSprite && spriteRenderer)
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
