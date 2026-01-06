using UnityEngine;

public class FireballScript : MonoBehaviour
{
    public ManaSpellData fireballData;
    private Animator animator;

    [Header("Fall")]
    [SerializeField] private float speed = 10f;

    [Header("Landing Indicator")]
    [SerializeField] private SpriteRenderer indicatorPrefab; 
    [SerializeField] private bool indicatorFadeIn = true;
    [SerializeField] private float indicatorMaxWorldSize = 10f;

    [Header("Impact Decal")]
    [SerializeField] private GameObject earthCrackPrefab;

    private SpriteRenderer indicatorInstance;
    private Vector3 targetPosition;

    private int damage;
    private int heal;
    private float range;

    private bool hasExploded;

    private float startDistance;

    private bool hitSoundPlayed;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>(true);
    }

    public void Initialize(Vector3 fallPosition)
    {
        hasExploded = false;
        targetPosition = fallPosition;

        hitSoundPlayed = false;

        damage = fireballData.damage;
        heal = fireballData.heal;
        range = fireballData.range;

        if (animator) animator.Play("fireballPixelartAnimController", 0, 0f);

        SpawnIndicator();

        startDistance = Vector3.Distance(transform.position, targetPosition);
        if (startDistance < 0.001f) startDistance = 0.001f;

        if (AudioManagerScript.Instance != null)
            AudioManagerScript.Instance.PlayFireballWhoosh();
    }

    private void SpawnIndicator()
    {
        if (indicatorPrefab == null) return;

        indicatorInstance = Instantiate(indicatorPrefab, targetPosition, Quaternion.identity);

        indicatorInstance.transform.localScale = Vector3.zero;

        if (indicatorFadeIn)
        {
            var c = indicatorInstance.color;
            c.a = 0f;
            indicatorInstance.color = c;
        }
    }

    private void Update()
    {
        if (hasExploded) return;

        if (!GameFlowScript.Started)
            return;

        FireballFall();
        UpdateIndicator();
    }

    private void FireballFall()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

         float sqrDist = (transform.position - targetPosition).sqrMagnitude;

        if (!hitSoundPlayed && sqrDist <= 2.3f * 2.3f) 
        {
            hitSoundPlayed = true;
            AudioManagerScript.Instance?.PlayFireballHit();
        }

        if ((transform.position - targetPosition).sqrMagnitude <= 0.5f * 0.5f)
            FireballExplosion(targetPosition);
    }

    private void UpdateIndicator()
    {
        if (indicatorInstance == null) return;

        float remaining = Vector3.Distance(transform.position, targetPosition);
        float progress = 1f - Mathf.Clamp01(remaining / startDistance); 

        float eased = 1f - Mathf.Pow(1f - progress, 2f);

        indicatorInstance.transform.position = targetPosition;
        indicatorInstance.transform.localScale = Vector3.one * (indicatorMaxWorldSize * eased);

        if (indicatorFadeIn)
        {
            var c = indicatorInstance.color;
            c.a = Mathf.Lerp(0f, 0.5f, eased);
            indicatorInstance.color = c;
        }
    }

    private void FireballExplosion(Vector3 fallPosition)
    {
        hasExploded = true;

        if (earthCrackPrefab != null)
        {
            Vector3 spawnPos = new Vector3(fallPosition.x, fallPosition.y, fallPosition.z);
            Instantiate(earthCrackPrefab, spawnPos, Quaternion.identity);
        }

        if (indicatorInstance != null)
            Destroy(indicatorInstance.gameObject);

        CameraShakeScript.Instance?.Shake(0.12f, 0.08f);

        foreach (var enemy in EnemyBaseScript.allEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(fallPosition, enemy.transform.position);
            if (distance <= range)
                enemy.setCurrentHealth(-damage);
        }

        Destroy(gameObject);
    }

}
