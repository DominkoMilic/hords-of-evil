using UnityEngine;
using System.Collections;

public class ShieldmanScript : SoldierBaseScript
{
    [SerializeField] private float healRadius = 3.5f;
    [SerializeField] private float healPercent = 0.2f;
    [SerializeField] private float healCastTime = 0.8f;

    private Coroutine healRoutine;
    
    [Header("Archer Projectile")]
    [SerializeField] private ProjectileScript projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private Sprite projectileSprite;

    protected override bool UsesAttackReleaseEvent() => true;

    public override void Initialize()
    {
        base.Initialize();
    }

    private void OnEnable()
    {
        TryStartHealLoop();
    }

    private void OnDisable()
    {
        StopHealLoop();
    }

    private void OnDestroy()
    {
        StopHealLoop();
    }

    private void TryStartHealLoop()
    {
        if (!gameObject.activeInHierarchy) return;

        if (getCurrentHealth() <= 0) return;

        if (healRoutine == null)
            healRoutine = StartCoroutine(HealLoop());
    }

    private void StopHealLoop()
    {
        if (healRoutine != null)
        {
            StopCoroutine(healRoutine);
            healRoutine = null;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (healRoutine != null && getCurrentHealth() <= 0)
            StopHealLoop();
    }

    private IEnumerator HealLoop()
    {
        yield return new WaitForSeconds(Random.Range(25f, 30f));

        while (true)
        {
            if (!gameObject.activeInHierarchy || getCurrentHealth() <= 0)
            {
                healRoutine = null;
                yield break;
            }

            isCasting = true;

            StopCombat();
            animator?.SetTrigger("Heal");

            yield return new WaitForSeconds(healCastTime);

            if (!gameObject.activeInHierarchy || getCurrentHealth() <= 0)
            {
                isCasting = false;
                healRoutine = null;
                yield break;
            }

            TroopsHeal();

            isCasting = false;

            yield return new WaitForSeconds(Random.Range(25f, 30f));
        }
    }

    private void TroopsHeal()
    {
        int healAmount = Mathf.FloorToInt(getMaxHealth() * healPercent);

        foreach (SoldierBaseScript ally in SoldierBaseScript.allSoldiers)
        {
            if (ally == null) continue;
            if (ally.getCurrentHealth() <= 0) continue;
            if (ally.getCurrentHealth() >= ally.getMaxHealth()) continue;

            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (ally == this || distance <= healRadius)
                ally.setCurrentHealth(healAmount);
        }
    }

    protected override void dealDamage()
    {
        if (targetEnemy == null) return;

        if (projectilePrefab == null || projectileSpawn == null)
        {
            ApplyMagicDamageInstant(targetEnemy);
            return;
        }

        int dmg = CalculateMagicDamage(targetEnemy);

        var proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);

        if (projectileSprite != null)
            proj.SetSprite(projectileSprite);

        proj.Launch(targetEnemy.transform, () =>
        {
            if (targetEnemy != null)
                targetEnemy.setCurrentHealth(-dmg);
        });
    }
}