using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject options;
    [SerializeField] private string game;

    public void Jogar()
    {
        SceneManager.LoadScene(game);
    }

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
    }

    public void CloseOptions()
    {
        mainMenu.SetActive(true);
        options.SetActive(false);
    }

    public void SairJogo()
    {
        Application.Quit();
    }
}
