using System.Collections.Generic;
using UnityEngine;

public class LoreScript : MonoBehaviour
{
    [SerializeField] private List<UnitLoreEntry> entries = new List<UnitLoreEntry>();

    private Dictionary<UnitId, UnitLoreData> loreByUnit;

    private void Awake()
    {
        loreByUnit = new Dictionary<UnitId, UnitLoreData>();

        foreach (var entry in entries)
        {
            if (entry == null || entry.data == null) continue;
            loreByUnit[entry.unitId] = entry.data;
        }
    }

    public string[] GetLore(UnitId unit, Language language)
    {
        if (loreByUnit == null) Awake();

        if (!loreByUnit.TryGetValue(unit, out var data) || data == null)
            return new[] { "Role: ?", "Weakness: ?", "Lore: ?" };

        return data.GetTexts(language);
    }

    public string GetName(UnitId unit, Language language)
    {
        if (loreByUnit == null) Awake();

        if (!loreByUnit.TryGetValue(unit, out var data) || data == null)
            return "Unknown";

        return data.GetName(language);
    }

    public Sprite GetImage(UnitId unit)
    {
        if (loreByUnit == null) Awake();

        if (!loreByUnit.TryGetValue(unit, out var data) || data == null)
            return null;

        return data.image;
    }
}
