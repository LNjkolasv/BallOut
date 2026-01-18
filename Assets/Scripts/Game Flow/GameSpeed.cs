using UnityEngine;
using UnityEngine.InputSystem;

public class GameSpeedWhenBotsOnly : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float normalTimeScale = 1f;
    [SerializeField] private float botsOnlyTimeScale = 2f;

    [Header("Detection")]
    [Tooltip("Cada cuánto recalcula el estado. 0.2s suele sobrar.")]
    [SerializeField] private float checkInterval = 0.2f;

    [Tooltip("Si está ON, también ajusta fixedDeltaTime para mantener física estable.")]
    [SerializeField] private bool scaleFixedDeltaTime = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private float baseFixedDeltaTime;
    private float nextCheckTime;
    private bool isBoosted;

    private void Awake()
    {
        baseFixedDeltaTime = Time.fixedDeltaTime;
        ApplyScale(normalTimeScale);
    }

    private void OnDisable()
    {
        // Seguridad: restaurar al salir / desactivar
        ApplyScale(normalTimeScale);
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime) return;
        nextCheckTime = Time.unscaledTime + Mathf.Max(0.05f, checkInterval);

        EvaluateAndApply();
    }

    private void EvaluateAndApply()
    {
        int aliveHumans = 0;
        int aliveBots = 0;

        // Buscamos todas las bolas que tengan BallMovement (humanos + bots)
        var movers = FindObjectsByType<BallMovement>(FindObjectsSortMode.None);

        foreach (var m in movers)
        {
            if (m == null) continue;

            var go = m.gameObject;

            bool alive = IsAlive(go);
            if (!alive) continue;

            bool isBot = go.GetComponent<BotWanderSafeZone2D>() != null;
            bool isHuman = go.GetComponent<PlayerInput>() != null && !isBot;

            if (isBot) aliveBots++;
            else if (isHuman) aliveHumans++;
        }

        bool shouldBoost = (aliveHumans == 0 && aliveBots > 0);

        if (shouldBoost != isBoosted)
        {
            isBoosted = shouldBoost;
            ApplyScale(isBoosted ? botsOnlyTimeScale : normalTimeScale);

            if (debugLogs)
                Debug.Log($"[GameSpeedWhenBotsOnly] humans={aliveHumans} bots={aliveBots} => timeScale={(isBoosted ? botsOnlyTimeScale : normalTimeScale)}", this);
        }
    }

    private bool IsAlive(GameObject go)
    {
        if (!go.activeInHierarchy) return false;

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null && !rb.simulated) return false;

        // Si hay varios colliders, con que uno esté enabled alcanza
        var col = go.GetComponent<Collider2D>();
        if (col != null && !col.enabled) return false;

        return true;
    }

    private void ApplyScale(float scale)
    {
        scale = Mathf.Max(0f, scale);
        Time.timeScale = scale;

        if (scaleFixedDeltaTime)
            Time.fixedDeltaTime = baseFixedDeltaTime * scale;
    }
}
