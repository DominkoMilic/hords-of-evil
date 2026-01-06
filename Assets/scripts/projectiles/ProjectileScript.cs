using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Flight")]
    public float travelTime = 0.35f;     
    public float arcHeight = 1.2f;       
    public bool rotateAlongPath = true;

    [SerializeField] private SpriteRenderer sr;

    private Transform target;
    private Vector3 startPos;
    private Vector3 targetStartPos;     
    private float t;

    private System.Action onHit;  

    private void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);
    }
        

    public void Launch(Transform target, System.Action onHitCallback)
    {
        this.target = target;
        onHit = onHitCallback;

        startPos = transform.position;
        targetStartPos = target ? target.position : startPos;
        t = 0f;
    }

    public void SetSprite(Sprite sprite)
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);
        if (sr == null)
        {
            Debug.LogError("ProjectileScript: No SpriteRenderer found on projectile prefab (root or children).");
            return;
        }
        sr.sprite = sprite;
    }



    private void Update()
    {
        if (target == null) { Destroy(gameObject); return; }

        if (!GameFlowScript.Started)
            return;

        Vector3 endPos = target.position;

        t += Time.deltaTime / Mathf.Max(0.0001f, travelTime);
        float u = Mathf.Clamp01(t);

        Vector3 pos = Vector3.Lerp(startPos, endPos, u);

        float arc = arcHeight * 4f * (u - u * u);
        pos.y += arc;

        if (rotateAlongPath)
        {
            Vector3 nextEnd = target.position;
            Vector3 next = Vector3.Lerp(startPos, nextEnd, Mathf.Clamp01(u + 0.02f));
            float u2 = Mathf.Clamp01(u + 0.02f);
            float nextArc = arcHeight * 4f * (u2 - u2 * u2);
            next.y += nextArc;

            Vector2 dir = (next - pos);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        transform.position = pos;

        if (u >= 1f)
        {
            onHit?.Invoke();
            Destroy(gameObject);
        }
    }
}
