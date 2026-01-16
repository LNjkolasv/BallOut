using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerSelectController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference joinAction;

    [Header("Limits")]
    [SerializeField] private int maxPlayers = 6;

    [Header("Bots Fill-To UI")]
    [SerializeField] private TMP_Text fillToText;
    [SerializeField] private int minFillTo = 2;
    [SerializeField] private int maxFillTo = 6;

    [Header("Preview")]
    [SerializeField] private GameObject previewBallPrefab;
    [SerializeField] private Transform[] previewSpawns;

    [Header("Colors")]
    [SerializeField]
    private Color[] palette =
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, new Color(1f,0.5f,0f)
    };

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "Game";

    private readonly List<GameObject> previews = new();
    private bool isActive = false;

    private void Awake()
    {
        SetActive(false, clearPreviews: true);
    }

    private void Start()
    {
        RefreshFillToLabel();
        RefreshStart();
    }

    private void OnDestroy()
    {
        UnhookInput();
    }

    // === LLAMAR DESDE EL MENÚ ===
    public void SetActive(bool active, bool clearPreviews)
    {
        isActive = active;

        if (active)
        {
            HookInput();
            RefreshFillToLabel();
            RefreshStart();
        }
        else
        {
            UnhookInput();
            if (clearPreviews)
                ClearPreviews();
            RefreshStart();
        }
    }

    private void HookInput()
    {
        if (joinAction == null) return;

        joinAction.action.Enable();
        joinAction.action.performed -= OnJoin;
        joinAction.action.performed += OnJoin;
    }

    private void UnhookInput()
    {
        if (joinAction == null) return;

        joinAction.action.performed -= OnJoin;
        joinAction.action.Disable();
    }

    private void OnJoin(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        if (GameManager.I == null) return;

        // 🔒 Límite duro de humanos
        if (GameManager.I.lobby.Count >= maxPlayers)
        {
            Debug.Log("[PlayerSelect] Max players reached (6).");
            return;
        }

        var device = ctx.control.device;
        if (device is Mouse) return;

        // evita duplicados (un jugador por device)
        if (GameManager.I.HasDevice(device)) return;

        int index = GameManager.I.lobby.Count;
        int colorIndex = index % palette.Length;

        GameManager.I.AddPlayer(device, colorIndex);

        SpawnPreview(index, colorIndex);
        RefreshStart();
    }

    // ✅ Botón UI: Completar hasta X
    public void OnToggleFillTo()
    {
        if (GameManager.I == null) return;

        int current = GameManager.I.FillToPlayers;
        int next = current + 1;
        if (next > maxFillTo) next = minFillTo;

        GameManager.I.SetFillToPlayers(next);
        RefreshFillToLabel();
    }

    private void RefreshFillToLabel()
    {
        if (fillToText == null) return;

        int value = (GameManager.I != null) ? GameManager.I.FillToPlayers : 2;
        fillToText.text = $"Fill with bots up to {value} players";
    }


    private void SpawnPreview(int playerIndex, int colorIndex)
    {
        if (previewBallPrefab == null)
        {
            Debug.LogWarning("[PlayerSelect] previewBallPrefab no asignado.");
            return;
        }

        if (previewSpawns == null || previewSpawns.Length == 0)
        {
            Debug.LogWarning("[PlayerSelect] No hay previewSpawns asignados.");
            return;
        }

        if (playerIndex >= previewSpawns.Length)
        {
            Debug.LogWarning($"[PlayerSelect] Faltan previewSpawns: players={playerIndex + 1}, previewSpawns={previewSpawns.Length}. No se crea preview.");
            return;
        }

        var spawn = previewSpawns[playerIndex];
        if (spawn == null) return;

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

    private void ClearPreviews()
    {
        for (int i = 0; i < previews.Count; i++)
        {
            if (previews[i] != null)
                Destroy(previews[i]);
        }
        previews.Clear();
    }

    private void RefreshStart()
    {
        if (startButton == null) return;
        if (GameManager.I == null) { startButton.interactable = false; return; }

        startButton.interactable = isActive && GameManager.I.lobby.Count > 0;
    }

    public void OnStartGame()
    {
        if (!isActive) return;
        if (GameManager.I == null) return;
        if (GameManager.I.lobby.Count == 0) return;

        SceneManager.LoadScene(gameSceneName);
    }
}
