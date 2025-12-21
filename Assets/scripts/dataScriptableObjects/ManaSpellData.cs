using UnityEngine;

[CreateAssetMenu(fileName = "ManaSpellData", menuName = "Game Data/ManaSpellData")]
public class ManaSpellData : ScriptableObject
{
    public int manaNeeded;
    public int damage;
    public int heal;
    public float range;
}
