using UnityEngine;

public class ogreScript : EnemyBaseScript
{
    [SerializeField] private float splashRadius = 0.7f;

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

        int rawDamage = Random.Range(getMinDamage(), getMaxDamage() + 1);

        int armorTier = targetSoldier.getPhysicalArmor();
        armorTier = Mathf.Clamp(armorTier, 0, ARMOR_REDUCTION.Length - 1);

        float reduction = ARMOR_REDUCTION[armorTier];
        int finalDamage = Mathf.RoundToInt(rawDamage * (1f - reduction));
        finalDamage = Mathf.Max(1, finalDamage);

        Vector3 center = targetSoldier.transform.position;

        foreach (var soldier in SoldierBaseScript.allSoldiers)
        {
            if (soldier == null) continue;
            if (soldier.getCurrentHealth() <= 0) continue; 

            float dist = Vector3.Distance(center, soldier.transform.position);
            if (dist <= splashRadius)
            {
                soldier.setCurrentHealth(-finalDamage);
            }
        }
    }


}
