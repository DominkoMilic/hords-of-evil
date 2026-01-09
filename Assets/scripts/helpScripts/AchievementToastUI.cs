using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class AchievementToastUI : MonoBehaviour
{
    [Header("UI References (drag from children)")]
    [SerializeField] private RectTransform panel;      
    [SerializeField] private Text titleText;             
    [SerializeField] private Text descriptionText;        
    [SerializeField] private Image iconImage;         

    [Header("Manager (auto-found if null)")]
    [SerializeField] private AchievementManager achievementManager;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float staySeconds = 3f;

    [Tooltip("How far above the shown position the toast starts/ends (positive Y goes UP when anchored to top).")]
    [SerializeField] private float hiddenYOffset = 250f;

    [Tooltip("Also fade in/out using CanvasGroup alpha.")]
    [SerializeField] private bool fade = true;

    private CanvasGroup canvasGroup;
    private Coroutine routine;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    private void Awake()
    {
        if (panel == null) panel = GetComponent<RectTransform>();

        shownPosition = panel.anchoredPosition;

        hiddenPosition = shownPosition + Vector2.up * hiddenYOffset;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetHiddenImmediate();

        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindManager();
    }

    private void Start()
    {
        RebindManager();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Unsubscribe();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindManager();
    }

    private void RebindManager()
    {
        Unsubscribe();

        if (achievementManager == null)
            achievementManager = AchievementManager.instance;

        if (achievementManager == null)
            achievementManager = FindFirstObjectByType<AchievementManager>();

        if (achievementManager != null)
            achievementManager.AchievementUnlocked += OnAchievementUnlocked;
    }


    private void Unsubscribe()
    {
        if (achievementManager != null)
            achievementManager.AchievementUnlocked -= OnAchievementUnlocked;
    }

    private void OnAchievementUnlocked(AchievementsDatabase.AchievementEntry entry)
    {
        if (titleText != null) titleText.text = entry.title;
        if (descriptionText != null) descriptionText.text = entry.description;

        if (iconImage != null)
        {
            if (entry.icon != null)
            {
                iconImage.sprite = entry.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetInteractable(false);
        canvasGroup.alpha = 1f;

        yield return Slide(hiddenPosition, shownPosition, slideDuration);

        yield return new WaitForSecondsRealtime(staySeconds);

        yield return Slide(shownPosition, hiddenPosition, slideDuration);

        canvasGroup.alpha = 0f;
        routine = null;
    }


    private IEnumerator Slide(Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            p = 1f - Mathf.Pow(1f - p, 3f);

            panel.anchoredPosition = Vector2.LerpUnclamped(from, to, p);
            yield return null;
        }

        panel.anchoredPosition = to;
    }

    private void SetHiddenImmediate()
    {
        panel.anchoredPosition = hiddenPosition;
        if (fade) canvasGroup.alpha = 0f;
        SetInteractable(false);
    }

    private void SetInteractable(bool value)
    {
        canvasGroup.blocksRaycasts = value;
        canvasGroup.interactable = value;
    }
}
