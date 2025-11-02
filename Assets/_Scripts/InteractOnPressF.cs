using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider))]
public class InteractHideEnterExit : MonoBehaviour
{
    [Header("Player detection")]
    public string playerTag = "Player";
    public Transform playerTransform; // optional direct reference
    public bool returnOnExit = true;  // volta ao sair da trigger

    [Header("Cinemachine (VCams)")]
    public CinemachineVirtualCamera vcamMain;
    public CinemachineVirtualCamera vcamInteract;

    [Header("Priority behavior")]
    public int interactPriorityOverride = -1; // se >0 usa esse valor
    public int priorityBoost = 10;            // se override == -1 usa main + boost

    [Header("Behavior")]
    public MonoBehaviour playerControllerToDisable; // optional: seu script de movimento
    public GameObject promptUI;                    // optional: "Press F"
    public float interactDuration = 0f;            // 0 = manual (não usado nesse fluxo)

    [Header("Animator / States")]
    public Animator doorAnimator;              // animator que contém os estados
    public string hideTrigger = "Esconder";    // trigger para animação de entrar (escondendo)
    public string hideStateName = "escondendo";// state name no Animator após disparo
    public string exitTrigger = "Sair";        // trigger para animação de sair (saindo)
    public string exitStateName = "saindo";    // state name para a animação de saída

    // internal state
    bool playerNearby = false;
    bool inInteractMode = false;
    bool isHidden = false; // true quando a animação "escondendo" terminou
    Coroutine autoReturnCoroutine = null;

    int initialMainPriority = 0;
    int initialInteractPriority = 0;
    bool prioritiesStored = false;

    // cache opcional do capsule do player caso queira reativar depois
    CapsuleCollider cachedPlayerCapsule = null;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void Awake()
    {
        if (vcamMain != null) initialMainPriority = vcamMain.Priority;
        if (vcamInteract != null) initialInteractPriority = vcamInteract.Priority;
        prioritiesStored = (vcamMain != null && vcamInteract != null);
    }

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!inInteractMode)
            {
                EnterInteractMode();
            }
            else
            {
                // se estamos no modo de interação:
                // - se ainda não terminou a animação de esconder, ignoramos (ou você pode cancelar)
                // - se já está escondido, então apertar F toca a animação de saída
                if (isHidden)
                {
                    StartCoroutine(PlayExitAndRestore());
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            playerNearby = true;
            if (promptUI != null && !inInteractMode) promptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            playerNearby = false;
            if (promptUI != null) promptUI.SetActive(false);

            if (returnOnExit && inInteractMode)
            {
                // Se o jogador sair enquanto em modo de interação, força saída (roda a animação de sair se já estiver escondido, senão só restaura)
                StartCoroutine(ForceRestore());
            }
        }
    }

    bool IsPlayer(Collider other)
    {
        if (playerTransform != null)
            return other.transform == playerTransform || other.transform.IsChildOf(playerTransform);
        else
            return other.CompareTag(playerTag);
    }

    // ---------- Core flow ----------
    void EnterInteractMode()
    {
        if (vcamInteract == null)
        {
            Debug.LogWarning("[Interact] vcamInteract não atribuída.");
            return;
        }

        // disable player control
        if (playerControllerToDisable != null) playerControllerToDisable.enabled = false;

        // hide prompt
        if (promptUI != null) promptUI.SetActive(false);

        // determina prioridade alvo
        int targetPriority;
        if (interactPriorityOverride > 0) targetPriority = interactPriorityOverride;
        else if (prioritiesStored) targetPriority = initialMainPriority + priorityBoost;
        else
        {
            int mainP = (vcamMain != null) ? vcamMain.Priority : 10;
            targetPriority = mainP + priorityBoost;
        }

        // aplica somente na vcamInteract
        vcamInteract.Priority = targetPriority;
        inInteractMode = true;
        isHidden = false;

        // espera o blend terminar, então toca a animação de esconder
        StartCoroutine(WaitForBlendThenHide());
    }

    void ExitInteractModeImmediateRestore()
    {
        // Restaura prioridade original da vcamInteract
        if (vcamInteract != null) vcamInteract.Priority = initialInteractPriority;

        // reativa o player
        if (playerControllerToDisable != null) playerControllerToDisable.enabled = true;

        inInteractMode = false;
        isHidden = false;

        if (playerNearby && promptUI != null) promptUI.SetActive(true);
    }

    IEnumerator ForceRestore()
    {
        // Se já estamos escondidos, faça a animação de saída antes de restaurar.
        if (isHidden)
        {
            yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));
        }

        ExitInteractModeImmediateRestore();
    }

    // Ao apertar F novamente enquanto escondido -> toca "saindo" e depois restaura
    IEnumerator PlayExitAndRestore()
    {
        if (!inInteractMode || !isHidden) yield break;

        // toca a animação de saída e espera terminar
        yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));

        // restaura câmera / controles
        ExitInteractModeImmediateRestore();
    }

    // ---------- blend -> hide ----------
    IEnumerator WaitForBlendThenHide()
    {
        CinemachineBrain brain = null;
        if (Camera.main != null) brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null) brain = FindObjectOfType<CinemachineBrain>();

        if (brain == null)
        {
            Debug.LogWarning("[Interact] CinemachineBrain não encontrado. Tocando animação imediatamente.");
            yield return StartCoroutine(PlayAnimationAndWait(hideTrigger, hideStateName));
            yield break;
        }

        // espera um frame pra brain atualizar
        yield return null;

        // espera blend terminar e vcamInteract estar ativa
        while (brain.IsBlending || brain.ActiveVirtualCamera != (ICinemachineCamera)vcamInteract)
        {
            yield return null;
        }

        // aqui o blend já acabou — toca a animação de esconder e espera ela terminar
        yield return StartCoroutine(PlayAnimationAndWait(hideTrigger, hideStateName));
        isHidden = true;

        // se quiser um retorno automático, poderia iniciar autoReturnCoroutine aqui
        if (interactDuration > 0f)
        {
            if (autoReturnCoroutine != null) StopCoroutine(autoReturnCoroutine);
            autoReturnCoroutine = StartCoroutine(AutoReturnAfterSeconds(interactDuration));
        }
    }

    IEnumerator AutoReturnAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);

        // se estiver escondido, toca saída e depois restaura
        if (isHidden)
        {
            yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));
        }

        ExitInteractModeImmediateRestore();
    }

    // ---------- animações ----------
    /// <summary>
    /// Dispara um trigger (ou Play se trigger vazio) e espera até que o state indicado esteja ativo e completo.
    /// </summary>
    IEnumerator PlayAnimationAndWait(string triggerName, string stateName)
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("[Interact] doorAnimator não atribuído — ignorando animação.");
            yield break;
        }

        // dispara a animação
        if (!string.IsNullOrEmpty(triggerName))
        {
            doorAnimator.SetTrigger(triggerName);
        }
        else if (!string.IsNullOrEmpty(stateName))
        {
            doorAnimator.Play(stateName, 0, 0f);
        }
        else
        {
            Debug.LogWarning("[Interact] triggerName e stateName vazios — nada para tocar.");
            yield break;
        }

        // espera o Animator trocar para o state desejado
        // cuidado: pode demorar alguns frames até o Animator transitar
        bool stateReached = false;
        for (int i = 0; i < 60; i++) // evita loop infinito (fallback depois de ~1s)
        {
            var info = doorAnimator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName))
            {
                stateReached = true;
                break;
            }
            yield return null;
        }

        if (!stateReached)
        {
            // fallback: pode não conseguir detectar pelo nome (ex: camadas, wrappers). Dá um pequeno delay e sai.
            Debug.Log("[Interact] Não detectou state '" + stateName + "' — aguardando 0.2s como fallback.");
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        // quando o state for alcançado, espera até normalizedTime >= 1 (fim do clip)
        // se a animação estiver em loop, cuidado: esse método nunca completará. Assuma que suas animações NÃO estão em loop.
        while (true)
        {
            var info = doorAnimator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(stateName)) yield return null; // ainda mudando
            else
            {
                // normalizedTime >= 1 significa que ao menos 1 ciclo completo já passou
                if (info.normalizedTime >= 1f) break;
                else yield return null;
            }
        }
    }

    // ---------- utilidade: desativar CapsuleCollider do personagem ----------
    /// <summary>
    /// Desativa o CapsuleCollider do personagem (usa playerTransform se atribuído, ou procura por playerTag).
    /// </summary>
    public void DisablePlayerCapsuleCollider()
    {
        // tenta usar o cache se existir e ainda for válido
        if (cachedPlayerCapsule == null)
        {
            Transform root = playerTransform;
            if (root == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag(playerTag);
                if (playerGO != null) root = playerGO.transform;
            }

            if (root == null)
            {
                Debug.LogWarning("[Interact] Player não encontrado para desativar CapsuleCollider.");
                return;
            }

            // procura primeiro no root, depois em filhos
            cachedPlayerCapsule = root.GetComponent<CapsuleCollider>();
            if (cachedPlayerCapsule == null)
                cachedPlayerCapsule = root.GetComponentInChildren<CapsuleCollider>(true);
        }

        if (cachedPlayerCapsule == null)
        {
            Debug.LogWarning("[Interact] CapsuleCollider do player não encontrado.");
            return;
        }

        cachedPlayerCapsule.enabled = false;
    }

    // métodos públicos (para disparar via outros scripts, botões etc.)
    public void ForceEnterInteract() => EnterInteractMode();
    public void ForceExitInteract() => StartCoroutine(ForceRestore());
}