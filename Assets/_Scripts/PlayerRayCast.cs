using System.Collections;
using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    public float rayDistance = 5f;
    public float LugarDoRaio = 0.80f;
    public GameObject DialogueText;
    public KeyCode interactKey = KeyCode.F;

    private Coroutine hideRoutine;
    private bool lookingAtCarlos;

    void Start()
    {
        if (DialogueText) DialogueText.SetActive(false);
    }

    void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * LugarDoRaio;
        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(rayOrigin, transform.forward * rayDistance, Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance) && hit.collider.CompareTag("Carlos"))
        {
            Debug.DrawRay(rayOrigin, transform.forward * rayDistance, Color.green);

            if (!lookingAtCarlos)
            {
                lookingAtCarlos = true;
                if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
            }

            if (Input.GetKeyDown(interactKey) && DialogueText)
            {
                DialogueText.SetActive(!DialogueText.activeSelf);
            }
        }
        else
        {
            if (lookingAtCarlos)
            {
                lookingAtCarlos = false;
                if (hideRoutine != null) StopCoroutine(hideRoutine);
                hideRoutine = StartCoroutine(HideAfterDelay(1f));
            }
        }
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (DialogueText) DialogueText.SetActive(false);
        hideRoutine = null;
    }
}