using UnityEngine;

[RequireComponent(typeof(BallMovement))]
public class BotBrain2D : MonoBehaviour
{
    private enum BotState { GoToCenter, HoldCenter, Attack }

    [Header("Refs")]
    [SerializeField] private Transform centerTransform;

    [Header("Targeting")]
    [SerializeField] private float retargetInterval = 0.25f;
    [SerializeField] private float maxTargetDistance = 999f;

    [Header("Cycle")]
    [SerializeField] private float attackSeconds = 0.9f;
    [SerializeField] private float retreatSeconds = 0.35f; // obligado: rompe órbitas
    [SerializeField] private float centerHoldSeconds = 0.15f;

    [Header("Center")]
    [SerializeField] private float centerPassRadius = 1.2f;
    [SerializeField] private float centerHoldRadius = 0.55f;

    [Header("Safety")]
    [SerializeField] private float leashRadius = 4.2f;           // si te alejás mucho -> volver
    [SerializeField] private float disengageDistance = 1.8f;     // si estás muy pegado -> cortar

    [Header("Steering")]
    [SerializeField] private float centerExtraPull = 0.35f;
    [SerializeField] private float jitter = 0.0f; // dejalo 0 por ahora

    private BallMovement mover;
    private Transform target;
    private float nextRetargetTime;

    private BotState state = BotState.GoToCenter;
    private float stateUntilTime;
    private float retreatUntilTime;

    public void SetCenter(Transform center) => centerTransform = center;

    private void Awake()
    {
        mover = GetComponent<BallMovement>();
        if (mover == null)
        {
            Debug.LogError("[BotBrain2D] Missing BallMovement.", this);
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (mover == null) return;

        if (centerTransform == null)
        {
            // No centro = no IA
            mover.SetMoveInput(Vector2.zero);
            return;
        }

        if (Time.time >= nextRetargetTime)
        {
            target = FindClosestTarget();
            nextRetargetTime = Time.time + retargetInterval;
        }

        mover.SetMoveInput(ComputeInput());
    }

    private Vector2 ComputeInput()
    {
        Vector2 pos = transform.position;

        Vector2 toCenterVec = (Vector2)centerTransform.position - pos;
        float distToCenter = toCenterVec.magnitude;
        Vector2 toCenter = (distToCenter > 0.001f) ? (toCenterVec / distToCenter) : Vector2.zero;

        // Si está en modo retirada forzada, IGNORAR TODO y volver al centro
        if (Time.time < retreatUntilTime)
        {
            if (distToCenter <= centerHoldRadius) return Vector2.zero;
            return Vector2.ClampMagnitude(toCenter, 1f);
        }

        // Sin target => volver al centro
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            state = BotState.GoToCenter;
            if (distToCenter <= centerHoldRadius) return Vector2.zero;
            return Vector2.ClampMagnitude(toCenter, 1f);
        }

        Vector2 toTargetVec = (Vector2)target.position - (Vector2)transform.position;
        float distToTarget = toTargetVec.magnitude;
        Vector2 toTarget = (distToTarget > 0.001f) ? (toTargetVec / distToTarget) : Vector2.zero;

        // Seguridad: si te vas mucho del centro -> reset
        if (distToCenter > leashRadius)
        {
            ForceRetreat();
            return Vector2.ClampMagnitude(toCenter, 1f);
        }

        // Anti-orbit: si estás muy pegado, cortar persecución y volver al centro sí o sí
        if (distToTarget <= disengageDistance)
        {
            ForceRetreat();
            return Vector2.ClampMagnitude(toCenter, 1f);
        }

        switch (state)
        {
            case BotState.GoToCenter:
                {
                    if (distToCenter <= centerPassRadius)
                    {
                        state = BotState.HoldCenter;
                        stateUntilTime = Time.time + centerHoldSeconds;
                    }
                    return Vector2.ClampMagnitude(toCenter, 1f);
                }

            case BotState.HoldCenter:
                {
                    if (distToCenter <= centerHoldRadius)
                    {
                        if (Time.time >= stateUntilTime)
                        {
                            state = BotState.Attack;
                            stateUntilTime = Time.time + attackSeconds;
                        }
                        return Vector2.zero;
                    }
                    return Vector2.ClampMagnitude(toCenter, 1f);
                }

            case BotState.Attack:
                {
                    if (Time.time >= stateUntilTime)
                    {
                        ForceRetreat();
                        return Vector2.ClampMagnitude(toCenter, 1f);
                    }

                    // Ataque con ancla al centro
                    Vector2 input =
                        (toTarget * 0.75f) +
                        (toCenter * centerExtraPull) +
                        (Random.insideUnitCircle * jitter);

                    return Vector2.ClampMagnitude(input, 1f);
                }
        }

        return Vector2.ClampMagnitude(toCenter, 1f);
    }

    private void ForceRetreat()
    {
        // Retirada obligatoria que rompe órbitas (clave)
        retreatUntilTime = Time.time + retreatSeconds;
        state = BotState.GoToCenter;
    }

    private Transform FindClosestTarget()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        Transform best = null;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < players.Length; i++)
        {
            var go = players[i];
            if (go == null) continue;
            if (go == gameObject) continue;
            if (!go.activeInHierarchy) continue;

            float d = Vector2.Distance(transform.position, go.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = go.transform;
            }
        }

        return best;
    }
}
