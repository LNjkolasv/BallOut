using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSelectController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference joinAction;

    [Header("Preview")]
    [SerializeField] private GameObject previewBallPrefab;
    [SerializeField] private Transform[] previewSpawns;

    [Header("Colors")]
    [SerializeField]
    private Color[] palette = new Color[]
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, new Color(1f,0.5f,0f)
    };

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "Game";

    // Para encontrar rápido la preview de cada jugador
    private readonly List<GameObject> previews = new();

    // Anti doble-tap (evita que el mismo input dispare 2 joins seguidos por rebote)
    private float lastJoinTime = -999f;
    private const float joinCooldown = 0.15f;

    private void Awake()
    {
        if (joinAction == null)
            Debug.LogError("[PlayerSelectController] joinAction no asignado en Inspector.", this);

        if (previewBallPrefab == null)
            Debug.LogError("[PlayerSelectController] previewBallPrefab no asignado.", this);

        if (previewSpawns == null || previewSpawns.Length == 0)
            Debug.LogError("[PlayerSelectController] previewSpawns está vacío. Creá PreviewSpawn_0..N y asignalos.", this);

        if (startButton == null)
            Debug.LogWarning("[PlayerSelectController] startButton no asignado (el botón Play no se podrá habilitar).", this);
    }

    private void OnEnable()
    {
        if (joinAction != null)
        {
            joinAction.action.Enable();
            joinAction.action.performed += OnJoin;
        }
    }

    private void Start()
    {
        RefreshStart();
    }

    private void OnDisable()
    {
        if (joinAction != null)
        {
            joinAction.action.performed -= OnJoin;
            joinAction.action.Disable();
        }
    }

    private void OnJoin(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // cooldown anti rebote
        if (Time.unscaledTime - lastJoinTime < joinCooldown) return;
        lastJoinTime = Time.unscaledTime;

        if (GameManager.I == null)
        {
            Debug.LogError("[PlayerSelectController] No existe GameManager en la escena (GameManager.I es null).", this);
            RefreshStart();
            return;
        }

        var device = ctx.control.device;

        // Evitar Mouse como jugador
        if (device is Mouse) return;

        // Evitar duplicados
        if (GameManager.I.HasDevice(device)) return;

        int playerIndex = GameManager.I.lobby.Count;
        int colorIndex = playerIndex % palette.Length;

        GameManager.I.AddPlayer(device, colorIndex);

        SpawnPreview(playerIndex, colorIndex);
        RefreshStart();
    }

    private void SpawnPreview(int playerIndex, int colorIndex)
    {
        if (previewBallPrefab == null) return;

        if (previewSpawns == null || previewSpawns.Length == 0)
        {
            Debug.LogError("[PlayerSelectController] No hay previewSpawns asignados.", this);
            return;
        }

        int spawnIndex = Mathf.Min(playerIndex, previewSpawns.Length - 1);
        var spawn = previewSpawns[spawnIndex];

        if (spawn == null)
        {
            Debug.LogError($"[PlayerSelectController] previewSpawns[{spawnIndex}] es null.", this);
            return;
        }

        var go = Instantiate(previewBallPrefab, spawn.position, Quaternion.identity, spawn);

        while (previews.Count <= playerIndex) previews.Add(null);
        previews[playerIndex] = go;

        ApplyPreviewColor(playerIndex, colorIndex);
    }

    private void ApplyPreviewColor(int playerIndex, int colorIndex)
    {
        if (playerIndex < 0 || playerIndex >= previews.Count) return;

        var go = previews[playerIndex];
        if (go == null) return;

        var bc = go.GetComponent<BallColor>();
        if (bc != null)
        {
            int idx = Mathf.Clamp(colorIndex, 0, palette.Length - 1);
            bc.Apply(palette[idx]);
        }
    }

    private void RefreshStart()
    {
        if (startButton == null) return;

        if (GameManager.I == null)
        {
            startButton.interactable = false;
            return;
        }

        startButton.interactable = GameManager.I.lobby.Count > 0;
    }

    public void OnStartGame()
    {
        if (GameManager.I == null)
        {
            Debug.LogError("[PlayerSelectController] No existe GameManager (GameManager.I es null).", this);
            return;
        }

        if (GameManager.I.lobby.Count == 0) return;

        SceneManager.LoadScene(gameSceneName);
    }

    // ====== Para botones de UI por jugador ======
    public void NextColorPlayer(int playerIndex)
    {
        if (GameManager.I == null) return;
        if (playerIndex < 0 || playerIndex >= GameManager.I.lobby.Count) return;

        var entry = GameManager.I.lobby[playerIndex];
        int next = (entry.colorIndex + 1) % palette.Length;

        entry.colorIndex = next;
        ApplyPreviewColor(playerIndex, next);
    }

    public void PrevColorPlayer(int playerIndex)
    {
        if (GameManager.I == null) return;
        if (playerIndex < 0 || playerIndex >= GameManager.I.lobby.Count) return;

        var entry = GameManager.I.lobby[playerIndex];
        int prev = (entry.colorIndex - 1 + palette.Length) % palette.Length;

        entry.colorIndex = prev;
        ApplyPreviewColor(playerIndex, prev);
    }
}
