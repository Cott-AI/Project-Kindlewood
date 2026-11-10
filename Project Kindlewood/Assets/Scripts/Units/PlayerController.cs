namespace Units
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;

        private Rigidbody2D _rb;
        
        // Changed to a single Animator, since only the Body has one now!
        private Animator _animator;
        
        private Vector2 _movementInput;
        private Vector2 _lastMoveDirection = Vector2.down;
        
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            
            // Grabs the single Animator from the Body child object
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            _movementInput = Vector2.zero;
            if (Keyboard.current == null) return;
            
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) _movementInput.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) _movementInput.y -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) _movementInput.x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) _movementInput.x -= 1f;

            _movementInput = _movementInput.normalized;

            if (_movementInput.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _movementInput;
                UpdateAnimator(_movementInput.x, _movementInput.y, true);
            }
            else
            {
                UpdateAnimator(_lastMoveDirection.x, _lastMoveDirection.y, false);
            }
        }

        private void FixedUpdate()
        {
            _rb.MovePosition(_rb.position + _movementInput * (moveSpeed * Time.fixedDeltaTime));
        }
        
        private void UpdateAnimator(float x, float y, bool isMoving)
        {
            if (_animator == null) return;
            
            _animator.SetFloat(MoveX, x);
            _animator.SetFloat(MoveY, y);
            _animator.SetBool(IsMoving, isMoving);
        }
    }
}