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

    void Start()
    {
    }

    void Update()
    {

    }

    public void Jogar()
    {
        sound.Stop();
        camTransition.StartMonitorCurveTransition(0f, 3f);
        camTransition.StartDitheringTransition(0f, 3f);
        StartCoroutine(CurveTransitionRoutine(3f));
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

    public void SairJogo()
    {
        Application.Quit();
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
    }
}
