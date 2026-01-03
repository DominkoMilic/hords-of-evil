using UnityEngine;

public class ArcherScript : SoldierBaseScript
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

    protected override void dealDamage()
    {
        if (targetEnemy == null) return;

        if (projectilePrefab == null || projectileSpawn == null)
        {
            ApplyInstantDamage(targetEnemy);
            return;
        }

        int dmg = CalculateDamage(targetEnemy);

        var proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        proj.SetSprite(projectileSprite);
        if(targetEnemy != null)
            proj.Launch(targetEnemy.transform, () => targetEnemy.setCurrentHealth(-dmg));
    }
}
