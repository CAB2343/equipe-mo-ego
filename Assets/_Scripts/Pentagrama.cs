using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pentagrama : MonoBehaviour
{
    public UiManager uiManager;
    public GameObject mula;
    public MenuManager menuManager;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiManager.ferraduras == 6)
            {
                Destroy(mula);
                menuManager.Venceu();
            }
            
        }
    }
}
