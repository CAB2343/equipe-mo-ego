using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Configs : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public Toggle fullScreen;
    private float volume;

    void Start()
    {
        volume = 1f;
        volumeSlider.value = volume;
        SetVolume();
    }

    public void SetVolume()
    {
        volume = volumeSlider.value;
        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("Volume", -80f); // Mudo
        }
        else
        {
            audioMixer.SetFloat("Volume", Mathf.Log10(volume) * 20f);
        }
    }

    public void SaveChanges()
    {
        if (fullScreen.isOn)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
            Screen.SetResolution(1920, 1080, false);
        }
    }
}
