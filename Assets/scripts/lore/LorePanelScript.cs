using UnityEngine;
using UnityEngine.UI;

public class LorePanelScript : MonoBehaviour
{
    private const string PREF_KEY = "SelectedLanguage";

    [Header("Data")]
    [SerializeField] private LoreDatabase loreDatabase;
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

    [Header("UI Refs")]
    [SerializeField] private Text unitNameText;
    [SerializeField] private Image unitImage;
    [SerializeField] private Text unitRoleText;
    [SerializeField] private Text unitWeaknessText;
    [SerializeField] private Text unitLoreText;

    [Header("Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private int index = 0;

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
    }

    private void OnEnable()
    {
        LoadLanguageFromPrefs();
        index = Mathf.Clamp(index, 0, unitOrder.Length - 1);
        Refresh();
    }

    private void LoadLanguageFromPrefs()
    {
        int saved = PlayerPrefs.GetInt(PREF_KEY, (int)Language.English);
        saved = Mathf.Clamp(saved, 0, 2);
        currentLanguage = (Language)saved;
    }

    public void SetLanguage(Language language)
    {
        currentLanguage = language;
        PlayerPrefs.SetInt(PREF_KEY, (int)language);
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
    }

    public void Prev()
    {
        if (unitOrder == null || unitOrder.Length == 0) return;
        index--;
        if (index < 0) index = unitOrder.Length - 1;
        Refresh();
    }

    private void Refresh()
    {
        if (loreDatabase == null)
        {
            Debug.LogError("LorePanelController: LoreDatabase reference missing.");
            return;
        }

        if (unitOrder == null || unitOrder.Length == 0)
        {
            Debug.LogError("LorePanelController: unitOrder is empty.");
            return;
        }

        UnitId unitId = unitOrder[index];
        UnitLoreData data = loreDatabase.GetUnitLore(unitId);

        if (data == null)
        {
            Debug.LogWarning($"LorePanelController: No lore data found for {unitId}");
            SetPlaceholder(unitId);
            UpdateButtonInteractable();
            return;
        }

        if (unitNameText != null) unitNameText.text = data.GetName(currentLanguage);
        if (unitImage != null) unitImage.sprite = data.image;

        var texts = data.GetTexts(currentLanguage);
        if (texts == null || texts.Length < 3)
        {
            SetPlaceholder(unitId);
        }
        else
        {
            if (unitRoleText != null) unitRoleText.text = texts[0];
            if (unitWeaknessText != null) unitWeaknessText.text = texts[1];
            if (unitLoreText != null) unitLoreText.text = texts[2];
        }

        UpdateButtonInteractable();
    }

    private void UpdateButtonInteractable()
    {
        if (prevButton != null) prevButton.interactable = unitOrder.Length > 1;
        if (nextButton != null) nextButton.interactable = unitOrder.Length > 1;
    }

    private void SetPlaceholder(UnitId unitId)
    {
        if (unitNameText != null) unitNameText.text = unitId.ToString();
        if (unitImage != null) unitImage.sprite = null;

        if (unitRoleText != null) unitRoleText.text = "Role: ?";
        if (unitWeaknessText != null) unitWeaknessText.text = "Weakness: ?";
        if (unitLoreText != null) unitLoreText.text = "Lore: ?";
    }
}
