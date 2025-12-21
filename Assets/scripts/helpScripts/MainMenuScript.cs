using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public GameObject languagePanel;
    public GameObject difficultyPanel;

    public Text playText;
    public Text exitText;
    public Text languageText;
    public Text difficultyText;

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
    }

    public void handleExitButtonClick(){
        Application.Quit();
    }

    public void handleInfintyGameModeButtonClick(){
        SceneManager.LoadScene("GameScene");
    }

    public void handleLanguageButtonClick(){
        languagePanel.SetActive(true);
    }

    public void handleDifficultyButtonClick(){
        difficultyPanel.SetActive(true);
    }
}
