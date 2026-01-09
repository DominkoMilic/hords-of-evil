using UnityEngine;
using UnityEngine.UI;

public class AchievementItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;

    [Header("Lock visuals")]
    [SerializeField] private Image lockOverlay;
    [SerializeField] private bool hideDetailsWhenLocked = false;

    private AchievementsDatabase.AchievementEntry entry;

    public void Bind(AchievementsDatabase.AchievementEntry e, bool unlocked)
    {
        entry = e;

        if (iconImage != null) iconImage.sprite = e.icon;
        if (titleText != null) titleText.text = e.title;
        if (descriptionText != null) descriptionText.text = e.description;

        SetUnlocked(unlocked);
    }

    public void SetUnlocked(bool unlocked)
    {
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!unlocked);

        if (hideDetailsWhenLocked && !unlocked)
        {
            if (titleText != null) titleText.text = "???";
            if (descriptionText != null) descriptionText.text = "Locked";
        }
        else
        {
            if (titleText != null) titleText.text = entry.title;
            if (descriptionText != null) descriptionText.text = entry.description;
        }
    }
}
