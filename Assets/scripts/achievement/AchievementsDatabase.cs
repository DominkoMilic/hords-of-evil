using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Achievements Database", fileName = "AchievementsDatabase")]
public class AchievementsDatabase : ScriptableObject
{
    public List<AchievementEntry> achievements = new();

    [Serializable]
    public class AchievementEntry
    {
        [Header("Identity")]
        public string id;
        public string title;
        [TextArea] public string description;
        public bool secret;

        [Header("Visuals")]
        public Sprite icon;

        [Header("Logic")]
        public AchievementType type;
        public int targetInt;
        public float targetFloat;
        public string targetString;
    }
}
