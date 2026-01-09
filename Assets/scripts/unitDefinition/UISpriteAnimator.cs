using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnimator : MonoBehaviour
{
    [SerializeField] private Image target;

    private Sprite[] frames;
    private int fps = 10;

    private int index;
    private float timer;
    private bool playing;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<Image>();
    }
    

    private void OnDisable()
    {
        playing = false;
    }

    private void Reset()
    {
        target = GetComponent<Image>();
    }

    public void Play(Sprite[] newFrames, int newFps)
    {
        frames = newFrames;
        fps = Mathf.Max(1, newFps);

        index = 0;
        timer = 0f;

        playing = frames != null && frames.Length > 0;

        if (target != null)
            target.sprite = playing ? frames[0] : null;
    }

    public void Stop(Sprite fallback = null)
    {
        playing = false;
        if (target != null) target.sprite = fallback;
    }

    private void Update()
    {
        if (!playing || target == null || frames == null || frames.Length == 0) return;

        timer += Time.unscaledDeltaTime;
        float frameTime = 1f / fps;

        while (timer >= frameTime)
        {
            timer -= frameTime;
            index = (index + 1) % frames.Length;
            target.sprite = frames[index];
        }
    }
}
