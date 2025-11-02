using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider))]
public class InteractHideEnterExit : MonoBehaviour
{
    [Header("Player detection")]
    public string playerTag = "Player";
    public Transform playerTransform; // referência opcional
    public bool returnOnExit = true;  // retorna se sair do trigger

    [Header("Cinemachine (VCams)")]
    public CinemachineVirtualCamera vcamMain;
    public CinemachineVirtualCamera vcamInteract;

    [Header("Priority behavior")]
    public int interactPriorityOverride = -1; // se >0 usa esse valor
    public int priorityBoost = 10;            // se override == -1 usa main + boost

    [Header("Behavior")]
    public MonoBehaviour playerControllerToDisable; // script de movimento
    public GameObject promptUI;                    // "Press F"
    public float interactDuration = 0f;            // 0 = manual

    [Header("Animator / States")]
    public Animator doorAnimator;              
    public string hideTrigger = "Esconder";    
    public string hideStateName = "escondendo";
    public string exitTrigger = "Sair";        
    public string exitStateName = "saindo";

    [Header("Esconderijo Transform")]
    public Transform esconderijoCamera; // transform da câmera do esconderijo

    // internal state
    private bool playerNearby = false;
    private bool inInteractMode = false;
    private bool isHidden = false; 
    private Coroutine autoReturnCoroutine = null;

    private int initialMainPriority = 0;
    private int initialInteractPriority = 0;
    private bool prioritiesStored = false;

    private Vector3 playerPosOriginal;
    private Quaternion playerRotOriginal;

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

        // salva posição e rotação original
        if (playerTransform != null)
        {
            playerPosOriginal = playerTransform.position;
            playerRotOriginal = playerTransform.rotation;

            // move player para o esconderijo
            if (esconderijoCamera != null)
            {
                playerTransform.position = esconderijoCamera.position;
                playerTransform.rotation = esconderijoCamera.rotation;
            }
        }

        // disable player control
        if (playerControllerToDisable != null) playerControllerToDisable.enabled = false;

        // hide prompt
        if (promptUI != null) promptUI.SetActive(false);

        // define prioridade
        int targetPriority;
        if (interactPriorityOverride > 0) targetPriority = interactPriorityOverride;
        else if (prioritiesStored) targetPriority = initialMainPriority + priorityBoost;
        else
        {
            int mainP = (vcamMain != null) ? vcamMain.Priority : 10;
            targetPriority = mainP + priorityBoost;
        }

        vcamInteract.Priority = targetPriority;
        inInteractMode = true;
        isHidden = false;

        // espera o blend e toca animação
        StartCoroutine(WaitForBlendThenHide());
    }

    void ExitInteractModeImmediateRestore()
    {
        // restaura posição e rotação do player
        if (playerTransform != null)
        {
            playerTransform.position = playerPosOriginal;
            playerTransform.rotation = playerRotOriginal;
        }

        // restaura prioridades
        if (vcamInteract != null) vcamInteract.Priority = initialInteractPriority;

        // reativa controles
        if (playerControllerToDisable != null) playerControllerToDisable.enabled = true;

        inInteractMode = false;
        isHidden = false;

        if (playerNearby && promptUI != null) promptUI.SetActive(true);
    }

    IEnumerator ForceRestore()
    {
        if (isHidden)
            yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));

        ExitInteractModeImmediateRestore();
    }

    IEnumerator PlayExitAndRestore()
    {
        if (!inInteractMode || !isHidden) yield break;

        yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));

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
            yield return StartCoroutine(PlayAnimationAndWait(hideTrigger, hideStateName));
            yield break;
        }

        yield return null;

        while (brain.IsBlending || brain.ActiveVirtualCamera != (ICinemachineCamera)vcamInteract)
        {
            yield return null;
        }

        yield return StartCoroutine(PlayAnimationAndWait(hideTrigger, hideStateName));
        isHidden = true;

        if (interactDuration > 0f)
        {
            if (autoReturnCoroutine != null) StopCoroutine(autoReturnCoroutine);
            autoReturnCoroutine = StartCoroutine(AutoReturnAfterSeconds(interactDuration));
        }
    }

    IEnumerator AutoReturnAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);

        if (isHidden)
            yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));

        ExitInteractModeImmediateRestore();
    }

    IEnumerator PlayAnimationAndWait(string triggerName, string stateName)
    {
        if (doorAnimator == null)
            yield break;

        if (!string.IsNullOrEmpty(triggerName))
            doorAnimator.SetTrigger(triggerName);
        else if (!string.IsNullOrEmpty(stateName))
            doorAnimator.Play(stateName, 0, 0f);

        bool stateReached = false;
        for (int i = 0; i < 60; i++)
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
            yield return new WaitForSeconds(0.2f);

        while (true)
        {
            var info = doorAnimator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(stateName)) yield return null;
            else if (info.normalizedTime >= 1f) break;
            else yield return null;
        }
    }

    // utilidade
    public void DisablePlayerCapsuleCollider()
    {
        CapsuleCollider cachedPlayerCapsule = playerTransform?.GetComponent<CapsuleCollider>();
        if (cachedPlayerCapsule != null) cachedPlayerCapsule.enabled = false;
    }

    public void ForceEnterInteract() => EnterInteractMode();
    public void ForceExitInteract() => StartCoroutine(ForceRestore());
}
