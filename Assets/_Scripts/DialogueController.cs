using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("Referências")]
    public TextAnimationController textAnimator; // Arraste o objeto com o TextAnimationController aqui
    public GameObject dialoguePanel; // O painel que contém o texto, para ativar/desativar

    [Header("Conteúdo do Diálogo")]
    [TextArea(3, 10)]
    public string[] sentences; // As 4 frases do seu diálogo

    private int currentSentenceIndex;
    private bool dialogueIsActive = false;

    void Start()
    {
        // Garante que o painel de diálogo comece desativado
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Inicia a conversa
    public void StartDialogue()
    {
        dialogueIsActive = true;
        currentSentenceIndex = 0;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Usa o método PlayText do seu animador
        textAnimator.PlayText(sentences[currentSentenceIndex]);
    }

    // Avança para a próxima frase
    public void DisplayNextSentence()
    {
        currentSentenceIndex++;

        // Se ainda houver frases, mostra a próxima
        if (currentSentenceIndex < sentences.Length)
        {
            textAnimator.PlayText(sentences[currentSentenceIndex]);
        }
        else
        {
            // Se acabaram as frases, termina o diálogo
            EndDialogue();
        }
    }

    // Pula a animação da frase atual
    public void SkipAnimation()
    {
        textAnimator.SkipTypewriter();
    }

    // Finaliza o diálogo
    private void EndDialogue()
    {
        dialogueIsActive = false;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Métodos para o PlayerInteraction saber o estado do diálogo
    public bool IsDialogueActive()
    {
        return dialogueIsActive;
    }

    public bool IsTextAnimating()
    {
        // Usa o método IsTyping() do seu animador
        return textAnimator.IsTyping();
    }
}