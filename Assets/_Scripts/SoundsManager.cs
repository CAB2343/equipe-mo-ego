using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundsManager : MonoBehaviour
{
    // Defini��o de lista
    public List<AudioClip> listSounds = new List<AudioClip>();

    // Defini��o de variaveis audio
    private AudioClip sound;
    public AudioSource audioSource;
    public AudioMixer audioMixer;
    private float oldVolume;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        oldVolume = audioSource.volume;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SoundPlay(int index)
    {
        sound = listSounds[index];
        audioMixer.GetFloat("Volume", out float effectsVolumeDB);
        float effectsVolume = Mathf.Pow(10f, effectsVolumeDB / 20f);
        audioSource.PlayOneShot(sound, effectsVolume); 
    }

    public void ChangeTheme(int index)
    {
        audioSource.Stop();
        audioSource.clip = listSounds[index];
        audioSource.loop = true;
        audioSource.Play();
    }

    public void AjustarVolumePorDistancia(float distancia, float maxDistance)
    {
        float fatorVolume = Mathf.Clamp01(1f - (distancia / maxDistance));
        //float novoVolume = Mathf.Clamp01(1 - (distancia / maxDistance)) * audioSource.volume;
        audioSource.volume = fatorVolume * 1;
        //audioSource.volume = novoVolume;
    }

    public void ReajustarVolume()
    {
        audioSource.volume = oldVolume;
    }
}
