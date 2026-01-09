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
    English = 0
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

public enum AttackRangeType
{
    Melee,
    Ranged
}

public enum DamageType
{
    Normal,
    Magic
}

public enum AchievementType
{
    // Kills
    KillFirstEnemy,              // run: first kill
    KillEnemiesInRun,            // run: >= targetInt
    KillEnemiesTotal,            // lifetime: >= targetInt
    KillWithAbilitiesOnlyInRun,  // run: >= targetInt

    // Waves
    SurviveWavesInRun,           // run: waves survived >= targetInt
    DieAfterWave,                // run: died after reaching >= targetInt

    // Fireballs
    CastFireballsInRun,          // run: >= targetInt
    CastFireballsTotal,          // lifetime: >= targetInt

    // Units
    SpawnUnitsInRun,             // run: >= targetInt (any units total)
    UnitsAliveSimultaneous,      // run: peak alive >= targetInt
    LoseUnitsInRun,              // run: units lost >= targetInt
    SpawnAnyUnitMaxTimes,        // run: spawned any single unit type to its max

    // Constraints / special wave clears
    BeatWavesWithOnlyOneUnitType, // run: total waves cleared while only one type used >= targetInt
    BeatWaveWithoutSpawningAny,   // run: total waves cleared without spawning >= targetInt (use 1 for “Solo Act”)
    ClearWaveWithoutSpawningOrCasting, // run: total waves cleared with no spawns and no fireballs >= targetInt

    // Death / run start
    StartFirstRun,               // lifetime: first run started
    DieWithinSeconds,            // run: died within targetFloat seconds
    DieWithGoldAtLeast,          // run: died with gold >= targetInt
    DieTimesTotal,               // lifetime: deaths >= targetInt

    // UI / meta
    PressButtonsInRun,           // run: >= targetInt
    UnlockXAchievements,         // meta: unlocked >= targetInt
    UnlockAllAchievements,       // meta: all unlocked
    SecretClick                  // secret decoration click
}

public enum DamageSource
{
    Soldier,
    Fireball,
    Ability,
    Unknown
}
