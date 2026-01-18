using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BallMovement : MonoBehaviour
{
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float maxSpeed = 20f;

    // Límite duro absoluto (seguridad)
    [SerializeField] private float hardMaxSpeed = 30f;

    [Header("External Impulse (ExtraPush)")]
    [Tooltip("Tiempo durante el cual NO se aplican clamps tras un impulso externo.")]
    [SerializeField] private float defaultExternalImpulseGrace = 0.12f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Ventana de gracia para que ExtraPush no sea recortado por los clamps
    private float externalImpulseGraceUntil = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void FixedUpdate()
    {
        // 1) Input normalizado (evita velocidad extra en diagonal)
        Vector2 input = Vector2.ClampMagnitude(moveInput, 1f);

        // 2) Aceleración solo si hay input
        if (input.sqrMagnitude > 0.001f)
        {
            rb.AddForce(input * acceleration, ForceMode2D.Force);
        }

        float speed = rb.linearVelocity.magnitude;

        // ¿Estamos en ventana de gracia por impulso externo?
        bool inGrace = Time.time < externalImpulseGraceUntil;

        // 3) Clamp de velocidad máxima mientras el jugador controla
        //    (si hubo impulso externo reciente, no lo recortamos)
        if (!inGrace && input.sqrMagnitude > 0.001f && speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            speed = maxSpeed; // consistente para el siguiente clamp
        }

        // 4) Clamp duro de seguridad (choques extremos)
        //    (si hubo impulso externo reciente, no lo recortamos)
        if (!inGrace && speed > hardMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * hardMaxSpeed;
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Si hay bot, ignorar input humano (seguridad)
        if (TryGetComponent<BotWanderSafeZone2D>(out _)) return;

        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Llamado por ExtraPush (u otros) para permitir que un impulso externo se sienta
    /// sin que el clamp lo mate instantáneamente.
    /// </summary>
    public void NotifyExternalImpulse(float seconds = -1f)
    {
        if (seconds < 0f) seconds = defaultExternalImpulseGrace;
        externalImpulseGraceUntil = Time.time + Mathf.Max(0f, seconds);
    }
}
