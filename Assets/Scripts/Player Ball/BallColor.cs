using UnityEngine;

public class BallColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Apply(Color c)
    {
        if (sr != null) sr.color = c;
    }
}
