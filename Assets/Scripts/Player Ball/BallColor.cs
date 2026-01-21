using UnityEngine;

public class BallColor : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private TrailRenderer trail;

    private void Awake()
    {
        sr = sr ?? GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[BallColor] No SpriteRenderer encontrado.", this);
            enabled = false;
            return;
        }

        trail = trail ?? GetComponent<TrailRenderer>();
        if (trail != null)
        {
            // Instancia material único para que el color sea por pelota
            trail.material = new Material(trail.material);
        }
        else
        {
            Debug.LogWarning("[BallColor] No TrailRenderer encontrado.", this);
        }
    }

    public void Apply(Color c)
    {
        if (sr != null)
            sr.color = c;

        if (trail != null && trail.material != null)
        {
            trail.material.color = c;
            // Opcional: si usas tint, podés hacer trail.material.SetColor("_TintColor", c);
        }
    }
}