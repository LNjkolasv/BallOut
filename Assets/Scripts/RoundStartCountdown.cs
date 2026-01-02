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

    void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        Time.timeScale = 0f;

        for (int i = seconds; i >= 1; i--)
        {
            if (countdownText) countdownText.text = i.ToString();

            if (AudioManager.I != null && tickSfx != null)
                AudioManager.I.PlaySfx(tickSfx, tickVolume);

            yield return new WaitForSecondsRealtime(1f);
        }

        if (countdownText) countdownText.text = "GO!";

        if (AudioManager.I != null && goSfx != null)
            AudioManager.I.PlaySfx(goSfx, goVolume);

        yield return new WaitForSecondsRealtime(0.5f);

        if (countdownText) countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
