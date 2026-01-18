using UnityEngine;

[RequireComponent(typeof(BallMovement))]
public class BotWanderSafeZone2D : MonoBehaviour
{
    [Header("Safe Zone")]
    [SerializeField] private Transform centerTransform;   // centro de la arena (Botcenter)
    [SerializeField] private float safeRadius = 4f;       // zona segura
    [SerializeField] private float edgeBuffer = 0.6f;     // antes del borde empieza a corregir

    [Header("Wander")]
    [SerializeField] private float repickMinSeconds = 0.25f;
    [SerializeField] private float repickMaxSeconds = 0.9f;
    [SerializeField] private float jitter = 0.35f;        // “random” extra en steering
    [SerializeField, Range(0f, 1f)] private float centerPullNearEdge = 0.85f; // cuanto tira al centro si está cerca del borde

    [Header("Last Bot Mode")]
    [Tooltip("Si queda 1 solo bot, evita salir de la zona segura: fuerza hacia el centro cerca del borde.")]
    [SerializeField] private bool lockInSafeZoneWhenLastBot = true;

    [Tooltip("Dentro de la zona, si queda 1 solo bot, se mueve con una orbita interna suave (evita borde).")]
    [SerializeField, Range(0f, 1f)] private float lastBotOrbitStrength = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = false;

    private BallMovement mover;
    private Vector2 currentDir;
    private float nextPickTime;

    public void SetCenter(Transform center) => centerTransform = center;

    private void Awake()
    {
        mover = GetComponent<BallMovement>();
        if (mover == null)
        {
            Debug.LogError("[BotWanderSafeZone2D] Missing BallMovement.", this);
            enabled = false;
            return;
        }

        PickNewDirection(force: true);
    }

    private void FixedUpdate()
    {
        if (centerTransform == null)
        {
            mover.SetMoveInput(Vector2.zero);
            return;
        }

        bool isLastBot = lockInSafeZoneWhenLastBot && IsLastBot();

        Vector2 pos = transform.position;
        Vector2 toCenter = (Vector2)centerTransform.position - pos;
        float dist = toCenter.magnitude;

        float edgeStart = Mathf.Max(0.1f, safeRadius - edgeBuffer);

        // 1) Si estoy fuera: volver al centro
        if (dist >= safeRadius)
        {
            Vector2 dirToCenter = (dist > 0.001f) ? (toCenter / dist) : Vector2.zero;

            // Si es el último bot, fuerza TOTAL al centro (sin blend ni random)
            mover.SetMoveInput(isLastBot ? dirToCenter : Vector2.ClampMagnitude(dirToCenter, 1f));
            return;
        }

        // 2) Re-elegir dirección cada X segundos (solo si NO es último bot)
        if (!isLastBot && Time.time >= nextPickTime)
        {
            PickNewDirection(force: false);
        }

        // 3) Si estoy cerca del borde
        if (dist >= edgeStart)
        {
            Vector2 centerDir = (dist > 0.001f) ? (toCenter / dist) : Vector2.zero;

            if (isLastBot)
            {
                // Último bot: NO se negocia el centro
                mover.SetMoveInput(centerDir);
                return;
            }

            // Normal: mezclar dirección con pull al centro
            float t = Mathf.InverseLerp(edgeStart, safeRadius, dist); // 0..1
            float pull = Mathf.Lerp(0.15f, centerPullNearEdge, t);

            Vector2 blended = Vector2.Lerp(currentDir, centerDir, pull);
            mover.SetMoveInput(Vector2.ClampMagnitude(blended, 1f));
            return;
        }

        // 4) Dentro de la zona segura
        if (isLastBot)
        {
            // Orbita interna suave para que no apunte al borde
            if (dist > 0.001f)
            {
                Vector2 tangent = new Vector2(-toCenter.y, toCenter.x).normalized;
                mover.SetMoveInput(Vector2.ClampMagnitude(tangent * Mathf.Clamp01(lastBotOrbitStrength), 1f));
            }
            else
            {
                mover.SetMoveInput(Vector2.zero);
            }
            return;
        }

        // Normal: moverse random
        mover.SetMoveInput(Vector2.ClampMagnitude(currentDir, 1f));
    }

    private void PickNewDirection(bool force)
    {
        // Random dir + jitter
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;

        // Le sumamos jitter para que se vea más errático
        dir += Random.insideUnitCircle * jitter;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;

        currentDir = dir.normalized;

        float wait = Random.Range(repickMinSeconds, repickMaxSeconds);
        nextPickTime = Time.time + wait;

        // opcional: si force, que sea inmediato
        if (force) nextPickTime = Time.time + Random.Range(0.05f, 0.15f);
    }

    private bool IsLastBot()
    {
        // Cuenta bots activos. Unity 6+: FindObjectsByType es lo correcto.
        int count = FindObjectsByType<BotWanderSafeZone2D>(FindObjectsSortMode.None).Length;
        return count <= 1;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (centerTransform == null) return;

        UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.15f);
        UnityEditor.Handles.DrawSolidDisc(centerTransform.position, Vector3.forward, safeRadius);

        UnityEditor.Handles.color = new Color(1f, 1f, 0f, 0.15f);
        UnityEditor.Handles.DrawSolidDisc(centerTransform.position, Vector3.forward, Mathf.Max(0f, safeRadius - edgeBuffer));
    }
#endif
}
