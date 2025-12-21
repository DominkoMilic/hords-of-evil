using UnityEngine;

[CreateAssetMenu(fileName = "LevelUpgradeData", menuName = "Game Data/LevelUpgradeData")]
public class LevelUpgradeData : ScriptableObject
{
    public string troopName;
    public int costPrice;
    public int upgradePrice;
}
