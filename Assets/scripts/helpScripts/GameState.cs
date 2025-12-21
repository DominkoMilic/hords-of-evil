public static class Game
{
    public static Difficulty SelectedDifficulty;
    public static Language SelectedLanguage;
}

public enum Difficulty
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
}

public enum Language
{
    English = 0,
    Spanish = 1,
    Croatian = 2
}

public enum UnitId
{
    Goblin,
    GoblinArcher,
    HeavyGoblin,
    OrcWarrior,
    OrcArcher,
    OrcShaman,
    Ogre,
    OgreCrusher,
    Swordsman,
    Spearman,
    Shieldman,
    Archer
}