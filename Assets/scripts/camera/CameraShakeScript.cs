using UnityEngine;

public class CameraShakeScript : MonoBehaviour
{
    public static CameraShakeScript Instance;

    private Vector3 originalPos;
    private float shakeTime;
    private float shakeMagnitude;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        originalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (shakeTime > 0f)
        {
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 0f;
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeTime = Mathf.Max(shakeTime, duration);
        shakeMagnitude = magnitude;
    }
}

