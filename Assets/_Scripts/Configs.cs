using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Configs : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public Toggle fullScreen;
    private float volumeAtual;

    public void SetVolume(float volume)
    {
        volumeAtual = volume;
        if (volume <= 0.0001f)
        {
            audioMixer.SetFloat("Volume", -80f); // Mudo
        }
        else
        {
            audioMixer.SetFloat("Volume", Mathf.Log10(volume) * 20f);
        }
    }
}
