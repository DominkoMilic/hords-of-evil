using UnityEngine;

[CreateAssetMenu(fileName = "New Soldier Stats", menuName = "Game Data/Soldier Data")]
public class SoldierStats : ScriptableObject
{
    public string soldierName;
    public int minDamage;
    public int maxDamage;
    public int maxHealth;
    public int cost;
    public float speed;
    public float attackSpeed;
    public int physicalArmor;
    public int magicArmor;
    public float attackRange;
}
