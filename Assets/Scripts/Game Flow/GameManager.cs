using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [System.Serializable]
    public class LobbyEntry
    {
        public InputDevice device;
        public int colorIndex;
        public bool ready;
    }

    [Header("Lobby")]
    public List<LobbyEntry> lobby = new();

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===== Lobby API =====

    public bool HasDevice(InputDevice device)
    {
        return lobby.Exists(e => e.device == device);
    }

    public void AddPlayer(InputDevice device, int colorIndex)
    {
        if (HasDevice(device)) return;

        lobby.Add(new LobbyEntry
        {
            device = device,
            colorIndex = colorIndex,
            ready = false
        });
    }

    public void RemovePlayer(InputDevice device)
    {
        lobby.RemoveAll(e => e.device == device);
    }

    public void SetColor(InputDevice device, int colorIndex)
    {
        var entry = lobby.Find(e => e.device == device);
        if (entry != null)
            entry.colorIndex = colorIndex;
    }

    public void ToggleReady(InputDevice device)
    {
        var entry = lobby.Find(e => e.device == device);
        if (entry != null)
            entry.ready = !entry.ready;
    }

    public bool AllReady()
    {
        if (lobby.Count == 0) return false;

        foreach (var e in lobby)
            if (!e.ready)
                return false;

        return true;
    }

    public void ClearLobby()
    {
        lobby.Clear();
    }
}
