using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnerScript : MonoBehaviour
{
    public GameLoop game;
    public GameObject[] enemy; 
    private LevelData levelData;

    [Header("Endless settings")]
    [SerializeField] private int endlessBaseEnemies = 5;

    [Header("Spawn randomization")]
    [SerializeField] private float horizontalSpread = 2f;  // left/right randomness
    [SerializeField] private float verticalSpread = 0.5f;  // up/down randomness

    private void Start()
    {
        if (!game)
        {
            game = FindFirstObjectByType<GameLoop>();
            if (!game) return;
        }

        if (game)
        {
            levelData = game.level;
            spawnNewWave();
            game.updateCurrentWave();
        }
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

        if (levelData.levelNumber == 100)
        {
            SpawnEndlessWave(currentWave);
        }
        else
        {
            SpawnNormalWave(currentWave);
        }
    }

    void SpawnNormalWave(int currentWave)
    {
        List<TroopPerWave> enemiesInCurrentWave =
            levelData.waves[currentWave].troopPerWaveInfo;

        foreach (TroopPerWave troop in enemiesInCurrentWave)
        {
            for (int i = 0; i < troop.enemyCount; i++)
            {
                SpawnSpecificEnemy(troop.enemyType);
            }
        }
    }

void SpawnEndlessWave(int waveIndex)
{
    int enemiesToSpawn =
        endlessBaseEnemies                         
        + Mathf.FloorToInt(waveIndex * 0.75f)      
        + Mathf.Max(0, waveIndex - 10);            

    if (enemiesToSpawn < endlessBaseEnemies)
        enemiesToSpawn = endlessBaseEnemies;

    game.waveEnemyKillCount = 0;
    game.enemiesInCurrentWave = enemiesToSpawn;

    for (int i = 0; i < enemiesToSpawn; i++)
    {
        GameObject prefab = GetEnemyForWave(waveIndex);
        if (prefab != null)
        {
            SpawnSpecificEnemy(prefab);
        }
    }
}



    void SpawnSpecificEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        // random offset around spawner
        float offsetX = Random.Range(-horizontalSpread, horizontalSpread);
        float offsetY = Random.Range(-verticalSpread, verticalSpread);

        Vector3 spawnPos = transform.position + new Vector3(offsetX, offsetY, 0f);

        GameObject newEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyBaseScript enemyBaseScript = newEnemy.GetComponent<EnemyBaseScript>();
        if (enemyBaseScript != null)
            enemyBaseScript.Initialize();
    }

    GameObject GetEnemyForWave(int waveIndex)
    {
        if (enemy == null || enemy.Length == 0)
            return null;

        GameObject goblin      = enemy.Length > 0 ? enemy[0] : null;
        GameObject heavyGoblin = enemy.Length > 1 ? enemy[1] : goblin;
        GameObject orc         = enemy.Length > 2 ? enemy[2] : heavyGoblin;

        float r = Random.value;

        if (waveIndex < 5)
        {
            // Waves 0–4: ONLY goblins, easy start
            return goblin;
        }
        else if (waveIndex < 10)
        {
            // Waves 5–9: 85% goblin, 15% heavy goblin (gentle intro)
            return (r < 0.85f) ? goblin : heavyGoblin;
        }
        else if (waveIndex < 16)
        {
            // Waves 10–15: 55% goblin, 35% heavy, 10% orc
            if (r < 0.55f) return goblin;
            if (r < 0.90f) return heavyGoblin;
            return orc;
        }
        else
        {
            // Waves 16+: 25% goblin, 40% heavy, 35% orc
            if (r < 0.25f) return goblin;
            if (r < 0.65f) return heavyGoblin;
            return orc;
        }
    }

}
