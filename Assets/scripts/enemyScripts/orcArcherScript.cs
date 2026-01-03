using UnityEngine;

public class orcArcherScript : EnemyBaseScript
{
    [Header("Archer Projectile")]
    [SerializeField] private ProjectileScript projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private Sprite projectileSprite;

    protected override bool UsesAttackReleaseEvent() => true;

    public override void Initialize()
    {
        base.Initialize();
    
        // displayAllStats();
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

    protected override void dealDamage()
    {
        if (targetSoldier == null) return;

        if (projectilePrefab == null || projectileSpawn == null)
        {
            ApplyInstantDamage(targetSoldier);
            return;
        }

        int dmg = CalculateDamage(targetSoldier);

        var proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        proj.SetSprite(projectileSprite);
        if(targetSoldier != null)
            proj.Launch(targetSoldier.transform, () => targetSoldier.setCurrentHealth(-dmg));
    }
}
