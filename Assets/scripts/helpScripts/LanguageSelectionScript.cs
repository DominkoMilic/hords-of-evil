using UnityEngine;

public class LanguageSelectionScript : MonoBehaviour
{
    public GameObject languagePanel;
    public RectTransform selectedLanguageIndicator;
    
    public RectTransform englishButton;
    public RectTransform croatianButton;
    public RectTransform spainButton;
    
    private MainMenuScript mainMenu;

    const string PREF_KEY = "SelectedLanguage";
    
    RectTransform ButtonRectFor(Language language)
    {
        switch (language)
        {
            case Language.Croatian:
                return croatianButton;
            case Language.Spanish:
                return spainButton;
            case Language.English:
            default:
                return englishButton;
        }
    }

    void Awake()
    {
        if (mainMenu == null)
            mainMenu = FindFirstObjectByType<MainMenuScript>();
    }

    void Start()
    {
        Game.SelectedLanguage = (Language)PlayerPrefs.GetInt(PREF_KEY, (int)Language.English);
        MoveTo(ButtonRectFor(Game.SelectedLanguage));

        if(mainMenu != null){
            mainMenu.updateLanguageTexts();
        }
    }

    void Save()
    {
        PlayerPrefs.SetInt(PREF_KEY, (int)Game.SelectedLanguage);
        PlayerPrefs.Save();
    }

    public void MoveTo(RectTransform target) {
        Vector2 newPos = selectedLanguageIndicator.anchoredPosition;
        newPos.y = target.anchoredPosition.y;
        selectedLanguageIndicator.anchoredPosition = newPos;
    }

    void Apply(Language lang)
    {
        Game.SelectedLanguage = lang;
        Save();
        MoveTo(ButtonRectFor(lang));

        if (mainMenu != null)
            mainMenu.updateLanguageTexts();

        languagePanel.SetActive(false);
    }

    public void handleEnglishButtonClick(){
        Apply(Language.English);

        PlaySoundEffect();
    }

    public void handleSpanishButtonClick(){
        Apply(Language.Spanish);

        PlaySoundEffect();
    }

    public void handleCroatianhButtonClick(){
        Apply(Language.Croatian);

        PlaySoundEffect();
    }

    private void PlaySoundEffect(){
        if (AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayButtonClick();
        }
    }
}
