using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundsManager : MonoBehaviour
{
    // Definição de lista
    public List<AudioClip> listSoundEffects = new List<AudioClip>();

    // Definição de variaveis audio
    private AudioClip sound;
    public AudioSource audioSource;
    public AudioMixer audioMixer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SoundPlay(int index)
    {
        sound = listSoundEffects[index];
        audioMixer.GetFloat("Volume", out float effectsVolume);
        audioSource.PlayOneShot(sound, effectsVolume); 
    }
}
