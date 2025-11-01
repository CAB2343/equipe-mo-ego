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
    [SerializeField] private string game;

    void Start()
    {
        //Time.timeScale = 0f;
    }

    public void Jogar()
    {
        //Time.timeScale = 1f;
        mainMenu.SetActive(false);
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
}
