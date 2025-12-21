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
    public SoldierStats[] mageStats;

    private SoldierStats[][] allSoldierStats;

    public LevelUpgradeData[] swordsmanUpgradeLevels;
    public LevelUpgradeData[] shieldmanUpgradeLevels;
    public LevelUpgradeData[] spearmanUpgradeLevels;
    public LevelUpgradeData[] mageUpgradeLevels;

    private LevelUpgradeData[][] allSoldiersUpgradeLevels;

    // swordsman, shieldman, spearman, mage
    private int[] levelForSoldier = new int[] { 0, 0, 0, 0 };

    void Awake()
    {
        allSoldiersUpgradeLevels = new LevelUpgradeData[][]
        {
            swordsmanUpgradeLevels,
            shieldmanUpgradeLevels,
            spearmanUpgradeLevels,
            mageUpgradeLevels
        };

        allSoldierStats = new SoldierStats[][]
        {
            swordsmanStats,
            shieldmanStats,
            spearmanStats,
            mageStats
        };

        input = new InputActions();
    }

    private bool IsOverUI(Vector2 screenPosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }

    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();
        input.Player.TapPosition.performed += OnTapPosition;
    }

    private void OnDisable()
    {
        if (input == null) return;
        input.Player.TapPosition.performed -= OnTapPosition;
        input.Player.Disable();
    }

    private void OnTapPosition(InputAction.CallbackContext context)
    {
        Vector2 screenPos = context.ReadValue<Vector2>();

        if (isFireballSelected && !IsOverUI(screenPos))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            fireballThrowPosition = worldPos;

            dropFireballButton.gameObject.SetActive(false);
            addMana(-25);

            GameObject newFireball = Instantiate(
                fireball,
                fireballSpawner.transform.position,
                Quaternion.identity
            );

            FireballScript fireballScript = newFireball.GetComponent<FireballScript>();
            fireballScript.Initialize(worldPos);

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
}
