using UnityEngine;
using UnityEngine.Audio;

public class AudioManagerScript : MonoBehaviour
{
    public static AudioManagerScript Instance;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private const string MASTER_VOL = "MasterVol";
    private const string MUSIC_VOL  = "MusicVol";
    private const string SFX_VOL    = "SfxVol";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX")]
    public AudioClip upgradeSound;
    public AudioClip buttonClickSound;

    [Header("Soldier Spawn")]
    public AudioClip swordDrawSound;
    public AudioClip hornBlowSound;
    public AudioClip areYouReadySound;
    public AudioClip spawnYellSound;

    [Header("Fireaball")]
    public AudioClip fireballHitSound;
    public AudioClip fireballWhooshSound;

    [Header("Enemy Wave Spawn")]
    public AudioClip waveStartSound1;
    public AudioClip waveStartSound2;
    public AudioClip waveStartSound3;
    public AudioClip waveStartSound4;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

       if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
    
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
    
        if (sfxGroup != null)  sfxSource.outputAudioMixerGroup = sfxGroup;
        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;

        SetMaster(PlayerPrefs.GetFloat("vol_master", 1f));
        SetMusic(PlayerPrefs.GetFloat("vol_music", 1f));
        SetSfx(PlayerPrefs.GetFloat("vol_sfx", 1f));
    }

    private float ToDb(float value01)
    {
        if (value01 <= 0.0001f) return -80f;
        return Mathf.Log10(value01) * 20f;
    }

    public void SetMaster(float value01)
    {
        if (mixer == null) return;
        mixer.SetFloat(MASTER_VOL, ToDb(value01));
        PlayerPrefs.SetFloat("vol_master", value01);
    }

    public void SetMusic(float value01)
    {
        if (mixer == null) return;
        mixer.SetFloat(MUSIC_VOL, ToDb(value01));
        PlayerPrefs.SetFloat("vol_music", value01);
    }

    public void SetSfx(float value01)
    {
        if (mixer == null) return;
        mixer.SetFloat(SFX_VOL, ToDb(value01));
        PlayerPrefs.SetFloat("vol_sfx", value01);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayUpgrade()        => PlaySfx(upgradeSound);
    public void PlayButtonClick()    => PlaySfx(buttonClickSound);
    public void PlaySwordDraw()      => PlaySfx(swordDrawSound);
    public void PlayHornBlow()       => PlaySfx(hornBlowSound);
    public void PlayFireballHit()    => PlaySfx(fireballHitSound);
    public void PlayFireballWhoosh() => PlaySfx(fireballWhooshSound);
    public void PlayAreYouReady()    => PlaySfx(areYouReadySound);
    public void PlaySpawnYell()      => PlaySfx(spawnYellSound);
    public void PlayWaveStart1()     => PlaySfx(waveStartSound1);
    public void PlayWaveStart2()     => PlaySfx(waveStartSound2);
    public void PlayWaveStart3()     => PlaySfx(waveStartSound3);
    public void PlayWaveStart4()     => PlaySfx(waveStartSound4);

    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (music == null || musicSource == null) return;
        musicSource.clip = music;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }
}
