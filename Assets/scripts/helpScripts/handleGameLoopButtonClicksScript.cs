using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class handleGameLoopButtonClicksScript : MonoBehaviour
{    
    public Button pauseGameButton;
    public GameObject pausePanel;
    public GameObject backgroundMusic;

    public void handlePauseGameClick(){
        pauseGameButton.gameObject.SetActive(false);
        backgroundMusic.GetComponent<AudioSource>().mute = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void handleResumeGameClick(){
        pauseGameButton.gameObject.SetActive(true);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        backgroundMusic.GetComponent<AudioSource>().mute = false;
    }

    public void handleRestartGameClick(){
        Time.timeScale = 1f;
        
        EnemyBaseScript.allEnemies?.Clear();
        SoldierBaseScript.allSoldiers?.Clear();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void handleExitButtonClick(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

}
