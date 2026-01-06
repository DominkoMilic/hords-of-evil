using UnityEngine;

public class ScaleBackgroundRootToCameraScript : MonoBehaviour
{
    [SerializeField] private SpriteRenderer referenceSprite;
    [SerializeField] private Camera cam;

    private Vector2 lastScreen;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        Resize();
        lastScreen = new Vector2(Screen.width, Screen.height);
    }

    private void Update()
    {
        if (Screen.width != lastScreen.x || Screen.height != lastScreen.y)
        {
            Resize();
            lastScreen = new Vector2(Screen.width, Screen.height);
        }
    }

    private void Resize()
    {
        if (referenceSprite == null || referenceSprite.sprite == null || cam == null || !cam.orthographic)
            return;

        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        Vector2 spriteSize = referenceSprite.sprite.bounds.size;

        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;

        float scale = Mathf.Max(scaleX, scaleY);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
