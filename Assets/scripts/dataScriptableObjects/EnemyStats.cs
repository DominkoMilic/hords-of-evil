using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Game Data/Enemy Data")]
public class EnemyStats : ScriptableObject
{
    public string enemyName;
    public int minDamage;
    public int maxDamage;
    public int maxHealth;
    public int bounty;
    public float speed;
    public float attackSpeed;
    public int physicalArmor;
    public int magicArmor;
    public float attackRange;
    
    public float separationRadius;
    public float separationStrength;
}
