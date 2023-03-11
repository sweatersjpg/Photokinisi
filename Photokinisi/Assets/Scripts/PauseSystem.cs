using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseSystem : MonoBehaviour
{
    public static PauseSystem pauseSystem;
    public static bool paused;

    [Header("Panels")]

    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Space]
    public AudioMixer masterMixer;

    [Header("Settings")]

    [Range(40, 80)]
    public float FOV = 60;
    [Range(0, 1)]
    public float volume = 1;
    [Range(2, 6)]
    public float mouseSensitivity = 4;

    Slider FOVSlider;
    Slider volumeSlider;
    Slider mouseSensitivitySlider;
    Toggle fullscreenToggle;

    private void Awake()
    {
        if (pauseSystem == null)
        {
            pauseSystem = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        FOVSlider = settingsPanel.transform.Find("FOV").GetComponent<Slider>();
        volumeSlider = settingsPanel.transform.Find("volume").GetComponent<Slider>();
        mouseSensitivitySlider = settingsPanel.transform.Find("mouse").GetComponent<Slider>();
        fullscreenToggle = settingsPanel.transform.Find("fullscreen").GetComponent<Toggle>();

        float vol;
        masterMixer.GetFloat("musicVolume", out vol);
        volumeSlider.value = vol;

        mouseSensitivitySlider.value = mouseSensitivity;
        FOVSlider.value = FOV;

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P)) TogglePaused();
    }

    public void TogglePaused()
    {
        Cursor.lockState = !paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = !paused;
        Time.timeScale = !paused ? 0 : 1;
        SetActivePause(!paused);

        paused = !paused;
    }

    public void SetActivePause(bool state)
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(state);
    }

    public void SetActiveSettings(bool state)
    {
        settingsPanel.SetActive(state);
        pausePanel.SetActive(!state);
    }

    // ---- settings ----

    // toggle

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    // sliders

    public void UpdateVolume(float value)
    {
        masterMixer.SetFloat("musicVolume", value);
    }

    public void UpdateSensitivity(float value) => mouseSensitivity = value;

    public void UpdateFOV(float value) => FOV = value;

}
