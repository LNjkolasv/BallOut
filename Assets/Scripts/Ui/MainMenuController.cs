using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelPlayerSelect;

    private void Awake()
    {
        // Estado inicial seguro aunque Start no llegue por errores
        ShowMain();
    }

    public void OnPlay()
    {
        if (panelMain) panelMain.SetActive(false);
        if (panelPlayerSelect) panelPlayerSelect.SetActive(true);

        // Limpia lobby solo si existe GameManager
        if (GameManager.I != null)
            GameManager.I.ClearLobby();
    }

    public void OnBackToMain()
    {
        if (panelPlayerSelect) panelPlayerSelect.SetActive(false);
        if (panelMain) panelMain.SetActive(true);

        if (GameManager.I != null)
            GameManager.I.ClearLobby();
    }

    public void OnExit()
    {
        Application.Quit();
    }

    private void ShowMain()
    {
        if (panelMain) panelMain.SetActive(true);
        if (panelPlayerSelect) panelPlayerSelect.SetActive(false);
    }
}
