using UnityEngine;
using UnityEngine.InputSystem;

public class GameSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private PlayerInput ballPlayerPrefab; // humanos
    [SerializeField] private GameObject botPrefab;         // bots (con BotBrain2D + BallMovement)

    [Header("Spawns")]
    [SerializeField] private Transform[] arenaSpawns;

    [Header("Arena Center (Scene Object)")]
    [SerializeField] private Transform arenaCenter; // ✅ asignar en Inspector (Game scene)

    [Header("Limits")]
    [SerializeField] private int maxPlayers = 6; // máximo absoluto del juego

    [Header("Colors")]
    [SerializeField]
    private Color[] palette = new Color[]
    {
        Color.red, Color.blue, Color.green, Color.yellow,
        Color.magenta, Color.cyan, new Color(1f,0.5f,0f)
    };

    private void Start()
    {
        if (GameManager.I == null)
        {
            Debug.LogError("[GameSpawner] No existe GameManager.", this);
            return;
        }

        if (ballPlayerPrefab == null)
        {
            Debug.LogError("[GameSpawner] ballPlayerPrefab NO asignado.", this);
            return;
        }

        if (arenaSpawns == null || arenaSpawns.Length == 0)
        {
            Debug.LogError("[GameSpawner] No hay arenaSpawns asignados.", this);
            return;
        }

        // fillTo elegido en el menú (2..6)
        int fillTo = Mathf.Clamp(GameManager.I.FillToPlayers, 2, maxPlayers);

        // capacidad real por escena
        int spawnCapacity = Mathf.Min(arenaSpawns.Length, maxPlayers);

        if (spawnCapacity < fillTo)
        {
            Debug.LogWarning($"[GameSpawner] fillTo={fillTo} pero spawnCapacity={spawnCapacity}. Se completará hasta {spawnCapacity}.", this);
            fillTo = spawnCapacity;
        }

        var lobby = GameManager.I.lobby;

        // Humanos: hasta maxPlayers y capacidad
        int humansToSpawn = Mathf.Min(lobby.Count, maxPlayers, spawnCapacity);

        // Bots: completar hasta fillTo (si faltan)
        int botsToSpawn = Mathf.Clamp(fillTo - humansToSpawn, 0, spawnCapacity - humansToSpawn);

        // ===== 1) Spawn Humanos =====
        for (int i = 0; i < humansToSpawn; i++)
        {
            var entry = lobby[i];
            var spawn = arenaSpawns[i];
            if (spawn == null) continue;

            var pi = PlayerInput.Instantiate(
                ballPlayerPrefab.gameObject,
                controlScheme: null,
                pairWithDevice: entry.device
            );

            pi.transform.position = spawn.position;

            var bc = pi.GetComponent<BallColor>();
            if (bc != null)
            {
                int idx = Mathf.Clamp(entry.colorIndex, 0, palette.Length - 1);
                bc.Apply(palette[idx]);
            }
        }

        // ===== 2) Spawn Bots =====
        if (botsToSpawn > 0 && botPrefab == null)
        {
            Debug.LogWarning($"[GameSpawner] Faltan {botsToSpawn} bots para completar hasta {fillTo}, pero botPrefab no está asignado.", this);
        }
        else
        {
            for (int b = 0; b < botsToSpawn; b++)
            {
                int spawnIndex = humansToSpawn + b;
                var spawn = arenaSpawns[spawnIndex];
                if (spawn == null) continue;

                var bot = Instantiate(botPrefab, spawn.position, Quaternion.identity);

                // ✅ Asignar centro al bot (para que no se suicide)
                var brain = bot.GetComponent<BotWanderSafeZone2D>();

                if (brain != null)
                {
                    if (arenaCenter == null)
                        Debug.LogWarning("[GameSpawner] arenaCenter no asignado. Bots sin ancla al centro.", this);
                    else
                        brain.SetCenter(arenaCenter);
                }

                // Color bot
                var bc = bot.GetComponent<BallColor>();
                if (bc != null)
                {
                    int colorIdx = (humansToSpawn + b) % palette.Length;
                    bc.Apply(palette[colorIdx]);
                }
            }
        }

        Debug.Log($"[GameSpawner] fillTo={fillTo}, Humans={humansToSpawn}, Bots={botsToSpawn}, SpawnCapacity={spawnCapacity}", this);
    }
}
