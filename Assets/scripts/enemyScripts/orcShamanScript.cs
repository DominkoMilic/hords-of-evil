using UnityEngine;
using System.Collections;

public class orcShamanScript : EnemyBaseScript
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

        
        if (getCurrentHealth() <= 0) return;
    
        // displayAllStats();
        if (healRoutine == null)
            healRoutine = StartCoroutine(HealLoop());
    }

    private IEnumerator HealLoop()
    {
        yield return new WaitForSeconds(Random.Range(7f, 10f));

        while (true)
        {
            isCasting = true;

            StopCombat();

            animator?.SetTrigger("Heal");

            yield return new WaitForSeconds(healCastTime);

            troopsHeal();

            isCasting = false;

            yield return new WaitForSeconds(Random.Range(7f, 10f));
        }
    }


    private void displayAllStats() {
        Debug.Log(getEnemyName() + " stats: " +  
            getMinDamage() + " / " +
            getMaxDamage() + " / " +
            getMaxHealth() + " / " +
            getCurrentHealth() + " / " +
            getBounty() + " / " +
            getSpeed() + " / " +
            getAttackSpeed() + " / " +
            getPhysicalArmor() + " / " +
            getMagicArmor()
        );
    }

    private void troopsHeal()
    {
        int healAmount = Mathf.FloorToInt(getMaxHealth() * healPercent);

        foreach (EnemyBaseScript ally in EnemyBaseScript.allEnemies)
        {
            if (ally == null) continue;
            if (ally.getCurrentHealth() <= 0) continue;
            if (ally.getCurrentHealth() >= ally.getMaxHealth()) continue;

            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (ally == this || distance <= healRadius)
                ally.setCurrentHealth(healAmount);
        }
    }

    private void OnDisable()
    {
        if (healRoutine != null)
        {
            StopCoroutine(healRoutine);
            healRoutine = null;
        }
    }

    protected override void dealDamage()
    {
        if (targetSoldier == null) return;
    
        if (projectilePrefab == null || projectileSpawn == null)
        {
            ApplyMagicDamageInstant(targetSoldier);
            return;
        }
    
        int dmg = CalculateMagicDamage(targetSoldier);
    
        var proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        proj.SetSprite(projectileSprite);
    
        proj.Launch(targetSoldier.transform, () =>
        {
            if (targetSoldier != null)
                targetSoldier.setCurrentHealth(-dmg);
        });
    }

}
