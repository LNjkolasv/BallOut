using UnityEngine;
using UnityEngine.InputSystem;

public class GameSpawner : MonoBehaviour
{
    [SerializeField] private PlayerInput ballPlayerPrefab;
    [SerializeField] private Transform[] arenaSpawns;

    [SerializeField]
    private Color[] palette = new Color[]
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, new Color(1f,0.5f,0f)
    };

    private void Start()
    {
        var lobby = GameManager.I.lobby;

        for (int i = 0; i < lobby.Count; i++)
        {
            var entry = lobby[i];
            var spawn = arenaSpawns[Mathf.Min(i, arenaSpawns.Length - 1)];

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
    }
}
