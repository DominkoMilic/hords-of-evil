using UnityEngine;
using UnityEngine.UI;

public class LanguageSetterScript : MonoBehaviour
{
    public Text exitText;
    public Text resumeText;
    public Text restartText;
    public Text fireballText;

    public Text restartGameOverText;
    public Text exitGameOverText;


    const string PREF_KEY = "SelectedLanguage";

    string[] resumeTextOptions = new string[]
    {
        "Resume",
        "Continuar",
        "Nastavi"
    };

    string[] exitTextOptions = new string[]
    {
        "Exit",
        "Salir",
        "Izlaz"
    };

    string[] restartTextOptions = new string[]
    {
        "Restart",
        "Reiniciar",
        "Igraj opet"
    };

    string[] fireballTextOptions = new string[]
    {
        "Fire",
        "Fuego",
        "Vatra"
    };

    void Start()
    {
        Game.SelectedLanguage = (Language)PlayerPrefs.GetInt(PREF_KEY, (int)Language.English);
        updateLanguageTexts();
    }

    public void updateLanguageTexts(){
        int idx = (int)Game.SelectedLanguage;
        idx = Mathf.Clamp(idx, 0, resumeTextOptions.Length - 1);

        restartText.text = restartTextOptions[idx];
        exitText.text = exitTextOptions[idx];
        resumeText.text = resumeTextOptions[idx];
        restartGameOverText.text = restartTextOptions[idx];
        exitGameOverText.text = exitTextOptions[idx];
        fireballText.text = fireballTextOptions[idx];
    }

}
