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
        // 1) Input normalizado (evita velocidad extra en diagonal)
        Vector2 input = Vector2.ClampMagnitude(moveInput, 1f);

        // 2) Aceleración solo si hay input
        if (input.sqrMagnitude > 0.001f)
        {
            rb.AddForce(input * acceleration, ForceMode2D.Force);
        }

        float speed = rb.linearVelocity.magnitude;

        // 3) Clamp de velocidad máxima mientras el jugador controla
        if (input.sqrMagnitude > 0.001f && speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // 4) Clamp duro de seguridad (choques extremos)
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
