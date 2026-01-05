using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeAndDestroySpriteScript : MonoBehaviour
{
    [SerializeField] private float stayTime = 2f;
    [SerializeField] private float fadeTime = 1f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(stayTime);

        Color c = sr.color;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0.4f, 0f, t / fadeTime);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }
}
