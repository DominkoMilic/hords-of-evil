using UnityEngine;

[CreateAssetMenu(fileName = "New Lore", menuName = "Game Data/Lore Data")]
public class UnitLoreData : ScriptableObject
{
    public string nameEnglish;

    public Sprite image;

    [TextArea(1, 2)] public string roleEnglish;
    [TextArea(1, 2)] public string weaknessEnglish;
    [TextArea(2, 4)] public string loreEnglish;

    public string GetName(Language language)
    {
        return language switch
        {
            _ => nameEnglish
        };
    }

    public string[] GetTexts(Language language)
    {
        return language switch
        {
            _ => new[] { roleEnglish, weaknessEnglish, loreEnglish }
        };
    }
}
