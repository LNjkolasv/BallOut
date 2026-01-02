using System.Collections;
using UnityEngine;

public class ArenaShrinker : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float startAfter = 6f;
    [SerializeField] private float shrinkDuration = 15f;

    [Header("Scale")]
    [SerializeField] private float targetScaleMultiplier = 0.5f;

    private Vector3 startScale;
    private bool shrinking;

    void Awake()
    {
        startScale = transform.localScale;
    }

    void Start()
    {
        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        yield return new WaitForSeconds(startAfter);

        if (shrinking) yield break;
        shrinking = true;

        Vector3 endScale = startScale * targetScaleMultiplier;

        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / shrinkDuration);

            transform.localScale = Vector3.Lerp(startScale, endScale, k);

            yield return null;
        }

        transform.localScale = endScale;
        shrinking = false;
    }

    public void StartShrinkNow()
    {
        StopAllCoroutines();
        StartCoroutine(ShrinkRoutine());
    }
}
