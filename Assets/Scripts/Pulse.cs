using UnityEngine;

// Gentle looping pulse (sine) for logo / "tap to start" so the user knows what to do.
public class Pulse : MonoBehaviour
{
    public float speed = 2f;
    public float minAlpha = 0.35f;
    public float scaleAmount = 0f;
    CanvasGroup cg;

    void Awake() { cg = GetComponent<CanvasGroup>(); }

    void Update()
    {
        float s = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        if (cg != null) cg.alpha = Mathf.Lerp(minAlpha, 1f, s);
        if (scaleAmount > 0f) transform.localScale = Vector3.one * (1f + scaleAmount * s);
    }
}
