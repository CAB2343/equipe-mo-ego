using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject mainMenuButtons;
    [SerializeField] private GameObject options;
    [SerializeField] private CRTCurveTransition camTransition;
    [SerializeField] private AudioSource sound;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject canvasGame;
    [SerializeField] private GameObject perdeu;
    [SerializeField] private GameObject ganhou;
    [SerializeField] private SoundsManager soundsManager;

    void Start()
    {
    }

    void Update()
    {

    }

    public void Jogar()
    {
        sound.Stop();
        camTransition.StartMonitorCurveTransition(0f, 2f);
        camTransition.StartDitheringTransition(0f, 2f);
        StartCoroutine(CurveTransitionRoutine(2f));
    }

    public void OpenOptions()
    {
        mainMenuButtons.SetActive(false);
        options.SetActive(true);
    }

    public void CloseOptions()
    {
        mainMenuButtons.SetActive(true);
        options.SetActive(false);
    }
    
    public void Perdeu()
    {
        canvas.SetActive(true);
        perdeu.SetActive(true);
        canvasGame.SetActive(false);
        camTransition.StartMonitorCurveTransition(0.289f, 2f);
        camTransition.StartDitheringTransition(0.07f, 2f);
        sound.Stop();
        soundsManager.SoundPlay(3);
    }
    
    public void Venceu()
    {
        canvas.SetActive(true);
        ganhou.SetActive(true);
        canvasGame.SetActive(false);
        camTransition.StartMonitorCurveTransition(0.289f, 2f);
        camTransition.StartDitheringTransition(0.07f, 2f);
        sound.Stop();
        soundsManager.SoundPlay(4);
    }

    public void SairJogo()
    {
        Application.Quit();
    }
    
    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private IEnumerator CurveTransitionRoutine(float duracao)
    {
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            yield return null;
        }
        canvas.SetActive(false);
        mainMenu.SetActive(false);
        canvasGame.SetActive(true);
        soundsManager.ChangeTheme(1);
    }
}
