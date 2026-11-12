namespace Units
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using AI;
    using Economy;

    [RequireComponent(typeof(PlayerController))]
    public class PlayerResourceCollector : MonoBehaviour
    {
        [Header("Gathering Settings")] [Tooltip("How far does player reach.")] 
        [SerializeField] private float gatherOffsetDistance = 0.6f;

        [Tooltip("Gather radius")] 
        [SerializeField] private float gatherRadius = 0.6f;

        [SerializeField] private int gatherAmountPerHit = 1;
        [SerializeField] private float gatherCooldown = 0.4f;
        [SerializeField] private LayerMask resourceLayer = ~0;

        [Header("Inventory & Storage Settings")]
        [Tooltip("If on, collected resources go directly to Resource Box")]
        [SerializeField] private bool depositDirectlyToBox;

        [SerializeField] private int maxInventoryCapacity = 20;
        [SerializeField] private ResourceBox resourceBox;

        [Header("Animation")] 
        [SerializeField] private string gatherTriggerName = "Gather";
        
        private PlayerController _playerController;
        private readonly Dictionary<string, int> _playerInventory = new Dictionary<string, int>();
        
        private readonly Collider2D[] _hitBuffer = new Collider2D[10];
        private ContactFilter2D _contactFilter;
        
        private int _currentTotalCarried;
        private float _cooldownTimer;
        private int _gatherTriggerHash;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();

            if (!resourceBox)
            {
                resourceBox = FindAnyObjectByType<ResourceBox>();
            }
            
            if (!string.IsNullOrEmpty(gatherTriggerName))
            {
                _gatherTriggerHash = Animator.StringToHash(gatherTriggerName);
                
            }

            _contactFilter = new ContactFilter2D()
            {
                useLayerMask = true,
                layerMask = resourceLayer,
                useTriggers = true
            };
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            if (WasGatherPressed() && _cooldownTimer <= 0f)
            {
                TryGatherInFront();
            }
        }

        private bool WasGatherPressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                return true;
            return false;
        }

        private Vector2 GetGatherCenter()
        {
            Vector2 facingDir = _playerController != null ? _playerController.LastMoveDirection : Vector2.down;
            return (Vector2)transform.position + (facingDir * gatherOffsetDistance);
        }

        private void TryGatherInFront()
        {
            if (!depositDirectlyToBox && _currentTotalCarried >= maxInventoryCapacity)
            {
                Debug.Log("[Player] Bag if full, return resources to Resource Box.");
                return;
            }
            Vector2 checkPos = GetGatherCenter();
            
            int hitCount = Physics2D.OverlapCircle(checkPos, gatherRadius, _contactFilter, _hitBuffer);

            ResourceNode closestNode = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = _hitBuffer[i];
                if (hit != null && hit.TryGetComponent<ResourceNode>(out var node) && node.IsAvailable())
                {
                    float dist = Vector2.Distance(checkPos, node.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestNode = node;
                    }
                }
            }

            if (closestNode != null)
            {
                _cooldownTimer = gatherCooldown;

                if (!string.IsNullOrEmpty(gatherTriggerName))
                {
                    _playerController.TriggerAnimation(_gatherTriggerHash);
                }
                
                int spaceLeft = depositDirectlyToBox ? gatherAmountPerHit : (maxInventoryCapacity - _currentTotalCarried);
                int amountToGather = Mathf.Min(gatherAmountPerHit, spaceLeft);
                
                int gathered = closestNode.Gather(amountToGather);
                if (gathered > 0)
                {
                    string resType = closestNode.GetResourceType();
                    AddResourceToPlayer(resType, gathered);
                }
            }
        }

        private void AddResourceToPlayer(string resourceType, int amount)
        {
            if (depositDirectlyToBox && resourceBox != null)
            {
                resourceBox.DepositResources(resourceType, amount);
                Debug.Log("[Player] Added resource to box: " + resourceType);
                return;
            }

            if (_playerInventory.TryAdd(resourceType, amount))
            {
                _playerInventory[resourceType] += amount;
            }

            _currentTotalCarried += amount;
            Debug.Log("[Player] Added resource to player: " + resourceType);
        }

        private void DepositAllToBox(ResourceBox targetBox)
        {
            if (targetBox == null || _currentTotalCarried <= 0) return;

            foreach (var kvp in _playerInventory)
            {
                if (kvp.Value > 0)
                {
                    targetBox.DepositResources(kvp.Key, kvp.Value);
                }
            }
            
            _playerInventory.Clear();
            _currentTotalCarried = 0;
            Debug.Log("[Player] Deposited all to box: " + targetBox);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (depositDirectlyToBox || _currentTotalCarried <= 0) return;

            if (other.TryGetComponent<ResourceBox>(out var box))
            {
                DepositAllToBox(box);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 facingDir = _playerController != null ? _playerController.LastMoveDirection : Vector2.down;
            Vector2 checkPos = (Vector2)transform.position + (facingDir * gatherOffsetDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkPos, gatherRadius);
        }
        
    }
}