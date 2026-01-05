using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUIScript : MonoBehaviour
{
    [Header("Sliders (0..1)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Optional")]
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private Button testSfxButton;

    private bool ignoreEvents;

    private void Start()
    {
        float master = PlayerPrefs.GetFloat("vol_master", 1f);
        float music  = PlayerPrefs.GetFloat("vol_music", 1f);
        float sfx    = PlayerPrefs.GetFloat("vol_sfx", 1f);

        ignoreEvents = true;
        masterSlider.value = master;
        musicSlider.value  = music;
        sfxSlider.value    = sfx;
        ignoreEvents = false;

        ApplyAll();

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteChanged);

        if (testSfxButton != null)
            testSfxButton.onClick.AddListener(TestSfx);
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

        if (muteToggle != null)
            muteToggle.onValueChanged.RemoveListener(OnMuteChanged);

        if (testSfxButton != null)
            testSfxButton.onClick.RemoveListener(TestSfx);
    }

    private void ApplyAll()
    {
        var am = AudioManagerScript.Instance;
        if (am == null) return;

        am.SetMaster(masterSlider.value);
        am.SetMusic(musicSlider.value);
        am.SetSfx(sfxSlider.value);
    }

    private void OnMasterChanged(float v)
    {
        if (ignoreEvents) return;
        AudioManagerScript.Instance?.SetMaster(v);
    }

    private void OnMusicChanged(float v)
    {
        if (ignoreEvents) return;
        AudioManagerScript.Instance?.SetMusic(v);
    }

    private void OnSfxChanged(float v)
    {
        if (ignoreEvents) return;
        AudioManagerScript.Instance?.SetSfx(v);
    }

    private float lastMasterBeforeMute = 1f;

    private void OnMuteChanged(bool muted)
    {
        var am = AudioManagerScript.Instance;
        if (am == null) return;

        if (muted)
        {
            lastMasterBeforeMute = masterSlider.value;
            ignoreEvents = true;
            masterSlider.value = 0f;
            ignoreEvents = false;
            am.SetMaster(0f);
        }
        else
        {
            ignoreEvents = true;
            masterSlider.value = Mathf.Clamp01(lastMasterBeforeMute);
            ignoreEvents = false;
            am.SetMaster(masterSlider.value);
        }
    }

    private void TestSfx()
    {
        AudioManagerScript.Instance?.PlayButtonClick();
    }
}
