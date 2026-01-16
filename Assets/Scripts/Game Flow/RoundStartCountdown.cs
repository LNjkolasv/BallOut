using System.Collections;
using UnityEngine;
using TMPro;

public class RoundStartCountdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Countdown")]
    [SerializeField] private int seconds = 3;

    [Header("SFX")]
    [SerializeField] private AudioClip tickSfx;
    [SerializeField] private AudioClip goSfx;
    [SerializeField] private float tickVolume = 0.5f;
    [SerializeField] private float goVolume = 0.6f;

    private void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        // Congelamos gameplay al iniciar el round
        Time.timeScale = 0f;

        for (int i = seconds; i >= 1; i--)
        {
            // Si está pausado por el menú, esperamos
            while (PauseMenuController.IsPaused)
                yield return null;

            if (countdownText) countdownText.text = i.ToString();

            if (AudioManager.I != null && tickSfx != null)
                AudioManager.I.PlaySfx(tickSfx, tickVolume);

            // Espera 1s de tiempo real, pero se frena si el menú pausa
            yield return WaitRealtimePausable(1f);
        }

        while (PauseMenuController.IsPaused)
            yield return null;

        if (countdownText) countdownText.text = "GO!";

        if (AudioManager.I != null && goSfx != null)
            AudioManager.I.PlaySfx(goSfx, goVolume);

        yield return WaitRealtimePausable(0.5f);

        if (countdownText) countdownText.gameObject.SetActive(false);

        // Reanudar gameplay
        Time.timeScale = 1f;
    }

    private IEnumerator WaitRealtimePausable(float seconds)
    {
        float remaining = seconds;

        while (remaining > 0f)
        {
            // Si el menú pausa, no consumimos tiempo
            if (!PauseMenuController.IsPaused)
                remaining -= Time.unscaledDeltaTime;

            yield return null;
        }
    }
}
