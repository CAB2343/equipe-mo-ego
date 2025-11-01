using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coletaveis : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Debug.Log("UUUU LEGAL VOCÊ PEGOU A PORRINHA");
        }
    }







}
