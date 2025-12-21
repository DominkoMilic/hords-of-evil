using System.Collections.Generic;
using UnityEngine;

public class LoreDatabase : MonoBehaviour
{
    [SerializeField] private List<UnitLoreEntry> entries = new List<UnitLoreEntry>();

    private Dictionary<UnitId, UnitLoreData> byId;

    private void Awake()
    {
        byId = new Dictionary<UnitId, UnitLoreData>();

        foreach (var e in entries)
        {
            if (e == null || e.data == null) continue;
            byId[e.unitId] = e.data;
        }
    }

    public UnitLoreData GetUnitLore(UnitId unitId)
    {
        if (byId == null) Awake(); 
        byId.TryGetValue(unitId, out var data);
        return data;
    }
}
