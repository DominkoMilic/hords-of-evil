using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public GameObject languagePanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;
    public GameObject lorePanel;

    public Text playText;
    public Text exitText;
    public Text languageText;
    public Text difficultyText;
    public Text loreText;
    public Text settingsText;
    public Text closeSettingsText;
    public Text roleText;
    public Text weaknessText;
    public Text loreDescriptionText;
    public Text exitLoreText;
    public Text settingsTitleText;

    const string PREF_KEY = "SelectedLanguage";

    string[] playTextOptions = new string[]
    {
        "Play",
        "Jugar",
        "Igraj"
    };

    string[] exitTextOptions = new string[]
    {
        "Exit",
        "Salir",
        "Izlaz"
    };

    string[] languageTextOptions = new string[]
    {
        "Language",
        "Idioma",
        "Jezik"
    };

    string[] difficultyTextOptions = new string[]
    {
        "Difficulty",
        "Dificultad",
        "Težina"
    };

    string[] loreTextOptions = new string[]
    {
        "Lore",
        "Historia",
        "Priča"
    };

    string[] settingsTextOptions = new string[]
    {
        "Settings",
        "Configuración",
        "Postavke"
    };

    string[] closeSettingsTextOptions = new string[]
    {
        "Close",
        "Cerrar",
        "Zatvori"
    };

    string[] roleTextOptions = new string[]
    {
        "Role",
        "Rol",
        "Uloga"
    };

    string[] weaknessTextOptions = new string[]
    {
        "Weakness",
        "Debilidad",
        "Slabost"
    };


    void Start(){
        Game.SelectedLanguage = (Language)PlayerPrefs.GetInt(PREF_KEY, (int)Language.English);
        updateLanguageTexts();
    }

    public void updateLanguageTexts(){
        int idx = (int)Game.SelectedLanguage;
        idx = Mathf.Clamp(idx, 0, playTextOptions.Length - 1);

        playText.text = playTextOptions[idx];
        exitText.text = exitTextOptions[idx];
        languageText.text = languageTextOptions[idx];
        difficultyText.text = difficultyTextOptions[idx];
        loreText.text = loreTextOptions[idx];
        settingsText.text = settingsTextOptions[idx];
        closeSettingsText.text = closeSettingsTextOptions[idx];
        roleText.text = roleTextOptions[idx] + ":";
        weaknessText.text = weaknessTextOptions[idx] + ":";
        loreDescriptionText.text = loreTextOptions[idx] + ":";
        exitLoreText.text = closeSettingsTextOptions[idx];
        settingsTitleText.text = settingsTextOptions[idx];
    }

    public void handleExitButtonClick(){
        Application.Quit();

        PlaySoundEffect();
    }

    public void handleInfintyGameModeButtonClick(){
        SceneManager.LoadScene("GameScene");

        PlaySoundEffect();
    }

    public void handleLanguageButtonClick(){
        languagePanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleDifficultyButtonClick(){
        difficultyPanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleSettingsButtonClick(){
        settingsPanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleLoreButtonClick(){
        lorePanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleCloseSettingsPanelButtonClick(){
        settingsPanel.SetActive(false);

        PlaySoundEffect();
    }

    public void handleCloseLorePanelButtonClick(){
        lorePanel.SetActive(false);

        PlaySoundEffect();
    }

    private void PlaySoundEffect(){
        if (AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayButtonClick();
        }
    }
}
