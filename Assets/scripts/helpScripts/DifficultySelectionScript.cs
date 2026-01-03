using UnityEngine;

public class DifficultySelectionScript : MonoBehaviour
{
    public GameObject difficultyPanel;
    public RectTransform selectDifficultyIndicator;

    public RectTransform easyButton;
    public RectTransform normalButton;
    public RectTransform hardButton;

    const string PREF_KEY = "SelectedDifficulty";

    RectTransform ButtonRectFor(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return easyButton;
            case Difficulty.Hard:
                return hardButton;
            case Difficulty.Normal:
            default:
                return normalButton;
        }
    }

    void Start()
    {
        Game.SelectedDifficulty = (Difficulty)PlayerPrefs.GetInt(PREF_KEY, (int)Difficulty.Normal);
        MoveTo(ButtonRectFor(Game.SelectedDifficulty));
    }


    public void MoveTo(RectTransform target) {
        Vector2 newPos = selectDifficultyIndicator.anchoredPosition;
        newPos.y = target.anchoredPosition.y;
        selectDifficultyIndicator.anchoredPosition = newPos;
    }

    void Save() 
    { 
        PlayerPrefs.SetInt(PREF_KEY, (int)Game.SelectedDifficulty);
        PlayerPrefs.Save();
    }

    void Apply(Difficulty difficulty)
    {
        Game.SelectedDifficulty = difficulty;
        Save();
        MoveTo(ButtonRectFor(difficulty));

        difficultyPanel.SetActive(false);
    }

    public void handleEasyButtonClick(){
        Apply(Difficulty.Easy);

        PlaySoundEffect();
    }

    public void handleNormalButtonClick(){
        Apply(Difficulty.Normal);

        PlaySoundEffect();
    }

    public void handleHardButtonClick(){
        Apply(Difficulty.Hard);

        PlaySoundEffect();
    }

    private void PlaySoundEffect(){
        if (AudioManagerScript.Instance != null)
        {
            AudioManagerScript.Instance.PlayButtonClick();
        }
    }
}
