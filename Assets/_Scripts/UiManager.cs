using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    private int ferraduras = 0;
    private bool corda = false;
    public GameObject pngFerra;
    public GameObject pngCorda;
    public TextMeshProUGUI textFerra;
    
    // Start is called before the first frame update
    void Start()
    {
        pngFerra.SetActive(false);
        pngCorda.SetActive(false);
        textFerra.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (ferraduras > 0)
        {
            pngFerra.SetActive(true);
        }

        if (corda)
        {
            pngCorda.SetActive(true);
        }

        if (ferraduras > 1)
        {
            textFerra.text = "" + ferraduras;
        }
    }
    
    public void AlterCorda(bool newCorda)
    {
        corda = newCorda;
    }
        
    public void SomaFerradura()
    {
        ferraduras++;
    }
}
