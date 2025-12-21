using UnityEngine;

[CreateAssetMenu(fileName = "New Lore", menuName = "Game Data/Lore Data")]
public class UnitLoreData : ScriptableObject
{
    public string nameEnglish;
    public string nameCroatian;
    public string nameSpanish;

    public Sprite image;

    [TextArea(1, 2)] public string roleEnglish;
    [TextArea(1, 2)] public string weaknessEnglish;
    [TextArea(2, 4)] public string loreEnglish;

    [TextArea(1, 2)] public string roleCroatian;
    [TextArea(1, 2)] public string weaknessCroatian;
    [TextArea(2, 4)] public string loreCroatian;

    [TextArea(1, 2)] public string roleSpanish;
    [TextArea(1, 2)] public string weaknessSpanish;
    [TextArea(2, 4)] public string loreSpanish;

    public string GetName(Language language)
    {
        return language switch
        {
            Language.Croatian => nameCroatian,
            Language.Spanish => nameSpanish,
            _ => nameEnglish
        };
    }

    public string[] GetTexts(Language language)
    {
        return language switch
        {
            Language.Croatian => new[] { roleCroatian, weaknessCroatian, loreCroatian },
            Language.Spanish => new[] { roleSpanish, weaknessSpanish, loreSpanish },
            _ => new[] { roleEnglish, weaknessEnglish, loreEnglish }
        };
    }
}
