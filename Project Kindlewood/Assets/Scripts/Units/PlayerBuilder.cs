namespace Units
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Economy;

    [RequireComponent(typeof(PlayerController))]
    public class PlayerBuilder : MonoBehaviour
    {
        [Header("Campfire Settings")]
        [SerializeField] private GameObject campfirePrefab;
        [SerializeField] private string requiredResource = "Wood";
        [SerializeField] private int woodCost = 5;

        [Header("Placement Settings")]
        [SerializeField] private float buildOffsetDistance = 1.0f;
        [SerializeField] private float placementCheckRadius = 0.4f;
        [Tooltip("Layers that CANNOT be built on (e.g., trees, walls, resource box)")]
        [SerializeField] private LayerMask obstacleLayer = ~0;
        [SerializeField] private Transform buildingsParent;

        [Header("Storage Reference")]
        [SerializeField] private ResourceBox resourceBox;

        private PlayerController _playerController;
        private readonly Collider2D[] _overlapBuffer = new Collider2D[5];
        private ContactFilter2D _obstacleFilter;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();

            if (!resourceBox)
            {
                resourceBox = FindAnyObjectByType<ResourceBox>();
            }

            _obstacleFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = obstacleLayer,
                useTriggers = false
            };
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                TryBuildCampfire();
            }
        }

        private Vector2 GetBuildPosition()
        {
            Vector2 facingDir = _playerController != null ? _playerController.LastMoveDirection : Vector2.down;
            return (Vector2)transform.position + (facingDir * buildOffsetDistance);
        }

        private void TryBuildCampfire()
        {
            if (!campfirePrefab)
            {
                Debug.LogWarning("[PlayerBuilder] Campfire Prefab is missing in the Inspector!");
                return;
            }

            if (!resourceBox)
            {
                Debug.LogWarning("[PlayerBuilder] ResourceBox is missing from the scene!");
                return;
            }

            Vector2 buildPos = GetBuildPosition();

            // 1. Check that there are no obstacles in front
            int hitCount = Physics2D.OverlapCircle(buildPos, placementCheckRadius, _obstacleFilter, _overlapBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_overlapBuffer[i] != null && _overlapBuffer[i].gameObject != gameObject)
                {
                    Debug.Log($"[PlayerBuilder] Build location is blocked! In the way: {_overlapBuffer[i].name}");
                    return;
                }
            }

            // 2. Try to spend resources directly from the ResourceBox
            if (!resourceBox.TrySpendResources(requiredResource, woodCost))
            {
                Debug.Log($"[PlayerBuilder] Not enough resources ({requiredResource}) in the ResourceBox! Need {woodCost}.");
                return;
            }

            // 3. Build the campfire!
            Instantiate(campfirePrefab, buildPos, Quaternion.identity, buildingsParent);
            Debug.Log($"[PlayerBuilder] Campfire built! Spent {woodCost}x {requiredResource} from the ResourceBox.");
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 facingDir = _playerController != null ? _playerController.LastMoveDirection : Vector2.down;
            Vector2 buildPos = (Vector2)transform.position + (facingDir * buildOffsetDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(buildPos, placementCheckRadius);
        }
    }
}