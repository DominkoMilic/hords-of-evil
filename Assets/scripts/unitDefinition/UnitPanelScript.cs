using UnityEngine;
using UnityEngine.UI;

public class UnitPanelScript : MonoBehaviour
{
    private const string PREF_KEY_LANG = "SelectedLanguage";
    private const string PREF_KEY_LEVEL = "SelectedUnitLevel";

    [Header("Data")]
    [SerializeField] private UnitDatabase unitDatabase;
    [SerializeField] private Language currentLanguage = Language.English;

    [Header("Unit Order")]
    [SerializeField] private UnitId[] unitOrder = new UnitId[]
    {
        UnitId.Swordsman,
        UnitId.Spearman,
        UnitId.Shieldman,
        UnitId.Archer,
        UnitId.Goblin,
        UnitId.GoblinArcher,
        UnitId.HeavyGoblin,
        UnitId.OrcWarrior,
        UnitId.OrcArcher,
        UnitId.OrcShaman,
        UnitId.Ogre,
        UnitId.OgreCrusher
    };

    [Header("UI Refs - Lore")]
    [SerializeField] private Text unitNameText;
    [SerializeField] private Image unitImage;
    [SerializeField] private Text unitLoreText;
    [SerializeField] private UISpriteAnimator unitAnimator;

    [Header("UI Refs - Stats")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text damageText;
    [SerializeField] private Text armorText;
    [SerializeField] private Text economyText;
    [SerializeField] private Text rangeTypeText;
    [SerializeField] private Text damageTypeText;

    [Header("Level UI")]
    [SerializeField] private Dropdown levelDropdown; 
    [SerializeField, Range(1, 4)] private int currentLevel = 1;

    [Header("Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private int index = 0;

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);

        if (levelDropdown != null)
            levelDropdown.onValueChanged.AddListener(OnLevelDropdownChanged);
    }

    private void OnEnable()
    {
        LoadLanguageFromPrefs();
        LoadLevelFromPrefs();

        index = Mathf.Clamp(index, 0, unitOrder.Length - 1);

        SyncDropdownToCurrentLevel();

        Refresh();
    }

    private void LoadLanguageFromPrefs()
    {
        int saved = PlayerPrefs.GetInt(PREF_KEY_LANG, (int)Language.English);
        saved = Mathf.Clamp(saved, 0, 2);
        currentLanguage = (Language)saved;
    }

    private void LoadLevelFromPrefs()
    {
        currentLevel = Mathf.Clamp(PlayerPrefs.GetInt(PREF_KEY_LEVEL, 1), 1, 4);
    }

    private void SaveLevelToPrefs()
    {
        PlayerPrefs.SetInt(PREF_KEY_LEVEL, currentLevel);
        PlayerPrefs.Save();
    }

    private void SyncDropdownToCurrentLevel()
    {
        if (levelDropdown == null) return;

        int dropdownValue = Mathf.Clamp(currentLevel - 1, 0, 3);

        levelDropdown.SetValueWithoutNotify(dropdownValue);
    }

    private void OnLevelDropdownChanged(int dropdownValue)
    {
        currentLevel = Mathf.Clamp(dropdownValue + 1, 1, 4);
        SaveLevelToPrefs();
        Refresh();
    }

    public void SetLanguage(Language language)
    {
        currentLanguage = language;
        PlayerPrefs.SetInt(PREF_KEY_LANG, (int)language);
        PlayerPrefs.Save();
        Refresh();
    }

    public void ShowUnit(UnitId unitId)
    {
        int found = System.Array.IndexOf(unitOrder, unitId);
        if (found >= 0) index = found;
        Refresh();
    }

    public void Next()
    {
        if (unitOrder == null || unitOrder.Length == 0) return;
        index = (index + 1) % unitOrder.Length;
        Refresh();
        PlaySoundEffect();
    }

    public void Prev()
    {
        if (unitOrder == null || unitOrder.Length == 0) return;
        index--;
        if (index < 0) index = unitOrder.Length - 1;
        Refresh();
        PlaySoundEffect();
    }

    private void PlaySoundEffect()
    {
        if (AudioManagerScript.Instance != null)
            AudioManagerScript.Instance.PlayButtonClick();
    }

    private void Refresh()
    {
        if (unitDatabase == null)
        {
            Debug.LogError("UnitPanelScript: UnitDatabase reference missing.");
            return;
        }

        if (unitOrder == null || unitOrder.Length == 0)
        {
            Debug.LogError("UnitPanelScript: unitOrder is empty.");
            return;
        }

        UnitId unitId = unitOrder[index];

        UnitDefinition lvl1 = unitDatabase.Get(unitId, 1);
        if (lvl1 == null)
        {
            Debug.LogWarning($"UnitPanelScript: No unit data found for {unitId} (level 1)");
            SetPlaceholder(unitId);
            UpdateButtonInteractable();
            return;
        }

        bool isSoldier = (lvl1.side == UnitSide.Player);

        int levelToShow = isSoldier ? currentLevel : 1;

        if (levelDropdown != null)
        {
            levelDropdown.gameObject.SetActive(isSoldier);

            if (isSoldier)
                SyncDropdownToCurrentLevel();
        }

        UnitDefinition data = unitDatabase.Get(unitId, levelToShow);

        if (data == null)
        {
            Debug.LogWarning($"UnitPanelScript: Missing data for {unitId} level {levelToShow}. Falling back to level 1.");
            data = lvl1;
            levelToShow = 1;
        }

        if (unitNameText != null) unitNameText.text = data.GetName(currentLanguage);
        if (unitAnimator != null)
        {
            if (data.HasAnimation)
                unitAnimator.Play(data.frames, data.fps);
            else
                unitAnimator.Stop(null);
        }
        else
        {
            if (unitImage != null) unitImage.sprite = data.GetFirstFrame();
        }


        if (unitLoreText != null) unitLoreText.text = data.GetLore(currentLanguage);

        if (healthText != null) healthText.text = $"HP: {data.stats.maxHealth}";
        if (damageText != null) damageText.text = $"DMG: {data.stats.minDamage} - {data.stats.maxDamage}";
        if (armorText != null) armorText.text = $"P/M: {data.stats.armorPhysical} / {data.stats.armorMagical}";

        if (economyText != null)
        {
            if (data.side == UnitSide.Player)
                economyText.text = $"Cost: {data.economy.cost}";
            else if (data.side == UnitSide.Enemy)
                economyText.text = $"Bounty: {data.economy.bounty}";
            else
                economyText.text = $"Value: {data.GetCostOrBounty()}";
        }

        if (rangeTypeText != null) rangeTypeText.text = $"Range: {data.attackRangeType}";
        if (damageTypeText != null) damageTypeText.text = $"Type: {data.damageType}";

        UpdateButtonInteractable();
    }

    private void UpdateButtonInteractable()
    {
        bool many = unitOrder != null && unitOrder.Length > 1;
        if (prevButton != null) prevButton.interactable = many;
        if (nextButton != null) nextButton.interactable = many;
    }

    private void SetPlaceholder(UnitId unitId)
    {
        if (unitNameText != null) unitNameText.text = unitId.ToString();
        if (unitImage != null) unitImage.sprite = null;

        if (unitLoreText != null) unitLoreText.text = "Lore: ?";

        if (healthText != null) healthText.text = "HP: ?";
        if (damageText != null) damageText.text = "DMG: ? - ?";
        if (armorText != null) armorText.text = "P/M: ? / ?";
        if (economyText != null) economyText.text = "Cost/Bounty: ?";
        if (rangeTypeText != null) rangeTypeText.text = "Range: ?";
        if (damageTypeText != null) damageTypeText.text = "Type: ?";
    }
}
