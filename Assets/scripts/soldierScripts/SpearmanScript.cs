using UnityEngine;

public class SpearmanScript : SoldierBaseScript
{
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
   
}
