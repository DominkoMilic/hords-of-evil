using UnityEngine;

public class heavyGoblinScript : EnemyBaseScript
{
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
}
