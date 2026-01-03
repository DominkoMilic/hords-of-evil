using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameLoop : MonoBehaviour
{
    private InputActions input;

    public Text waveText;
    public Text coinsText;
    public Text manaText;

    private bool isGameOver = false;
    public GameObject gameOverPanel;
    public Button pauseGameButton;

    public GameObject backgroundMusic;
    public GameObject gameOverMusic;

    public Button dropFireballButton;
    public GameObject fireball;
    public GameObject fireballSpawner;
    [SerializeField] private float fireballCooldown = 60f;
    private float fireballReadyAt = 0f; 
    [SerializeField] private Image fireballCooldownImage;
    private float fireballCooldownStartedAt = -1f;

    private int currentCoins;
    private int currentWave = 0;          // wave index / counter
    private int totalWaveNumber;
    private int currentMana = 0;

    public int waveEnemyKillCount = 0;    // how many enemies died this wave
    public int enemiesInCurrentWave = 0;  // how many enemies were spawned this wave
    public bool shouldNewWaveStart = false;
    public int totalKills = 0;

    private bool isFireballSelected = false;
    private Vector3 fireballThrowPosition;

    public LevelData level;

    public SoldierStats[] swordsmanStats;
    public SoldierStats[] shieldmanStats;
    public SoldierStats[] spearmanStats;
    public SoldierStats[] archerStats;

    private SoldierStats[][] allSoldierStats;

    public int maxSpawnedSoldiersPerType = 15;

    private int[] aliveSoldiersPerType = new int[] { 0, 0, 0, 0 };

    public LevelUpgradeData[] swordsmanUpgradeLevels;
    public LevelUpgradeData[] shieldmanUpgradeLevels;
    public LevelUpgradeData[] spearmanUpgradeLevels;
    public LevelUpgradeData[] archerUpgradeLevels;

    private LevelUpgradeData[][] allSoldiersUpgradeLevels;

    // swordsman, shieldman, spearman, archer
    private int[] levelForSoldier = new int[] { 0, 0, 0, 0 };

    void Awake()
    {
        allSoldiersUpgradeLevels = new LevelUpgradeData[][]
        {
            swordsmanUpgradeLevels,
            shieldmanUpgradeLevels,
            spearmanUpgradeLevels,
            archerUpgradeLevels
        };

        allSoldierStats = new SoldierStats[][]
        {
            swordsmanStats,
            shieldmanStats,
            spearmanStats,
            archerStats
        };

        input = new InputActions();

        fireballCooldownStartedAt = Time.time;
        fireballReadyAt = Time.time + fireballCooldown;
        fireballCooldownImage.fillAmount = 1f;
        dropFireballButton.interactable = false;
    }

private bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }

    private void OnEnable()
    {
        if (input == null)
            input = new InputActions();

        input.Player.Enable();

        input.Player.Tap.performed += OnTap;
    }

    private void OnDisable()
    {
        if (input == null) return;

        input.Player.Tap.performed -= OnTap;

        input.Player.Disable();
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        if (isFireballSelected && !IsOverUI(screenPos))
        {
            if (!IsFireballReady())
            {
                isFireballSelected = false;
                dropFireballButton.GetComponent<Image>().color = Color.white;
                return;
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            worldPos.z = 0f;

            GameObject newFireball = Instantiate(fireball, fireballSpawner.transform.position, Quaternion.identity);
            newFireball.GetComponent<FireballScript>().Initialize(worldPos);

            StartFireballCooldown();

            isFireballSelected = false;
            dropFireballButton.GetComponent<Image>().color = Color.white;
        }
    }


    void Start()
    {
        backgroundMusic.gameObject.SetActive(true);
        currentCoins = level.startCoins;

        // Normal level (finite waves)
        if (level.levelNumber != 100)
        {
            totalWaveNumber = level.totalWaveNumber;
            updateWaveText(false);
        }
        // Endless level
        else
        {
            updateWaveText(true);
        }

        updateCoinsText();
    }

    // ----------------- GAME STATE -----------------

    public bool getIsGameOver()
    {
        return isGameOver;
    }

    public void setIsGameOver(bool newState)
    {
        isGameOver = newState;
        Time.timeScale = 0f;
        pauseGameButton.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        this.enabled = false;
        backgroundMusic.gameObject.SetActive(false);
        gameOverMusic.gameObject.SetActive(true);
    }

    // ----------------- FIREBALL -----------------

    public bool getIsFireballSelected()
    {
        return isFireballSelected;
    }

    public void setIsFireballSelected(bool newState)
    {
        isFireballSelected = newState;
    }

    // ----------------- SOLDIER STATS / UPGRADES -----------------

    public SoldierStats getAllSoldierStats(int soldierID, int soldierLevel)
    {
        return allSoldierStats[soldierID][soldierLevel];
    }

    public LevelUpgradeData getAllsoldiersUpgradeLevel(int soldierId, int soldierLevel)
    {
        if (soldierLevel >= 2)
            return allSoldiersUpgradeLevels[soldierId][2];
        else
            return allSoldiersUpgradeLevels[soldierId][soldierLevel];
    }

    public int getLevelForSoldier(int index)
    {
        return levelForSoldier[index];
    }

    public void upgradeLevelForSoldier(int index)
    {
        levelForSoldier[index]++;
    }

    // ----------------- COINS -----------------

    public void addCoins(int coinsToAdd)
    {
        currentCoins += coinsToAdd;
        updateCoinsText();
    }

    public int getCurrentCoins()
    {
        return currentCoins;
    }

    private void updateCoinsText()
    {
        coinsText.text = currentCoins.ToString();
    }

    // ----------------- WAVES -----------------

    public int getCurrentWave()
    {
        return currentWave;
    }

    // Called when a new wave has been spawned
    public void updateCurrentWave()
    {
        currentWave++;
        updateWaveText(true);
    }

    /// <summary>
    /// Called (usually from EnemyBaseScript.enemyDeath)
    /// to check if we should start the next wave.
    /// </summary>
    public void shouldNewWaveDeployChecker()
    {
        int numberOfEnemiesInWave = 0;

        // ENDLESS MODE: use the count provided by the spawner
        if (level.levelNumber == 100)
        {
            numberOfEnemiesInWave = enemiesInCurrentWave;
        }
        else
        {
            // FINITE MODE: sum enemyCount from level data,
            // but guard against going past the waves array.
            if (currentWave < 0 || currentWave >= level.waves.Length)
            {
                // No more waves defined; you could also end the game here
                shouldNewWaveStart = false;
                return;
            }

            SingleWaveStats currentWaveData = level.waves[currentWave];

            foreach (TroopPerWave troop in currentWaveData.troopPerWaveInfo)
            {
                numberOfEnemiesInWave += troop.enemyCount;
            }
        }

        // If wave has no enemies defined, never trigger new wave from kills
        if (numberOfEnemiesInWave <= 0)
        {
            shouldNewWaveStart = false;
            return;
        }

        shouldNewWaveStart = (waveEnemyKillCount >= numberOfEnemiesInWave);
    }

    public void updateWaveText(bool isEndless)
    {
        if (isEndless)
        {
            waveText.text = totalKills.ToString();
        }
        else
        {
            // +1 if you want to show 1-based wave numbers
            int displayWave = currentWave + 1;
            waveText.text = "Wave: " + displayWave.ToString() + "/" + totalWaveNumber.ToString();
        }
    }

    // ----------------- MANA -----------------

    public void addMana(int manaToAdd)
    {
        if (currentMana < 25 || manaToAdd < 0)
        {
            currentMana += manaToAdd;
            updateManaText();
        }

        if (currentMana == 25 && manaToAdd >= 0)
        {
            dropFireballButton.gameObject.SetActive(true);
        }
    }

    private void updateManaText()
    {
        manaText.text = currentMana.ToString() + " / 25";
    }

    public int getMana()
    {
        return currentMana;
    }

    // ----------------- INPUT HELPER -----------------

    private Vector3 getScreenTapPosition()
    {
        #if UNITY_EDITOR
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Vector2 screenPos = Mouse.current.position.ReadValue();
                    return new Vector3(screenPos.x, screenPos.y, 0f);
                }
        #else
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                {
                    Vector2 screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    return new Vector3(screenPos.x, screenPos.y, 0f);
                }
        #endif
        return Vector3.zero;
    }

    public bool CanSpawn(int soldierId)
    {
        return aliveSoldiersPerType[soldierId] < maxSpawnedSoldiersPerType;
    }

    public void RegisterSpawn(int soldierId)
    {
        aliveSoldiersPerType[soldierId]++;
    }

    public void RegisterDeath(int soldierId)
    {
        aliveSoldiersPerType[soldierId] = Mathf.Max(0, aliveSoldiersPerType[soldierId] - 1);
    }

    public int GetAliveCount(int soldierId) => aliveSoldiersPerType[soldierId];

    public bool IsFireballReady()
    {
        return Time.time >= fireballReadyAt;
    }

    public void StartFireballCooldown()
    {
        
        fireballCooldownStartedAt = Time.time;
        fireballReadyAt = Time.time + fireballCooldown;

        fireballCooldownImage.fillAmount = 1f;
    }

    private void Update()
    {
        dropFireballButton.interactable = IsFireballReady();

        if (fireballCooldownStartedAt < 0f)
            return;

        float remaining = fireballReadyAt - Time.time;

        if (remaining <= 0f)
        {
            fireballCooldownImage.fillAmount = 0f;
            fireballCooldownStartedAt = -1f;
            return;
        }

        fireballCooldownImage.fillAmount = remaining / fireballCooldown;
    }



}
