using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public GameObject languagePanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;
    public GameObject enemyCharactersPanel;
    public GameObject soldierCharactersPanel;
    public GameObject CharactersPanel;
    public GameObject volumePanel;
    public GameObject AchievementsPanel;

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

    public void handleVolumeButtonClick(){
        volumePanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleCloseSingleSettingPanelButtonClick(){
        if(languagePanel.activeSelf)
            languagePanel.SetActive(false);
        else if(difficultyPanel.activeSelf)
            difficultyPanel.SetActive(false);
        else if(volumePanel.activeSelf)
            volumePanel.SetActive(false);

        PlaySoundEffect();
    }

    public void handleCharactersButtonClick(){
        CharactersPanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleCloseCharactersPanelButtonClick(){
        CharactersPanel.SetActive(false);

        PlaySoundEffect();
    }

    public void handleCloseSettingsPanelButtonClick(){
        settingsPanel.SetActive(false);

        PlaySoundEffect();
    }

    public void handleEnemyCharactersButtonClick(){
        enemyCharactersPanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleSoldierCharactersButtonClick(){
        soldierCharactersPanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleCloseEnemyCharactersPanelButtonClick(){
        enemyCharactersPanel.SetActive(false);

        PlaySoundEffect();
    }

    public void handleCloseSoldierCharactersPanelButtonClick(){
        soldierCharactersPanel.SetActive(false);

        PlaySoundEffect();
    }

    private void PlaySoundEffect(){
        if (AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayButtonClick();
        }
    }

    public void handleAchievementsButtonClick(){
        AchievementsPanel.SetActive(true);

        PlaySoundEffect();
    }

    public void handleCloseAchievementsPanelButtonClick(){
        AchievementsPanel.SetActive(false);

        PlaySoundEffect();
    }
}
