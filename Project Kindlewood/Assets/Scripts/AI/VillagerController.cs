using AI.States;

namespace AI
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody2D))]
    public class VillagerController : MonoBehaviour
    {
        private StateMachine _stateMachine;

        [Header("Movement & Target References")]
        [SerializeField] private float moveSpeed = 4f;
        
        public Transform CurrentTarget { get; set; }
        public int InventoryCount { get; set; }
        public int MaxInventory { get; set; } = 10;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;

            // Setup state machine
            _stateMachine = new StateMachine();

            // Set States
            _stateMachine.RegisterState(new VillagerIdleState(this));
            _stateMachine.RegisterState(new VillagerGatherState(this));
        }

        private void Start()
        {
            // Set default to idle
            _stateMachine.ChangeState<VillagerIdleState>();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void ChangeState<T>() where T : IState
        {
            _stateMachine.ChangeState<T>();
        }

        public void MoveTowards(Vector2 targetPosition)
        {
            Vector2 currentPos = transform.position;
            Vector2 direction = (targetPosition - currentPos).normalized;

            _rb.MovePosition(_rb.position + direction * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}