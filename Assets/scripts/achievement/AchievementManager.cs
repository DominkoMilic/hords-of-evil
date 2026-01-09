using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [SerializeField] private AchievementsDatabase database;

    public event Action<AchievementsDatabase.AchievementEntry> AchievementUnlocked;

    private const string SaveKey = "ACHIEVEMENTS_SAVE_V1";

    // Save + lifetime stats
    private SaveData _save;

    // Run stats
    private RunStats _run;

    // Wave trackers for “constraint” achievements
    private bool _spawnedThisWave;
    private bool _castFireballThisWave;
    private HashSet<string> _unitTypesUsedThisWave = new();

    // For Quality Over Quantity: count how many waves cleared while only one unit type was used in that wave
    private int _wavesClearedWithOnlyOneUnitType;
    private int _wavesClearedWithoutSpawning;
    private int _wavesClearedWithoutSpawningOrCasting;

    // Unit Spammer: track per-type spawn counts + their max
    private Dictionary<string, int> _spawnCountByType = new();
    private Dictionary<string, int> _maxAllowedByType = new();

    public static AchievementManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (database == null)
        {
            Debug.LogError("AchievementManager: database not assigned.");
            enabled = false;
            return;
        }

        _save = Load();
    }

    private void OnEnable()
    {
        AchievementEvents.RunStarted += OnRunStarted;
        AchievementEvents.RunEnded += OnRunEnded;

        AchievementEvents.EnemyKilled += OnEnemyKilled;
        AchievementEvents.EnemyKilledByAbility += OnEnemyKilledByAbility;

        AchievementEvents.FireballCast += OnFireballCast;
        AchievementEvents.WaveCleared += OnWaveCleared;

        AchievementEvents.UnitSpawned += OnUnitSpawned;
        AchievementEvents.UnitDied += OnUnitDied;

        AchievementEvents.GoldChanged += OnGoldChanged;
        AchievementEvents.AnyUIButtonPressed += OnAnyUIButtonPressed;

        AchievementEvents.SecretDecorationClicked += OnSecretDecorationClicked;
    }

    private void OnDisable()
    {
        AchievementEvents.RunStarted -= OnRunStarted;
        AchievementEvents.RunEnded -= OnRunEnded;

        AchievementEvents.EnemyKilled -= OnEnemyKilled;
        AchievementEvents.EnemyKilledByAbility -= OnEnemyKilledByAbility;

        AchievementEvents.FireballCast -= OnFireballCast;
        AchievementEvents.WaveCleared -= OnWaveCleared;

        AchievementEvents.UnitSpawned -= OnUnitSpawned;
        AchievementEvents.UnitDied -= OnUnitDied;

        AchievementEvents.GoldChanged -= OnGoldChanged;
        AchievementEvents.AnyUIButtonPressed -= OnAnyUIButtonPressed;

        AchievementEvents.SecretDecorationClicked -= OnSecretDecorationClicked;
    }

    // ------------------ Event handlers ------------------

    private void OnRunStarted()
    {
        _run = new RunStats();
        _spawnedThisWave = false;
        _castFireballThisWave = false;
        _unitTypesUsedThisWave.Clear();

        _wavesClearedWithOnlyOneUnitType = 0;
        _wavesClearedWithoutSpawning = 0;
        _wavesClearedWithoutSpawningOrCasting = 0;

        _spawnCountByType.Clear();
        _maxAllowedByType.Clear();

        _save.lifetimeRunsStarted++;
        SaveNow();

        // Getting Started
        EvaluateType(AchievementType.StartFirstRun);
    }

    private void OnRunEnded(RunEndInfo info)
    {
        if (info.died)
        {
            _save.lifetimeDeaths++;
            SaveNow();

            // death-related
            EvaluateType(AchievementType.DieTimesTotal);
            EvaluateType(AchievementType.DieWithinSeconds, info);
            EvaluateType(AchievementType.DieWithGoldAtLeast, info);
            EvaluateType(AchievementType.DieAfterWave, info);
        }

        // meta unlock checks
        EvaluateType(AchievementType.UnlockXAchievements);
        EvaluateType(AchievementType.UnlockAllAchievements);
    }

    private void OnEnemyKilled()
    {
        _run.killsInRun++;
        _save.lifetimeKills++;

        SaveNow(); // cheap enough; if you prefer, batch saves
        EvaluateType(AchievementType.KillFirstEnemy);
        EvaluateType(AchievementType.KillEnemiesInRun);
        EvaluateType(AchievementType.KillEnemiesTotal);
    }

    private void OnEnemyKilledByAbility(bool abilityKill)
    {
        if (!abilityKill) return;
        _run.abilityKillsInRun++;
        EvaluateType(AchievementType.KillWithAbilitiesOnlyInRun);
    }

    private void OnFireballCast()
    {
        _run.fireballsInRun++;
        _save.lifetimeFireballs++;
        _castFireballThisWave = true;

        SaveNow();
        EvaluateType(AchievementType.CastFireballsInRun);
        EvaluateType(AchievementType.CastFireballsTotal);
    }

    private void OnWaveCleared(int wave)
    {
        _run.wavesSurvived = Mathf.Max(_run.wavesSurvived, wave);

        // Constraint wave checks happen at the moment a wave is cleared
        if (!_spawnedThisWave) _wavesClearedWithoutSpawning++;
        if (!_spawnedThisWave && !_castFireballThisWave) _wavesClearedWithoutSpawningOrCasting++;
        if (_unitTypesUsedThisWave.Count == 1) _wavesClearedWithOnlyOneUnitType++;

        // Reset per-wave trackers for next wave
        _spawnedThisWave = false;
        _castFireballThisWave = false;
        _unitTypesUsedThisWave.Clear();

        EvaluateType(AchievementType.SurviveWavesInRun);
        EvaluateType(AchievementType.BeatWavesWithOnlyOneUnitType);
        EvaluateType(AchievementType.BeatWaveWithoutSpawningAny);
        EvaluateType(AchievementType.ClearWaveWithoutSpawningOrCasting);
    }

    private void OnUnitSpawned(string unitType, int maxAllowedForThisType)
    {
        _run.unitsSpawnedInRun++;
        _spawnedThisWave = true;
        _unitTypesUsedThisWave.Add(unitType);

        // per-type spawn counts
        if (!_spawnCountByType.ContainsKey(unitType)) _spawnCountByType[unitType] = 0;
        _spawnCountByType[unitType]++;

        _maxAllowedByType[unitType] = maxAllowedForThisType;

        // alive counters
        _run.unitsAlive++;
        _run.peakUnitsAlive = Mathf.Max(_run.peakUnitsAlive, _run.unitsAlive);

        EvaluateType(AchievementType.SpawnUnitsInRun);
        EvaluateType(AchievementType.UnitsAliveSimultaneous);
        EvaluateType(AchievementType.SpawnAnyUnitMaxTimes);
    }

    private void OnUnitDied(string unitType)
    {
        _run.unitsLostInRun++;
        _run.unitsAlive = Mathf.Max(0, _run.unitsAlive - 1);

        EvaluateType(AchievementType.LoseUnitsInRun);
    }

    private void OnGoldChanged(int currentGold)
    {
        _run.currentGold = currentGold;
    }

    private void OnAnyUIButtonPressed()
    {
        _run.buttonsPressedInRun++;
        EvaluateType(AchievementType.PressButtonsInRun);
    }

    private void OnSecretDecorationClicked()
    {
        EvaluateType(AchievementType.SecretClick);
    }

    // ------------------ Evaluation ------------------

    private void EvaluateType(AchievementType type) => EvaluateType(type, default(RunEndInfo));

    private void EvaluateType(AchievementType type, RunEndInfo endInfo)
    {
        foreach (var a in database.achievements)
        {
            if (a.type != type) continue;
            if (IsUnlocked(a.id)) continue;

            if (IsSatisfied(a, endInfo))
                Unlock(a);
        }
    }

    private bool IsSatisfied(AchievementsDatabase.AchievementEntry a, RunEndInfo endInfo)
    {
        switch (a.type)
        {
            case AchievementType.KillFirstEnemy:
                return _run.killsInRun >= 1;

            case AchievementType.KillEnemiesInRun:
                return _run.killsInRun >= a.targetInt;

            case AchievementType.KillEnemiesTotal:
                return _save.lifetimeKills >= a.targetInt;

            case AchievementType.KillWithAbilitiesOnlyInRun:
                return _run.abilityKillsInRun >= a.targetInt;

            case AchievementType.SurviveWavesInRun:
                return _run.wavesSurvived >= a.targetInt;

            case AchievementType.DieAfterWave:
                return endInfo.died && endInfo.waveReached >= a.targetInt;

            case AchievementType.CastFireballsInRun:
                return _run.fireballsInRun >= a.targetInt;

            case AchievementType.CastFireballsTotal:
                return _save.lifetimeFireballs >= a.targetInt;

            case AchievementType.SpawnUnitsInRun:
                return _run.unitsSpawnedInRun >= a.targetInt;

            case AchievementType.UnitsAliveSimultaneous:
                return _run.peakUnitsAlive >= a.targetInt;

            case AchievementType.LoseUnitsInRun:
                return _run.unitsLostInRun >= a.targetInt;

            case AchievementType.SpawnAnyUnitMaxTimes:
            {
                foreach (var kv in _spawnCountByType)
                {
                    var unitType = kv.Key;
                    var count = kv.Value;
                    if (_maxAllowedByType.TryGetValue(unitType, out int maxAllowed) && maxAllowed > 0)
                    {
                        if (count >= maxAllowed) return true;
                    }
                }
                return false;
            }

            case AchievementType.BeatWavesWithOnlyOneUnitType:
                return _wavesClearedWithOnlyOneUnitType >= a.targetInt;

            case AchievementType.BeatWaveWithoutSpawningAny:
                return _wavesClearedWithoutSpawning >= a.targetInt;

            case AchievementType.ClearWaveWithoutSpawningOrCasting:
                return _wavesClearedWithoutSpawningOrCasting >= a.targetInt;

            case AchievementType.StartFirstRun:
                return _save.lifetimeRunsStarted >= 1;

            case AchievementType.DieWithinSeconds:
                return endInfo.died && endInfo.runSeconds <= a.targetFloat;

            case AchievementType.DieWithGoldAtLeast:
                return endInfo.died && endInfo.goldAtDeathOrEnd >= a.targetInt;

            case AchievementType.DieTimesTotal:
                return _save.lifetimeDeaths >= a.targetInt;

            case AchievementType.PressButtonsInRun:
                return _run.buttonsPressedInRun >= a.targetInt;

            case AchievementType.UnlockXAchievements:
                return _save.unlockedIds.Count >= a.targetInt;

            case AchievementType.UnlockAllAchievements:
            {
                // Ignore “UnlockAllAchievements” itself if you want it to unlock last
                var totalNonMeta = database.achievements.Count(x => x.type != AchievementType.UnlockAllAchievements);
                return _save.unlockedIds.Count >= totalNonMeta;
            }

            case AchievementType.SecretClick:
                return true;

            default:
                return false;
        }
    }

    private void Unlock(AchievementsDatabase.AchievementEntry a)
    {
        _save.unlockedIds.Add(a.id);
        Debug.Log($"Achievement unlocked: {a.id}");

        // Completionist / Master might unlock due to this unlock:
        SaveNow();

        AchievementUnlocked?.Invoke(a);

        // Immediately re-check meta achievements that depend on count
        EvaluateType(AchievementType.UnlockXAchievements);
        EvaluateType(AchievementType.UnlockAllAchievements);
    }

    private bool IsUnlocked(string id) => _save.unlockedIds.Contains(id);

    public bool IsAchievementUnlocked(string id)
    {
        return _save != null && _save.unlockedIds.Contains(id);
    }

    public int UnlockedCount => _save?.unlockedIds.Count ?? 0;

    // ------------------ Save/Load ------------------

    [Serializable]
    private class SaveData
    {
        public HashSet<string> unlockedIds = new();

        public int lifetimeKills;
        public int lifetimeFireballs;
        public int lifetimeDeaths;
        public int lifetimeRunsStarted;
    }

    [Serializable]
    private struct RunStats
    {
        public int killsInRun;
        public int abilityKillsInRun;

        public int wavesSurvived;

        public int fireballsInRun;

        public int unitsSpawnedInRun;
        public int unitsLostInRun;

        public int unitsAlive;
        public int peakUnitsAlive;

        public int currentGold;
        public int buttonsPressedInRun;
    }

    private void SaveNow()
    {
        // HashSet isn't directly JSON-friendly with Unity's JsonUtility, so wrap as list
        var wrapper = new SaveWrapper
        {
            unlockedIds = _save.unlockedIds.ToList(),
            lifetimeKills = _save.lifetimeKills,
            lifetimeFireballs = _save.lifetimeFireballs,
            lifetimeDeaths = _save.lifetimeDeaths,
            lifetimeRunsStarted = _save.lifetimeRunsStarted
        };

        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private SaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return new SaveData();

        var json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrWhiteSpace(json))
            return new SaveData();

        var wrapper = JsonUtility.FromJson<SaveWrapper>(json);
        var data = new SaveData
        {
            lifetimeKills = wrapper.lifetimeKills,
            lifetimeFireballs = wrapper.lifetimeFireballs,
            lifetimeDeaths = wrapper.lifetimeDeaths,
            lifetimeRunsStarted = wrapper.lifetimeRunsStarted,
            unlockedIds = new HashSet<string>(wrapper.unlockedIds ?? new List<string>())
        };
        return data;
    }

    [Serializable]
    private class SaveWrapper
    {
        public List<string> unlockedIds;

        public int lifetimeKills;
        public int lifetimeFireballs;
        public int lifetimeDeaths;
        public int lifetimeRunsStarted;
    }
}
