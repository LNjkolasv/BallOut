using System.Collections;
using UnityEngine;

public class FallShrink2D : MonoBehaviour
{
    [Header("Fall shrink")]
    [SerializeField] private float shrinkDuration = 1.5f;
    [SerializeField] private float minScale = 0.05f;

    [Header("Movement slow")]
    [SerializeField] private float slowTime = 1f;        // tiempo real de frenado
    [SerializeField] private float minSpeedFactor = 0.03f;

    private Rigidbody2D rb;
    private Vector3 startScale;
    private Vector2 startVelocity;

    private float speedFactor = 1f;
    private float speedFactorVelocity; // requerido por SmoothDamp
    private bool isFalling;

    public bool IsFalling => isFalling;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startScale = transform.localScale;
    }

    public void StartFall()
    {
        if (!gameObject.activeInHierarchy) return;
        if (isFalling) return;
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        isFalling = true;

        if (rb != null)
            startVelocity = rb.linearVelocity;

        speedFactor = 1f;
        speedFactorVelocity = 0f;

        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;

            // 1️ Escala (lineal, independiente)
            float k = Mathf.Clamp01(t / shrinkDuration);
            float s = Mathf.Lerp(startScale.x, minScale, k);
            transform.localScale = new Vector3(s, s, startScale.z);

            // 2️ Frenado con SmoothDamp (independiente del shrink)
            speedFactor = Mathf.SmoothDamp(
                speedFactor,
                minSpeedFactor,
                ref speedFactorVelocity,
                slowTime
            );

            if (rb != null)
                rb.linearVelocity = startVelocity * speedFactor;

            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void ResetFallState()
    {
        isFalling = false;
        transform.localScale = startScale;
    }
}
