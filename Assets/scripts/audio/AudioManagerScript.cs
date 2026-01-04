using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{
    public static AudioManagerScript Instance;

    [Header("SFX")]
    public AudioClip upgradeSound;
    public AudioClip buttonClickSound;
    public AudioClip swordDrawSound;
    public AudioClip hornBlowSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayUpgrade()
    {
        audioSource.PlayOneShot(upgradeSound);
    }

    public void PlayButtonClick()
    {
        audioSource.PlayOneShot(buttonClickSound);
    }

    public void PlaySwordDraw()
    {
        audioSource.PlayOneShot(swordDrawSound);
    }

    public void PlayHornBlow()
    {
        audioSource.PlayOneShot(hornBlowSound);
    }
}
