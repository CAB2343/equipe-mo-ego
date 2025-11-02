using UnityEngine;
using TMPro; // Adicione esta linha para usar o TextMeshPro

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações de Interação")]
    public float interactionDistance = 3f; // Distância máxima para interagir
    public KeyCode interactionKey = KeyCode.E; // Tecla para interagir
    public string targetTag = "Carlos"; // A Tag do objeto com qual interagir

    [Header("UI de Aviso")]
    public TextMeshProUGUI interactionPrompt; // Opcional: Texto para "Pressione E para conversar"

    private Camera playerCamera;
    private DialogueController currentDialogueController;
    private bool isInDialogue = false;

    void Start()
    {
        // Pega a câmera principal. O ideal é que seja a câmera do jogador.
        playerCamera = Camera.main;

        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Se já estiver em um diálogo, o Update vai cuidar de avançar as falas
        if (isInDialogue)
        {
            // Verifica se o jogador pressionou a tecla de interação novamente
            if (Input.GetKeyDown(interactionKey))
            {
                // Se a animação de texto ainda estiver rodando, pula a animação
                if (currentDialogueController.IsTextAnimating())
                {
                    currentDialogueController.SkipAnimation();
                }
                // Se a animação já terminou, avança para a próxima fala
                else
                {
                    currentDialogueController.DisplayNextSentence();
                }
            }

            // Se o diálogo terminou, reseta o estado
            if (!currentDialogueController.IsDialogueActive())
            {
                isInDialogue = false;
                currentDialogueController = null;
            }

            return; // Sai do Update para não fazer o Raycast durante o diálogo
        }

        // Se não estiver em diálogo, faz o Raycast para detectar NPCs
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Verifica se o objeto atingido tem a tag correta
            if (hit.collider.CompareTag(targetTag))
            {
                // Mostra o aviso de interação
                if (interactionPrompt != null)
                {
                    interactionPrompt.gameObject.SetActive(true);
                }

                // Tenta pegar o DialogueController do objeto
                currentDialogueController = hit.collider.GetComponent<DialogueController>();

                // Se o jogador pressionar a tecla de interação e encontrou um controlador de diálogo
                if (currentDialogueController != null && Input.GetKeyDown(interactionKey))
                {
                    isInDialogue = true;
                    currentDialogueController.StartDialogue();
                    if (interactionPrompt != null)
                    {
                        interactionPrompt.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // Se o Raycast não estiver atingindo o objeto correto, esconde o aviso
                if (interactionPrompt != null)
                {
                    interactionPrompt.gameObject.SetActive(false);
                }
                currentDialogueController = null;
            }
        }
        else
        {
            // Se o Raycast não atingir nada, esconde o aviso
            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(false);
            }
            currentDialogueController = null;
        }
    }
}