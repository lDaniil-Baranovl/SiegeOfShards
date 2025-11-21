using UnityEngine;
using UnityEngine.UI;

public class JellyElixir : MonoBehaviour
{
    public float punchScale = 1.15f;
    public float animationSpeed = 8f;
    public float elasticity = 0.15f;
    private RectTransform rect;
    private Vector3 defaultScale;
    private bool animating = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        defaultScale = rect.localScale;
    }

    public void PlayJelly()
    {
        if (!animating)
            StartCoroutine(Jelly());
    }

    private System.Collections.IEnumerator Jelly()
    {
        animating = true;

        Vector3 target = defaultScale * punchScale;

        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * animationSpeed;
            rect.localScale = Vector3.Lerp(defaultScale, target, t);
            yield return null;
        }
        t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * animationSpeed;

            float spring = Mathf.Sin(t * Mathf.PI) * elasticity;

            rect.localScale = Vector3.Lerp(target, defaultScale, t) + Vector3.one * spring;
            yield return null;
        }

        rect.localScale = defaultScale;
        animating = false;
    }
}
