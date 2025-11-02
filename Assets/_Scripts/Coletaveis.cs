using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coletaveis : MonoBehaviour
{
    public UiManager ui;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            switch (gameObject.tag)
            {
                case "Ferradura":
                    ui.SomaFerradura();
                    break;
                case "Corda":
                    ui.AlterCorda(true);
                    break;
            }
            Destroy(gameObject);
            Debug.Log("UUUU LEGAL VOCÊ PEGOU A PORRINHA");
        }
    }







}
