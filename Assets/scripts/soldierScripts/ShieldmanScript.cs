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

        
        if (getCurrentHealth() <= 0) return;
    
        // displayAllStats();
        if (healRoutine == null)
            healRoutine = StartCoroutine(HealLoop());
    }

     protected override void Update(){
        base.Update();
    }

    private void displayAllStats() {
        Debug.Log(getSoldierName() + " stats: " +  
            getMinDamage() + " / " +
            getMaxDamage() + " / " +
            getMaxHealth() + " / " +
            getCurrentHealth() + " / " +
            getCost() + " / " +
            getSpeed() + " / " +
            getAttackSpeed() + " / " +
            getPhysicalArmor() + " / " +
            getMagicArmor() + " / " +
            getAttackRange()
        );
    }

    private IEnumerator HealLoop()
    {
        yield return new WaitForSeconds(Random.Range(35f, 40f));

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

    private void troopsHeal()
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
        if (targetEnemy == null) return;
    
        if (projectilePrefab == null || projectileSpawn == null)
        {
            ApplyMagicDamageInstant(targetEnemy);
            return;
        }
    
        int dmg = CalculateMagicDamage(targetEnemy);
    
        var proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        proj.SetSprite(projectileSprite);
    
        proj.Launch(targetEnemy.transform, () =>
        {
            if (targetEnemy != null)
                targetEnemy.setCurrentHealth(-dmg);
        });
    }
}
