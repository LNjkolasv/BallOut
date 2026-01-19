using UnityEngine;

public class KillZone2D : MonoBehaviour
{
    [Header("Rare random scream (1 in N)")]
    [Tooltip("Sonidos raros que pueden salir al caer (ej: Kirby, Miley, etc.)")]
    [SerializeField] private AudioClip[] rareExitSounds;

    [Tooltip("Probabilidad 1 en N")]
    [SerializeField] private int rareChance = 100;

    [SerializeField] private float rareScreamVolume = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool logExit = false;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (logExit)
            Debug.Log($"[KillZone] {other.name} salió de la arena", this);

        // 🎲 Sonido raro 1 en N
        TryPlayRareExitSound();

        // 🎬 Caída visual
        var fall = other.GetComponent<FallShrink2D>();
        if (fall != null)
            fall.StartFall();
        else
            other.gameObject.SetActive(false);
    }

    private void TryPlayRareExitSound()
    {
        if (AudioManager.I == null) return;
        if (rareExitSounds == null || rareExitSounds.Length == 0) return;
        if (rareChance <= 0) return;

        // 1 en N
        if (Random.Range(0, rareChance) != 0)
            return;

        AudioClip clip = rareExitSounds[Random.Range(0, rareExitSounds.Length)];
        if (clip == null) return;

        AudioManager.I.PlaySfx(clip, rareScreamVolume);
    }
}
