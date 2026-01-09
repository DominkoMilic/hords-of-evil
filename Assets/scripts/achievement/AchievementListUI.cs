using System.Collections.Generic;
using UnityEngine;

public class AchievementsListUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AchievementsDatabase database;
    [SerializeField] private AchievementManager achievementManager;

    [Header("UI")]
    [SerializeField] private Transform contentRoot;          
    [SerializeField] private AchievementItemView itemPrefab;  

    private readonly List<AchievementItemView> spawned = new();

    private void OnEnable()
    {
        if (achievementManager == null)
            achievementManager = FindFirstObjectByType<AchievementManager>();

        BuildList();

        if (achievementManager != null)
            achievementManager.AchievementUnlocked += OnAchievementUnlocked;
    }

    private void OnDisable()
    {
        if (achievementManager != null)
            achievementManager.AchievementUnlocked -= OnAchievementUnlocked;
    }

    private void OnAchievementUnlocked(AchievementsDatabase.AchievementEntry entry)
    {
        RefreshAll();
    }

    public void BuildList()
    {
        foreach (Transform child in contentRoot) Destroy(child.gameObject);
        spawned.Clear();

        foreach (var entry in database.achievements)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            spawned.Add(item);

            bool unlocked = achievementManager != null && achievementManager.IsAchievementUnlocked(entry.id);
            item.Bind(entry, unlocked);
        }
    }

    private void RefreshAll()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            var entry = database.achievements[i];
            bool unlocked = achievementManager != null && achievementManager.IsAchievementUnlocked(entry.id);
            spawned[i].SetUnlocked(unlocked);
        }
    }
}
