using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider))]
public class InteractHideEnterExit : MonoBehaviour
{
    [Header("Player detection")]
    public string playerTag = "Player";
    public Transform playerTransform;
    public bool returnOnExit = true;

    [Header("Cinemachine (VCams)")]
    public CinemachineVirtualCamera vcamMain;
    public CinemachineVirtualCamera vcamInteract;

    [Header("Priority behavior")]
    public int interactPriorityOverride = -1;
    public int priorityBoost = 10;

    [Header("Behavior")]
    public MonoBehaviour playerControllerToDisable; 
    public GameObject promptUI;                    
    public float interactDuration = 0f;            

    [Header("Animator / States")]
    public Animator doorAnimator;              
    public string hideTrigger = "Esconder";    
    public string hideStateName = "escondendo";
    public string exitTrigger = "Sair";        
    public string exitStateName = "saindo";

    [Header("Esconderijo Transform")]
    public Transform esconderijoCamera; // referência do ponto do esconderijo

    private bool playerNearby = false;
    private bool inInteractMode = false;
    private bool isHidden = false;
    private Coroutine autoReturnCoroutine = null;

    private int initialMainPriority = 0;
    private int initialInteractPriority = 0;
    private bool prioritiesStored = false;

    // posição original do player
    private Vector3 originalPosition;
    private Quaternion originalRotation;

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
            else if (isHidden)
            {
                StartCoroutine(PlayExitAndRestore());
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
        if (vcamInteract == null || esconderijoCamera == null)
        {
            Debug.LogWarning("[Interact] vcamInteract ou esconderijoCamera não atribuídos.");
            return;
        }

        // salva posição original antes de teleporte
        originalPosition = playerTransform.position;
        originalRotation = playerTransform.rotation;

        // teleporta player para o esconderijo antes da animação
        MovePlayerToEsconderijoIgnoreCamera();

        if (playerControllerToDisable != null) playerControllerToDisable.enabled = false;
        if (promptUI != null) promptUI.SetActive(false);

        int targetPriority = interactPriorityOverride > 0 
            ? interactPriorityOverride 
            : (prioritiesStored ? initialMainPriority + priorityBoost : 10 + priorityBoost);

        vcamInteract.Priority = targetPriority;
        inInteractMode = true;
        isHidden = false;

        StartCoroutine(WaitForBlendThenHide());
    }

    void ExitInteractModeImmediateRestore()
    {
        if (vcamInteract != null) vcamInteract.Priority = initialInteractPriority;

        // restaura player na posição original
        RestorePlayerOriginalPosition();

        if (playerControllerToDisable != null) playerControllerToDisable.enabled = true;

        inInteractMode = false;
        isHidden = false;

        if (playerNearby && promptUI != null) promptUI.SetActive(true);
    }

    IEnumerator ForceRestore()
    {
        if (isHidden) yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));
        ExitInteractModeImmediateRestore();
    }

    IEnumerator PlayExitAndRestore()
    {
        if (!inInteractMode || !isHidden) yield break;

        yield return StartCoroutine(PlayAnimationAndWait(exitTrigger, exitStateName));
        ExitInteractModeImmediateRestore();
    }

    IEnumerator WaitForBlendThenHide()
    {
        CinemachineBrain brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : FindObjectOfType<CinemachineBrain>();
        if (brain == null)
        {
            yield return StartCoroutine(PlayAnimationAndWait(hideTrigger, hideStateName));
            isHidden = true;
            yield break;
        }

        yield return null;

        while (brain.IsBlending || brain.ActiveVirtualCamera != (ICinemachineCamera)vcamInteract)
            yield return null;

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
        if (doorAnimator == null) yield break;

        if (!string.IsNullOrEmpty(triggerName)) doorAnimator.SetTrigger(triggerName);
        else if (!string.IsNullOrEmpty(stateName)) doorAnimator.Play(stateName, 0, 0f);
        else yield break;

        bool stateReached = false;
        for (int i = 0; i < 60; i++)
        {
            var info = doorAnimator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName)) { stateReached = true; break; }
            yield return null;
        }

        if (!stateReached) yield return new WaitForSeconds(0.2f);

        while (true)
        {
            var info = doorAnimator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(stateName)) yield return null;
            else if (info.normalizedTime >= 1f) break;
            else yield return null;
        }
    }

    // ---------- Player movement ----------
    private void MovePlayerToEsconderijoIgnoreCamera()
    {
        if (playerTransform == null || esconderijoCamera == null) return;

        playerTransform.position = esconderijoCamera.position;
        playerTransform.rotation = esconderijoCamera.rotation;

        foreach (Transform child in playerTransform)
        {
            if (child.CompareTag("MainCamera")) continue;
            child.position = esconderijoCamera.position;
            child.rotation = esconderijoCamera.rotation;
        }
    }

    private void RestorePlayerOriginalPosition()
    {
        if (playerTransform == null) return;

        playerTransform.position = originalPosition;
        playerTransform.rotation = originalRotation;

        foreach (Transform child in playerTransform)
        {
            if (child.CompareTag("MainCamera")) continue;
            child.position = originalPosition;
            child.rotation = originalRotation;
        }
    }
}
