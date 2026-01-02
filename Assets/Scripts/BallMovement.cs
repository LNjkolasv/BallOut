using UnityEngine;
using UnityEngine.InputSystem;

public class BallMovement : MonoBehaviour
{
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float maxSpeed = 20f;

    // Opcional: si querés un límite duro absoluto (por seguridad)
    [SerializeField] private float hardMaxSpeed = 30f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void FixedUpdate()
    {
        // Acelerar por input
        rb.AddForce(moveInput * acceleration, ForceMode2D.Force);

        float speed = rb.linearVelocity.magnitude;

        // 1) Clamp SOLO cuando hay input (control del jugador)
        if (moveInput.sqrMagnitude > 0.001f && speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // 2) Clamp duro opcional (evita misiles por choques extremos)
        if (speed > hardMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * hardMaxSpeed;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
