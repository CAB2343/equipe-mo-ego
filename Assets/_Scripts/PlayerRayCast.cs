using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    public float rayDistance = 5f;
    public float LugarDoRaio = 0.80f;
    public GameObject DialogueText;



    void Update()
    {
        Vector3 RayOrigin = transform.position + Vector3.up * LugarDoRaio;
        Ray ray = new Ray(RayOrigin, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(RayOrigin, transform.forward * rayDistance, Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if(hit.collider.CompareTag("Carlos"))
            {
                Debug.Log("Encostou em " + hit.collider.name);
                Debug.DrawRay(RayOrigin, transform.forward * rayDistance, Color.green);
                
                if(Input.GetKeyDown(KeyCode.E))
                {
                    DialogueText.SetActive(true);
                    Debug.Log("apertou");
                }

            }
        }
    }
}
