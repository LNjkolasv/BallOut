using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Input (optional)")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "Menu";

    private bool isPaused;
    private float prevTimeScale = 1f;

    // ✅ evita doble toggle en el mismo frame
    private int lastToggleFrame = -1;

    private void Awake()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        isPaused = false;
        IsPaused = false;
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void Update()
    {
        // ✅ Si el InputAction está habilitado, NO usamos fallback
        // porque si no, se togglearía 2 veces (action + fallback).
        if (pauseAction != null && pauseAction.action != null && pauseAction.action.enabled)
            return;

        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        if (esc || start)
            TogglePause();
    }

    private void OnPausePerformed(InputAction.CallbackContext _)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        // ✅ gate por frame: aunque lleguen 2 triggers, solo uno aplica
        if (Time.frameCount == lastToggleFrame) return;
        lastToggleFrame = Time.frameCount;

        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (pauseMenuRoot == null) return;
        if (isPaused) return;

        prevTimeScale = Time.timeScale;

        pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f;

        isPaused = true;
        IsPaused = true;
    }

    public void Resume()
    {
        if (!isPaused) return;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        Time.timeScale = prevTimeScale;

        isPaused = false;
        IsPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        IsPaused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        IsPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
