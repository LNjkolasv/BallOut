using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ArenaColorCycle : MonoBehaviour
{
    [SerializeField] private float cycleSeconds = 6f;   // cuánto tarda en dar una vuelta
    [SerializeField] private float saturation = 0.35f;  // 0..1 (más bajo = más sobrio)
    [SerializeField] private float value = 0.35f;       // 0..1 (brillo)
    [SerializeField] private bool useUnscaledTime = false;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float h = (t / Mathf.Max(0.01f, cycleSeconds)) % 1f; // 0..1
        sr.color = Color.HSVToRGB(h, saturation, value);
    }
}
