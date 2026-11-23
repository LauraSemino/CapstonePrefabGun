using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;

public class OptionsMenu : MonoBehaviour
{
    public PlayerControl player;

    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    void Start()
    {
        // Sensitivity
        sensitivitySlider.minValue = 0.2f;
        sensitivitySlider.maxValue = 1f;

        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
        sensitivitySlider.value = sensitivity;
        player.SetSensitivity(sensitivity);

        // Volume
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = volume;

        RuntimeManager.StudioSystem.setParameterByName("MasterVolume", volume);

        // Resolutions
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        foreach (Resolution resolution in resolutions)
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolution.width + " x " + resolution.height));
        }

        resolutionDropdown.RefreshShownValue();
    }

    public void ChangeSensitivity(float value)
    {
        Debug.Log(value);
        player.SetSensitivity(value);
        PlayerPrefs.SetFloat("Sensitivity", value);
    }

    public void ChangeVolume(float value)
    {
        RuntimeManager.StudioSystem.setParameterByName("MasterVolume", value);
        PlayerPrefs.SetFloat("Volume", value);
    }

    public void ChangeResolution(int index)
    {
        Resolution resolution = resolutions[index];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}

