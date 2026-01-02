using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ExtraPush : MonoBehaviour
{
    [Header("Boost")]
    [SerializeField] private float boost = 1.5f;          // cuánta "explosión" extra
    [SerializeField] private float minImpact = 5f;        // no boost en roces
    [SerializeField] private float maxExtraImpulse = 20f;  // límite para que no se vaya al infinito
    [SerializeField] private float cooldown = 0.05f;      // evita doble impulso por múltiples contactos

    private Rigidbody2D rb;
    private float lastBoostTime;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        if (Time.time - lastBoostTime < cooldown) return;

        float impact = col.relativeVelocity.magnitude;
        if (impact < minImpact) return;

        // normal del contacto: hacia dónde empujar (alejarse del otro)
        Vector2 n = col.GetContact(0).normal; // normal apuntando desde el otro hacia vos
        // Impulso extra proporcional al impacto
        float extra = Mathf.Clamp(impact * (boost - 1f), 0f, maxExtraImpulse);

        rb.AddForce(n * extra, ForceMode2D.Impulse);

        lastBoostTime = Time.time;
    }
}
