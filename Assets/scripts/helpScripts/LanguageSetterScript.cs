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

    public Text maxSoldiersAmountText;


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

    string[] maxSoldiersAmountFormat = new string[]
    {
        "Max {0} reached ({1}/{2})",                
        "Máximo de {0} alcanzado ({1}/{2})",        
        "Maksimalan broj {0} dostignut ({1}/{2})"   
    };

    string[][] soldierNamesLocalized =
    {
        new string[] { "Swordsman", "Shieldman", "Spearman", "Archer" },

        new string[] { "Espadachín", "Escudero", "Lancero", "Arquero" },

        new string[] { "Mačevalaca", "Štitonoša", "Kopljanika", "Strijelaca" }
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
        
        if (maxSoldiersAmountText)
            maxSoldiersAmountText.gameObject.SetActive(false);
    }

    public string GetSoldierName(int soldierId)
    {
        int lang = (int)Game.SelectedLanguage;
        lang = Mathf.Clamp(lang, 0, soldierNamesLocalized.Length - 1);

        soldierId = Mathf.Clamp(soldierId, 0, soldierNamesLocalized[lang].Length - 1);

        return soldierNamesLocalized[lang][soldierId];
    }


    public string GetMaxSoldiersText(string soldierName, int current, int max)
    {
        int idx = (int)Game.SelectedLanguage;
        idx = Mathf.Clamp(idx, 0, maxSoldiersAmountFormat.Length - 1);

        return string.Format(
            maxSoldiersAmountFormat[idx],
            soldierName,
            current,
            max
        );
    }


}
