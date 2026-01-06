using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemySpawnerScript : MonoBehaviour
{
    [Serializable]
    public struct UnitPrefab
    {
        public UnitId id;
        public GameObject prefab;
    }

    public GameLoop game;
    private LevelData levelData;
    public GeneralManagerScript generalManager;

    [Header("Prefab Mapping (safer than array indexes)")]
    [SerializeField] private UnitPrefab[] prefabMap;

    [Header("Endless settings")]
    [SerializeField] private int endlessBaseEnemies = 5;

    // Threat budget tuning (main knobs)
    [SerializeField] private int baseThreatBudget = 20;      // wave 1 budget
    [SerializeField] private float threatGrowth = 4.0f;      // budget growth per wave
    [SerializeField] private float threatGrowthRamp = 0.15f; // makes later waves grow faster

    [Header("Spawn randomization")]
    [SerializeField] private float horizontalSpread = 2f;
    [SerializeField] private float verticalSpread = 0.5f;

    // Runtime lookup
    private readonly Dictionary<UnitId, GameObject> prefabById = new();

    [Header("Wave Start SFX")]
    [SerializeField] private bool playWaveStartSfx = true;
    [SerializeField] private float waveSfxCooldown = 2f;

    private float nextWaveSfxTime = -999f;


    // “Threat cost” per enemy (your earlier values)
    // IMPORTANT: These do NOT need to match bounty, they are for difficulty composition.
    private readonly Dictionary<UnitId, int> threat = new()
    {
        { UnitId.Goblin, 4 },
        { UnitId.GoblinArcher, 5 },
        { UnitId.HeavyGoblin, 7 },
        { UnitId.OrcWarrior, 11 },
        { UnitId.OrcArcher, 11 },
        { UnitId.OrcShaman, 12 },
        { UnitId.Ogre, 14 },
        { UnitId.OgreCrusher, 18 },
    };

    private void Awake()
    {
        prefabById.Clear();
        foreach (var p in prefabMap)
        {
            if (p.prefab != null)
                prefabById[p.id] = p.prefab;
        }
    }

    private void Start()
    {
        if (!game)
        {
            game = FindFirstObjectByType<GameLoop>();
            if (!game) return;
        }

        levelData = game.level;
        spawnNewWave();
        game.updateCurrentWave();
    }

    private void Update()
    {
        if (!game) return;

        if (game.shouldNewWaveStart)
        {
            spawnNewWave();
            game.shouldNewWaveStart = false;
            game.updateCurrentWave();
        }
    }

    void spawnNewWave()
    {
        int currentWave = game.getCurrentWave();

        if(generalManager != null)
            generalManager.DisplayGeneralMessage(currentWave);

        PlayWaveStartSfx();

        if (levelData.levelNumber == 100)
            SpawnEndlessWave(currentWave);
        else
            SpawnNormalWave(currentWave);
    }

    void SpawnNormalWave(int currentWave)
    {
        List<TroopPerWave> enemiesInCurrentWave = levelData.waves[currentWave].troopPerWaveInfo;

        game.waveEnemyKillCount = 0;
        game.enemiesInCurrentWave = 0;

        foreach (TroopPerWave troop in enemiesInCurrentWave)
        {
            for (int i = 0; i < troop.enemyCount; i++)
            {
                // If your TroopPerWave stores GameObject: keep it
                // If it stores UnitId: call SpawnSpecificEnemy(UnitId)
                SpawnSpecificEnemy(troop.enemyType);
                game.enemiesInCurrentWave++;
            }
        }
    }

    private void FilterPoolToAvailable(List<(UnitId id, float weight)> pool)
    {
        pool.RemoveAll(e => !prefabById.ContainsKey(e.id) || !threat.ContainsKey(e.id));
    }


    // ------------------------------
    // ENDLESS: budget-based wave fill
    // ------------------------------
    void SpawnEndlessWave(int waveIndex)
{
    game.waveEnemyKillCount = 0;

    int w = waveIndex + 1;

    int budget = Mathf.RoundToInt(
        baseThreatBudget
        + w * threatGrowth
        + (w * w) * threatGrowthRamp
    );

    int maxEnemies =
        endlessBaseEnemies
        + Mathf.FloorToInt(waveIndex * 0.75f)
        + Mathf.Max(0, waveIndex - 10);

    maxEnemies = Mathf.Max(endlessBaseEnemies, maxEnemies);

    // Build + filter pool
    var pool = BuildEndlessPool(waveIndex);
    FilterPoolToAvailable(pool);

    if (pool.Count == 0)
    {
        Debug.LogWarning("Endless pool is empty (no prefabs mapped).");
        game.enemiesInCurrentWave = 0;
        return;
    }

    int spawned = 0;
    int spent = 0;
    int safety = 500;

    while (spawned < maxEnemies && safety-- > 0)
    {
        UnitId pick = WeightedPick(pool);

        if (!threat.TryGetValue(pick, out int t))
            continue;

        if (spent + t > budget && spawned >= endlessBaseEnemies)
            break;

        spent += t;
        SpawnSpecificEnemy(pick);
        spawned++;
    }

    game.enemiesInCurrentWave = spawned;
}


    private List<(UnitId id, float weight)> BuildEndlessPool(int waveIndex)
{
    if (waveIndex < 2)
    {
        return new List<(UnitId, float)> { (UnitId.Goblin, 1.00f) };
    }
    else if(waveIndex < 4)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.Goblin, 0.60f),
            (UnitId.HeavyGoblin, 0.40f),
        };
    }
    else if (waveIndex < 8)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.Goblin, 0.40f),
            (UnitId.GoblinArcher, 0.30f),
            (UnitId.HeavyGoblin, 0.30f),
        };
    }
    else if (waveIndex < 10)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.Goblin, 0.30f),
            (UnitId.GoblinArcher, 0.35f),
            (UnitId.HeavyGoblin, 0.25f),
            (UnitId.OrcWarrior, 0.10f),
        };
    }
    else if (waveIndex < 13)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.Goblin, 0.15f),
            (UnitId.GoblinArcher, 0.25f),
            (UnitId.HeavyGoblin, 0.25f),
            (UnitId.OrcWarrior, 0.25f),
            (UnitId.OrcArcher, 0.10f),
        };
    }
    else if (waveIndex < 16)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.Goblin, 0.05f),
            (UnitId.GoblinArcher, 0.15f),
            (UnitId.HeavyGoblin, 0.25f),
            (UnitId.OrcWarrior, 0.30f),
            (UnitId.OrcArcher, 0.25f),
        };
    }
    else if (waveIndex < 20)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.HeavyGoblin, 0.20f),
            (UnitId.OrcWarrior, 0.30f),
            (UnitId.OrcArcher, 0.25f),
            (UnitId.OrcShaman, 0.25f),
        };
    }
    else if (waveIndex < 25)
    {
        return new List<(UnitId, float)>
        {
            (UnitId.HeavyGoblin, 0.15f),
            (UnitId.OrcWarrior, 0.25f),
            (UnitId.OrcArcher, 0.20f),
            (UnitId.OrcShaman, 0.20f),
            (UnitId.Ogre, 0.20f),
        };
    }
    else
    {
        return new List<(UnitId, float)>
        {
            (UnitId.HeavyGoblin, 0.20f),
            (UnitId.OrcWarrior, 0.25f),
            (UnitId.OrcArcher, 0.15f),
            (UnitId.OrcShaman, 0.12f),
            (UnitId.Ogre, 0.20f),
            (UnitId.OgreCrusher, 0.08f),
        };
    }
}


    private UnitId WeightedPick(List<(UnitId id, float weight)> pool)
    {
        float total = 0f;
        for (int i = 0; i < pool.Count; i++) total += Mathf.Max(0f, pool[i].weight);

        float r = UnityEngine.Random.value * total;
        float acc = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            acc += Mathf.Max(0f, pool[i].weight);
            if (r <= acc) return pool[i].id;
        }

        return pool[pool.Count - 1].id;
    }

    // Overload: spawn by UnitId
    void SpawnSpecificEnemy(UnitId id)
    {
        if (!prefabById.TryGetValue(id, out GameObject prefab) || prefab == null)
            return;

        SpawnSpecificEnemy(prefab);
    }

    // Existing: spawn by prefab
    void SpawnSpecificEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        float offsetX = UnityEngine.Random.Range(-horizontalSpread, horizontalSpread);
        float offsetY = UnityEngine.Random.Range(-verticalSpread, verticalSpread);
        Vector3 spawnPos = transform.position + new Vector3(offsetX, offsetY, 0f);

        GameObject newEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        EnemyBaseScript enemyBaseScript = newEnemy.GetComponent<EnemyBaseScript>();
        if (enemyBaseScript != null)
            enemyBaseScript.Initialize();
    }

    private void PlayWaveStartSfx()
    {
        if (!playWaveStartSfx) return;
        if (AudioManagerScript.Instance == null) return;

        if (Time.time < nextWaveSfxTime) return;
        nextWaveSfxTime = Time.time + waveSfxCooldown;

        System.Action[] sounds =
        {
            AudioManagerScript.Instance.PlayWaveStart1,
            AudioManagerScript.Instance.PlayWaveStart2,
            AudioManagerScript.Instance.PlayWaveStart3,
            AudioManagerScript.Instance.PlayWaveStart4
        };

        int randomIndex = UnityEngine.Random.Range(0, sounds.Length);
        sounds[randomIndex]?.Invoke();
    }
}
