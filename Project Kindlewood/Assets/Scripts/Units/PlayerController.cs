namespace Units
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;

        private Rigidbody2D _rb;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        
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
            
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
                
                _animator.SetFloat(MoveX, _movementInput.x);
                _animator.SetFloat(MoveY, _movementInput.y);
                _animator.SetBool(IsMoving, true);

                if (_movementInput.x > 0.01f)
                {
                    _spriteRenderer.flipX = false;
                }
                else if (_movementInput.x < -0.01f)
                {
                    _spriteRenderer.flipX = true;
                }
            }
            else
            {
                _animator.SetFloat(MoveX, _lastMoveDirection.x);
                _animator.SetFloat(MoveY, _lastMoveDirection.y);
                _animator.SetBool(IsMoving, false);
            }
        }

        private void FixedUpdate()
        {
            _rb.MovePosition(_rb.position + _movementInput * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}