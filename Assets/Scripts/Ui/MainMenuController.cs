using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelPlayerSelect;

    [Header("Player Select")]
    [SerializeField] private PlayerSelectController playerSelect;

    private void Awake()
    {
        ShowMain();
    }

    public void OnPlay()
    {
        if (panelMain) panelMain.SetActive(false);
        if (panelPlayerSelect) panelPlayerSelect.SetActive(true);

        // activar escucha de Join
        if (playerSelect) playerSelect.SetActive(true, clearPreviews: true);

        // si querés, limpiá lobby al entrar
        if (GameManager.I != null) GameManager.I.ClearLobby();
    }

    public void OnBackToMain()
    {
        if (panelPlayerSelect) panelPlayerSelect.SetActive(false);
        if (panelMain) panelMain.SetActive(true);

        // desactivar escucha de Join + borrar previews
        if (playerSelect) playerSelect.SetActive(false, clearPreviews: true);

        if (GameManager.I != null)
            GameManager.I.ClearLobby();
    }

    public void OnExit()
    {
        Debug.Log("[MainMenuController] Exit pressed.");

#if UNITY_EDITOR
        // En el Editor no se puede cerrar Unity desde Application.Quit().
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMain()
    {
        if (panelMain) panelMain.SetActive(true);
        if (panelPlayerSelect) panelPlayerSelect.SetActive(false);

        if (playerSelect) playerSelect.SetActive(false, clearPreviews: true);

        if (GameManager.I != null)
            GameManager.I.ClearLobby();
    }
}
