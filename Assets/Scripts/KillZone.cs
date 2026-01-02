using UnityEngine;

public class KillZone2D : MonoBehaviour
{
    [Header("Kirby scream")]
    [SerializeField] private AudioClip kirbyScream;
    [SerializeField] private float maxSpeedThreshold = 18f;
    [SerializeField] private float screamVolume = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool logExitSpeed = false;
    private float maxRecordedSpeed = 0f;

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            float speed = rb.linearVelocity.magnitude;

            // 🧪 DEBUG
            if (logExitSpeed)
            {
                if (speed > maxRecordedSpeed)
                {
                    maxRecordedSpeed = speed;
                    Debug.Log($"[KillZone] Nueva velocidad máxima: {maxRecordedSpeed:F2}", this);
                }
                else
                {
                    Debug.Log($"[KillZone] Velocidad de salida: {speed:F2}", this);
                }
            }

            // 🔊 Kirby grita solo en casos excepcionales
            if (AudioManager.I != null &&
                kirbyScream != null &&
                speed >= maxSpeedThreshold)
            {
                AudioManager.I.PlaySfx(kirbyScream, screamVolume);
            }
        }

        // 🎬 Caída visual
        var fall = other.GetComponent<FallShrink2D>();
        if (fall != null)
            fall.StartFall();
        else
            other.gameObject.SetActive(false);
    }
}
