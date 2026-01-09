using System;
using UnityEngine;

public enum UnitSide
{
    Player,
    Enemy,
    Neutral
}

[Serializable]
public struct UnitStats
{
    [Min(1)] public int maxHealth;

    [Header("Damage")]
    [Min(0)] public int minDamage;
    [Min(0)] public int maxDamage;

    [Header("Armor")]
    public int armorPhysical;
    public int armorMagical;
}

[Serializable]
public struct UnitEconomy
{
    [Tooltip("Used for Player units (shop/training). Set 0 for enemies.")]
    [Min(0)] public int cost;

    [Tooltip("Reward for killing the unit (enemies). Set 0 for player units.")]
    [Min(0)] public int bounty;
}

[CreateAssetMenu(fileName = "New Unit", menuName = "Game Data/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Identity")]
    public UnitId unitId;
    public UnitSide side = UnitSide.Player;

    [Header("Presentation")]
    public Sprite[] frames;
    [Min(1)] public int fps = 10;

    [Header("Localization - English")]
    public string nameEnglish;

    [TextArea(2, 4)] public string loreEnglish;

    [Header("Gameplay")]
    public UnitStats stats;
    public UnitEconomy economy;

    [Header("Combat")]
    public AttackRangeType attackRangeType = AttackRangeType.Melee;
    public DamageType damageType = DamageType.Normal;

    public string GetName(Language language) => nameEnglish;

    public string GetLore(Language language) => loreEnglish;

    public int GetCostOrBounty() => side == UnitSide.Player ? economy.cost : economy.bounty;

    public bool HasAnimation => frames != null && frames.Length > 0;
    public Sprite GetFirstFrame() => HasAnimation ? frames[0] : null;
}
