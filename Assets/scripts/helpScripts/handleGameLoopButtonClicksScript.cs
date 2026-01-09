using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class handleGameLoopButtonClicksScript : MonoBehaviour
{    
    public Button pauseGameButton;
    public GameObject pausePanel;
    public GameObject backgroundMusic;

    [Header("Secret Achievement Animation Settings")]
    [SerializeField] private RectTransform SecretAChievmentButton;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float staySeconds = 2f;
    [SerializeField] private float fadeDuration = 0.25f;

    private float disableSeconds = 4f;

    private void Start()
    {
        pauseGameButton.interactable = false;
        StartCoroutine(EnableButtonAfterDelay());
    }

    private IEnumerator EnableButtonAfterDelay()
    {
        yield return new WaitForSecondsRealtime(disableSeconds);
        pauseGameButton.interactable = true;
    }

    public void handlePauseGameClick(){
        pauseGameButton.gameObject.SetActive(false);
        backgroundMusic.GetComponent<AudioSource>().mute = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        PlaySoundEffect();
    }

    public void handleResumeGameClick(){
        pauseGameButton.gameObject.SetActive(true);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        backgroundMusic.GetComponent<AudioSource>().mute = false;
    
        PlaySoundEffect();
    }

    public void handleRestartGameClick(){
        Time.timeScale = 1f;
        
        EnemyBaseScript.allEnemies?.Clear();
        SoldierBaseScript.allSoldiers?.Clear();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    
        PlaySoundEffect();
    }

    public void handleExitButtonClick(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
        
        PlaySoundEffect();
    }

    private void PlaySoundEffect(){
        if (AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayButtonClick();
        }
    }
    
    public void OnCroatianFlagClicked()
    {
        AchievementEvents.EmitSecretDecorationClicked();
        
        if (SecretAChievmentButton != null)
            StartCoroutine(AnimateAndHideSecretButton());
    }

    
    private IEnumerator AnimateAndHideSecretButton()
    {
        CanvasGroup cg = SecretAChievmentButton.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = SecretAChievmentButton.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 1f;

        Vector2 startPos = SecretAChievmentButton.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, -405f);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / moveDuration);
            p = 1f - Mathf.Pow(1f - p, 3f);

            SecretAChievmentButton.anchoredPosition =
                Vector2.LerpUnclamped(startPos, targetPos, p);

            yield return null;
        }

        SecretAChievmentButton.anchoredPosition = targetPos;

        yield return new WaitForSecondsRealtime(staySeconds);

        float fadeT = 0f;
        while (fadeT < fadeDuration)
        {
            fadeT += Time.unscaledDeltaTime;
            cg.alpha = 1f - (fadeT / fadeDuration);
            yield return null;
        }

        cg.alpha = 0f;

        SecretAChievmentButton.gameObject.SetActive(false);
    }
}
