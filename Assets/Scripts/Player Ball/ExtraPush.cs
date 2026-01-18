using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class ExtraPush : MonoBehaviour
{
    [Header("Who gets pushed")]
    [Tooltip("Si está activo, solo empuja objetos que tengan BallMovement.")]
    [SerializeField] private bool onlyPushObjectsWithBallMovement = true;

    [Header("Impulse")]
    [Tooltip("Multiplicador del impacto. 0.5 = suave, 2 = fuerte.")]
    [SerializeField] private float impulseMultiplier = 0.5f;

    [Tooltip("Impacto mínimo para aplicar empuje (bajalo a 1-2 para test).")]
    [SerializeField] private float minImpact = 5f;

    [Tooltip("Límite duro del impulso extra.")]
    [SerializeField] private float maxImpulse = 10f;

    [Tooltip("Cooldown para evitar múltiples empujes en un mismo choque.")]
    [SerializeField] private float cooldown = 0.08f;

    [Header("BallMovement clamp bypass (optional)")]
    [Tooltip("Si el objetivo tiene BallMovement, le da una ventana corta para que el clamp no mate el impulso.")]
    [SerializeField] private bool notifyBallMovement = true;

    [SerializeField] private float notifyGraceSeconds = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Rigidbody2D selfRb;
    private float lastPushTime;

    private void Awake()
    {
        selfRb = GetComponent<Rigidbody2D>();
        if (selfRb == null)
        {
            Debug.LogError("[ExtraPush] Missing Rigidbody2D.", this);
            enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Log de entrada: si no aparece, OnCollision no está pasando
        if (debugLogs)
            Debug.Log($"[ExtraPush] HIT with '{col.collider.name}' (root: {col.collider.transform.root.name})", this);

        if (Time.time - lastPushTime < cooldown) return;

        // Buscar el Rigidbody2D del otro de forma robusta:
        // 1) col.rigidbody (si el collider pertenece al RB)
        // 2) GetComponentInParent<Rigidbody2D> (si chocaste un collider hijo)
        Rigidbody2D otherRb = col.rigidbody != null
            ? col.rigidbody
            : col.collider.GetComponentInParent<Rigidbody2D>();

        if (otherRb == null)
        {
            if (debugLogs)
                Debug.Log("[ExtraPush] No Rigidbody2D found on the other collider/parents. (Probably static collider)", this);
            return;
        }

        if (otherRb == selfRb) return;

        // Filtro opcional: solo empujar "bolas" (BallMovement)
        BallMovement otherMovement = otherRb.GetComponent<BallMovement>();
        if (onlyPushObjectsWithBallMovement && otherMovement == null)
        {
            if (debugLogs)
                Debug.Log($"[ExtraPush] Skipped: other has no BallMovement ({otherRb.name})", this);
            return;
        }

        float impact = col.relativeVelocity.magnitude;
        if (debugLogs)
            Debug.Log($"[ExtraPush] impact={impact:0.00} minImpact={minImpact:0.00}", this);

        if (impact < minImpact) return;

        // Dirección: empujar AL OTRO alejándolo de mí
        Vector2 dir = (otherRb.position - selfRb.position);
        if (dir.sqrMagnitude < 0.0001f)
        {
            // fallback: usar normal del contacto
            dir = -col.GetContact(0).normal;
        }
        dir.Normalize();

        // Impulso: proporcional al impacto
        float impulse = Mathf.Clamp(impact * impulseMultiplier, 0f, maxImpulse);

        // Aplicar en punto de contacto para que se "sienta"
        Vector2 contactPoint = col.GetContact(0).point;
        otherRb.AddForceAtPosition(dir * impulse, contactPoint, ForceMode2D.Impulse);

        if (debugLogs)
            Debug.Log($"[ExtraPush] PUSHED '{otherRb.name}' dir={dir} impulse={impulse:0.00} otherSpeedNow={otherRb.linearVelocity.magnitude:0.00}", this);

        // Opcional: avisar a BallMovement para que no lo clampée instantáneamente
        if (notifyBallMovement && otherMovement != null)
        {
            // Si tu BallMovement todavía no tiene esto, abajo te dejo el add mínimo.
            otherMovement.NotifyExternalImpulse(notifyGraceSeconds);
        }

        lastPushTime = Time.time;
    }
}
