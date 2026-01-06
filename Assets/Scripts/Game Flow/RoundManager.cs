using System.Collections;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Restart")]
    [SerializeField] private float restartDelay = 5f;

    private bool restarting;

    void Update()
    {
        if (restarting) return;

        int alivePlayers = CountAlivePlayers();

        if (alivePlayers <= 1)
        {
            StartCoroutine(RestartRoutine());
        }
    }

    private int CountAlivePlayers()
    {
        int count = 0;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            if (p.activeInHierarchy)
                count++;
        }

        return count;
    }

    private IEnumerator RestartRoutine()
    {
        restarting = true;

        Debug.Log("Round ending... restarting soon");

        yield return new WaitForSeconds(restartDelay);

        RestartRound();
    }

    private void RestartRound()
    {
        // Opción A: recargar escena (simple y segura)
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
