using UnityEngine;

namespace AI
{
    public class ResourceNode : MonoBehaviour
    {
        [Header("Resource Settings")] 
        [SerializeField] private string resourceType = "Wood";
        [SerializeField] private int maxResources = 20;

        private int _currentResources;
        
        [Header("Visual settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite depletedSprite;
        [SerializeField] private Animator animator;

        private bool _isDepleted;
        private static readonly int HitTrigger = Animator.StringToHash("Hit");

        // Trim() to make sure new resource isn't made when depositing into Resource Box
        public string GetResourceType() => resourceType.Trim();

        public bool IsAvailable() => !_isDepleted;

        private void Awake()
        {
            _currentResources = maxResources;

            if (!spriteRenderer)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (!animator)
            {
                animator = GetComponent<Animator>();
            }
        }

        // Returns how many resources was actually collected
        public int Gather(int amount = 1)
        {
            if (_isDepleted || amount <= 0) return 0;

            int gatheredAmount = Mathf.Min(amount, _currentResources);
            _currentResources -= gatheredAmount;

            if (animator)
            {
                animator.SetTrigger(HitTrigger);
            }
            
            if (_currentResources <= 0)
            {
                DepleteNode();
            }

            return gatheredAmount;
        }

        private void DepleteNode()
        {
            _isDepleted = true;

            // Disable animator, so it doesn't overwrite depletedSprite image
            if (animator)
            {
                animator.enabled = false;
            }
            
            // Swap to empty resource node sprite (esim. kanto)
            if (depletedSprite && spriteRenderer)
            {
                spriteRenderer.sprite = depletedSprite;
            }
            
            // Remove collider, so villager doesn't try to gather empty node
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.enabled = false;
            }
        }
    }
}