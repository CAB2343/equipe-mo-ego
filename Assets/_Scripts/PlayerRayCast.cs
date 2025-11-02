using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRayCast : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayDistance = 5f;
    public float LugarDoRaio = 0.80f;
    public KeyCode interactKey = KeyCode.F;

    [Header("Dialogue Settings")]
    public GameObject DialogueText;
    public string[] dialogues; // array com as falas do NPC
    private int currentDialogue = 0;

    private Coroutine hideRoutine;
    private bool lookingAtCarlos;

    private Text textComponent;

    void Start()
    {
        if (DialogueText)
        {
            DialogueText.SetActive(false);
            textComponent = DialogueText.GetComponent<Text>();
            if (textComponent == null)
            {
                Debug.LogError("DialogueText precisa ter um componente Text!");
            }
        }
    }

    void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * LugarDoRaio;
        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(rayOrigin, transform.forward * rayDistance, Color.red);

        bool hitCarlos = Physics.Raycast(ray, out hit, rayDistance) && hit.collider.CompareTag("Carlos");

        if (hitCarlos)
        {
            Debug.DrawRay(rayOrigin, transform.forward * rayDistance, Color.green);

            if (!lookingAtCarlos)
            {
                lookingAtCarlos = true;
                if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
            }

            // Avança diálogo apenas se tecla F for pressionada
            if (Input.GetKeyDown(interactKey) && DialogueText && dialogues.Length > 0)
            {
                DialogueText.SetActive(true);

                // Atualiza a fala atual
                textComponent.text = dialogues[currentDialogue];

                // Avança para próxima fala
                currentDialogue++;
                if (currentDialogue >= dialogues.Length)
                {
                    currentDialogue = 0; // reinicia do começo
                }
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
