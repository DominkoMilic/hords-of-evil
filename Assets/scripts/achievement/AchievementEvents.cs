using System;
using UnityEngine;

public static class AchievementEvents
{
    public static event Action RunStarted;
    public static event Action<RunEndInfo> RunEnded; // includes death, wave reached, time, gold

    public static event Action EnemyKilled;
    public static event Action<bool> EnemyKilledByAbility; // true if ability kill

    public static event Action FireballCast;

    public static event Action<int> WaveCleared; // wave index cleared (1-based or whatever you use)

    /// <summary>
    /// unitType: e.g. "Archer", "Swordsman"
    /// maxAllowedForThisType: your design limit for this run
    /// </summary>
    public static event Action<string, int> UnitSpawned;
    public static event Action<string> UnitDied;

    public static event Action<int> GoldChanged; // current gold
    public static event Action AnyUIButtonPressed;

    public static event Action SecretDecorationClicked;

    // --- Emit helpers (optional convenience) ---
    public static void EmitRunStarted() => RunStarted?.Invoke();
    public static void EmitRunEnded(RunEndInfo info) => RunEnded?.Invoke(info);

    public static void EmitEnemyKilled() => EnemyKilled?.Invoke();
    public static void EmitEnemyKilledByAbility(bool abilityKill) => EnemyKilledByAbility?.Invoke(abilityKill);

    public static void EmitFireballCast() => FireballCast?.Invoke();
    public static void EmitWaveCleared(int wave) => WaveCleared?.Invoke(wave);

    public static void EmitUnitSpawned(string unitType, int maxAllowedForThisType) => UnitSpawned?.Invoke(unitType, maxAllowedForThisType);
    public static void EmitUnitDied(string unitType) => UnitDied?.Invoke(unitType);

    public static void EmitGoldChanged(int currentGold) => GoldChanged?.Invoke(currentGold);
    public static void EmitAnyUIButtonPressed() => AnyUIButtonPressed?.Invoke();
    public static void EmitSecretDecorationClicked() => SecretDecorationClicked?.Invoke();
}

[Serializable]
public struct RunEndInfo
{
    public bool died;
    public int waveReached;     // highest wave reached/cleared (your choice, be consistent)
    public float runSeconds;
    public int goldAtDeathOrEnd;
}
