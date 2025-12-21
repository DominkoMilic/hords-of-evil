using UnityEngine;

public class goblinScript : EnemyBaseScript
{

    public override void Initialize()
    {
        base.Initialize();

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
