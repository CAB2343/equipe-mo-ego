using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coletaveis : MonoBehaviour
{
    public UiManager ui;
    public MulaController mula;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            switch (gameObject.tag)
            {
                case "Ferradura":
                    ui.SomaFerradura();
                    mula.chaseSpeed += 2f;
                    mula.patrolSpeed += 1f;
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
