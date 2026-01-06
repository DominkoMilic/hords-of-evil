using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GeneralManagerScript : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Text messageText;

    [Header("Timing")]
    [SerializeField] private float showSeconds = 2.2f;
    [SerializeField] private float slideSeconds = 0.35f;

    [Header("Slide")]
    [Tooltip("How far (in UI pixels) offscreen the panel starts/ends.")]
    [SerializeField] private float offscreenDistance = 900f;

    [Header("Sound")]
    [SerializeField] private bool playSound = true;

    public enum SlideFrom { Left, Right, Top, Bottom }
    [SerializeField] private SlideFrom slideFrom = SlideFrom.Left;

    private RectTransform panelRect;
    private Vector2 visiblePos;
    private Vector2 hiddenPos;

    private Coroutine showRoutine;

    [Header("First Wave")]
    [SerializeField] private float firstWaveDelay = 0.8f;

    private bool firstWaveShown = false;

    private string[] firstWaveMessages =
    {
        "Steady now. The enemy approaches.",
        "Steel ready. This is where we hold.",
        "Let them come. We are prepared.",
        "The first test is upon us.",
        "Stand firm. All battles begin somewhere."
    };

    private string[] fifthWaveMessages =
    {
        "Well fought! But this is far from over!",
        "They return in greater numbers — hold the line!",
        "Your courage is noted. Prepare for the next assault!",
        "The enemy adapts. So must we!",
        "This battle will be remembered!",
        "They throw everything they have at us now!",
        "Steel and fire! Show them no mercy!"
    };

    private string[] otherWaveMessages =
    {
        "There will be no retreat!",
        "They will break — or we will!",
        "Hold the line! HOLD!",
        "Every second we stand matters!",
        "This ground is paid for in blood!",
        "No reinforcements are coming. We are enough!",
        "Fight like legends, or die as footnotes!",
        "If this is the end — make it costly!",
        "Let the enemy remember this day!",
        "My arms ache… but we fight on!",
        "I have seen worse. Not many survived it.",
        "By the old gods… they just keep coming.",
        "We've come too far to fall now!"
    };

    private void Awake()
    {
        if (panelRoot != null)
        {
            panelRect = panelRoot.GetComponent<RectTransform>();
            visiblePos = panelRect.anchoredPosition;
            hiddenPos = GetHiddenPos(visiblePos);

            panelRect.anchoredPosition = hiddenPos;
            panelRoot.SetActive(false);
        }
    }

    public void DisplayGeneralMessage(int currentWave)
    {
        int wave = currentWave + 1;

        string msg = null;

        if (wave == 1)
            msg = Pick(firstWaveMessages);
        else if (wave % 5 == 0 && wave < 50)
            msg = Pick(fifthWaveMessages);
        else if (wave >= 50 && wave % 2 == 0)
            msg = Pick(otherWaveMessages);

        if (string.IsNullOrEmpty(msg))
            return;

        ShowMessage("General: " + msg);
    }

    private string Pick(string[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }

    private void ShowMessage(string text)
    {
        if (panelRoot == null || messageText == null || panelRect == null)
        {
            Debug.LogWarning("GeneralManagerScript: Assign panelRoot and messageText in Inspector.");
            Debug.Log(text);
            return;
        }

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(
            firstWaveShown
                ? SlideRoutine(text)
                : SlideRoutineWithDelay(text, firstWaveDelay)
        );
        firstWaveShown = true;
    }

    private IEnumerator SlideRoutine(string text)
    {
        if(AudioManagerScript.Instance != null && playSound)
            AudioManagerScript.Instance.PlayScrollOpen();

        panelRoot.SetActive(true);
        messageText.text = text;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        panelRect.anchoredPosition = hiddenPos;

        float t = 0f;
        while (t < slideSeconds)
        {
            t += Time.unscaledDeltaTime;
            float u = t / slideSeconds;

            panelRect.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, u);
            group.alpha = u; 
            yield return null;
        }

        panelRect.anchoredPosition = visiblePos;
        group.alpha = 1f;

        yield return new WaitForSecondsRealtime(showSeconds);

        t = 0f;
        while (t < slideSeconds)
        {
            t += Time.unscaledDeltaTime;
            float u = t / slideSeconds;

            panelRect.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, u);
            group.alpha = 1f - u;
            yield return null;
        }

        group.alpha = 0f;
        panelRoot.SetActive(false);
    }


    private Vector2 GetHiddenPos(Vector2 onScreen)
    {
        switch (slideFrom)
        {
            case SlideFrom.Left:   return onScreen + new Vector2(-offscreenDistance, 0f);
            case SlideFrom.Right:  return onScreen + new Vector2(+offscreenDistance, 0f);
            case SlideFrom.Top:    return onScreen + new Vector2(0f, +offscreenDistance);
            case SlideFrom.Bottom: return onScreen + new Vector2(0f, -offscreenDistance);
            default:               return onScreen;
        }
    }

    private IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / seconds);

            u = u * u * (3f - 2f * u);

            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, u);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    private IEnumerator SlideRoutineWithDelay(string text, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        yield return SlideRoutine(text);
    }
    
}
