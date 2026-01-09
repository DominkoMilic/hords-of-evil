using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentUICameraBinder : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Bind();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Bind();
    }

    private void Bind()
    {
        if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null) return;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[UI] No Camera.main found to bind.");
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;

        if (canvas.planeDistance < 1f) canvas.planeDistance = 10f;

        Debug.Log($"[UI] Bound canvas to camera: {cam.name}");
    }
}
