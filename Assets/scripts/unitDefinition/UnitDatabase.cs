using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitEntry
{
    public UnitId unitId;
    [Range(1, 4)] public int level = 1;
    public UnitDefinition data;
}

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "Game Data/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    [SerializeField] private List<UnitEntry> entries = new List<UnitEntry>();

    private Dictionary<(UnitId unitId, int level), UnitDefinition> byKey;

    private void OnEnable()
    {
        Rebuild();
    }

    private void Rebuild()
    {
        byKey = new Dictionary<(UnitId, int), UnitDefinition>();

        foreach (var e in entries)
        {
            if (e == null || e.data == null) continue;

            int lvl = Mathf.Clamp(e.level, 1, 4);
            byKey[(e.unitId, lvl)] = e.data;
        }
    }

    public UnitDefinition Get(UnitId unitId, int level = 1)
    {
        if (byKey == null || byKey.Count == 0) Rebuild();

        level = Mathf.Clamp(level, 1, 4);

        byKey.TryGetValue((unitId, level), out var data);
        return data;
    }
}
