using AI.States;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class VillagerController : MonoBehaviour
    {
        private StateMachine _stateMachine;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        
        [Header("Movement & Target References")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float stoppingDistance = 1f;
        
        [field: SerializeField] public Transform CurrentTarget { get; set; }
        public float StoppingDistance => stoppingDistance;
        public int InventoryCount { get; set; }
        public int MaxInventory { get; set; } = 10;
        public string CarriedResourceType { get; set; } = "Wood";

        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _stateMachine = new StateMachine();
            _stateMachine.RegisterState(new VillagerIdleState(this));
            _stateMachine.RegisterState(new VillagerGatherState(this));
            _stateMachine.RegisterState(new VillagerReturnState(this));
        }

        private void Start()
        {
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

        public bool IsInRange(Vector2 targetPosition)
        {
            return Vector2.Distance(transform.position, targetPosition) <= stoppingDistance;
        }

        public void StopMoving()
        {
            _animator.SetBool(IsMoving, false);
            _rb.linearVelocity = Vector2.zero;
        }

        public void MoveTowards(Vector2 targetPosition)
        {
            if (IsInRange(targetPosition))
            {
                StopMoving();
                return;
            }

            Vector2 currentPos = transform.position;
            Vector2 direction = (targetPosition - currentPos).normalized;
            
            _animator.SetFloat(MoveX, direction.x);
            _animator.SetFloat(MoveY, direction.y);
            _animator.SetBool(IsMoving, direction.magnitude > 0.01f);

            if (Mathf.Abs(direction.x) > 0.01f)
            {
                _spriteRenderer.flipX = direction.x < 0;
            }
            
            _rb.MovePosition(_rb.position + direction * (moveSpeed * Time.deltaTime));
        }
    }
}