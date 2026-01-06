using System;
using System.Collections;
using UnityEngine;

public class DoorTranisitonScript : MonoBehaviour
{
    [Header("Doors")]
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;

    [Header("Timing")]
    [SerializeField] private float openDuration = 1.0f;
    [SerializeField] private float startDelay = 0.15f;

    public event Action OnOpened;

    private Vector2 leftClosedPos;
    private Vector2 rightClosedPos;
    private Vector2 leftOpenPos;
    private Vector2 rightOpenPos;

    private void Awake()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("DoorTransition: Assign leftDoor and rightDoor.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        CachePositions();

        leftDoor.anchoredPosition = leftClosedPos;
        rightDoor.anchoredPosition = rightClosedPos;

        StartCoroutine(PlayIntro());
    }

    private void CachePositions()
    {
        float w = leftDoor.rect.width;

        leftClosedPos = leftDoor.anchoredPosition;
        rightClosedPos = rightDoor.anchoredPosition;

        leftOpenPos = leftClosedPos + Vector2.left * w;
        rightOpenPos = rightClosedPos + Vector2.right * w;
    }

    private IEnumerator PlayIntro()
    {
        float prevScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return UnscaledWait(startDelay);

        yield return SlideDoors(leftClosedPos, leftOpenPos, rightClosedPos, rightOpenPos, openDuration);

        GameFlowScript.StartGame();

        Time.timeScale = prevScale;

        OnOpened?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator SlideDoors(Vector2 lFrom, Vector2 lTo, Vector2 rFrom, Vector2 rTo, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            float eased = 1f - Mathf.Pow(1f - k, 3f);

            leftDoor.anchoredPosition = Vector2.LerpUnclamped(lFrom, lTo, eased);
            rightDoor.anchoredPosition = Vector2.LerpUnclamped(rFrom, rTo, eased);

            yield return null;
        }

        leftDoor.anchoredPosition = lTo;
        rightDoor.anchoredPosition = rTo;
    }

    private static IEnumerator UnscaledWait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
