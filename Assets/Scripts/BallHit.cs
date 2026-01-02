using UnityEngine;

public class BallHit : MonoBehaviour
{
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip strongHitSound;

    [Header("Impact")]
    [SerializeField] private float minImpact = 3f;
    [SerializeField] private float strongImpactThreshold = 21f;

    [Header("Audio")]
    [SerializeField] private float minVolume = 0.1f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float strongHitVolume = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool logImpact = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (AudioManager.I == null)
            return;

        float impact = collision.relativeVelocity.magnitude;

        if (logImpact)
            Debug.Log($"[BallHit] Impact = {impact}", this);

        if (impact < minImpact)
            return;

        // 🔊 Sonido base (siempre que supere el mínimo)
        if (hitSound != null)
        {
            float t = Mathf.InverseLerp(minImpact, strongImpactThreshold, impact);
            float volume = Mathf.Lerp(minVolume, maxVolume, t);

            AudioManager.I.PlaySfx(hitSound, volume);
        }

        // 💥 Sonido extra para impactos fuertes
        if (impact >= strongImpactThreshold && strongHitSound != null)
        {
            AudioManager.I.PlaySfx(strongHitSound, strongHitVolume);
        }
    }
}
