using UnityEngine;

using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D _rb;
    private Vector2 _movementInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
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
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _movementInput * (moveSpeed * Time.fixedDeltaTime));
    }
}

