using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject mainMenuButtons;
    [SerializeField] private GameObject options;
    [SerializeField] private CRTCurveTransition camTransition;
    [SerializeField] private AudioSource sound;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject canvasGame;
    [SerializeField] private GameObject perdeu;
    [SerializeField] private GameObject ganhou;
    [SerializeField] private SoundsManager soundsManager;

    // NOVO: referência ao GameObject do tutorial (arraste no inspector)
    [SerializeField] private GameObject tutorial;

    // Tempo máximo de espera (em segundos) pelo fim da animação do tutorial antes de prosseguir
    [SerializeField] private float tutorialTimeout = 10f;

    void Start()
    {
    }

    void Update()
    {

    }

    public void Jogar()
    {
        sound.Stop();

        if (tutorial != null)
        {
            // ativa o tutorial e aguarda sua conclusão antes de iniciar as transições
            tutorial.SetActive(true);
            StartCoroutine(WaitForTutorialThenStart());
        }
        else
        {
            // fallback: comportamento original se não houver tutorial configurado
            camTransition.StartMonitorCurveTransition(0f, 2f);
            camTransition.StartDitheringTransition(0f, 2f);
            StartCoroutine(CurveTransitionRoutine(2f));
        }
    }

    public void OpenOptions()
    {
        mainMenuButtons.SetActive(false);
        options.SetActive(true);
    }

    public void CloseOptions()
    {
        mainMenuButtons.SetActive(true);
        options.SetActive(false);
    }
    
    public void Perdeu()
    {
        canvas.SetActive(true);
        perdeu.SetActive(true);
        canvasGame.SetActive(false);
        camTransition.StartMonitorCurveTransition(0.289f, 2f);
        camTransition.StartDitheringTransition(0.07f, 2f);
        sound.Stop();
        soundsManager.SoundPlay(3);
    }
    
    public void Venceu()
    {
        canvas.SetActive(true);
        ganhou.SetActive(true);
        canvasGame.SetActive(false);
        camTransition.StartMonitorCurveTransition(0.289f, 2f);
        camTransition.StartDitheringTransition(0.07f, 2f);
        sound.Stop();
        soundsManager.SoundPlay(4);
    }

    public void SairJogo()
    {
        Application.Quit();
    }
    
    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private IEnumerator CurveTransitionRoutine(float duracao)
    {
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            yield return null;
        }
        canvas.SetActive(false);
        mainMenu.SetActive(false);
        canvasGame.SetActive(true);
        soundsManager.ChangeTheme(1);
    }

    // -------------------- NOVO: espera o tutorial terminar --------------------
    private IEnumerator WaitForTutorialThenStart()
    {
        float elapsed = 0f;

        // tenta detectar um Animator no tutorial e esperar a animação terminar
        Animator anim = null;
        if (tutorial != null)
            anim = tutorial.GetComponent<Animator>();

        if (anim != null)
        {
            // Se existir Animator: aguarda até o estado atual completar (ou timeout)
            while (elapsed < tutorialTimeout)
            {
                // se o animator estiver em transição, espera
                if (anim.IsInTransition(0))
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                    continue;
                }

                AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

                // Se animation clip tem duração (length > 0) e normalizedTime >= 1 => provavelmente terminou
                // Note: se a animação estiver em loop, normalizedTime continuará aumentando — então o fallback para timeout existe.
                if (info.length > 0f && info.normalizedTime >= 1f)
                {
                    break;
                }

                // caso o Animator use triggers/events para desativar o tutorial ao final,
                // o fallback abaixo (verificar se o tutorial foi desativado) também ajuda.
                if (!tutorial.activeInHierarchy)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // Se não há Animator: espera o tutorial ser desativado (supondo que o próprio tutorial se desative ao terminar)
            while (tutorial != null && tutorial.activeInHierarchy && elapsed < tutorialTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Se deu timeout, a gente ainda prossegue para não travar a entrada do jogo
        // Agora executa as transições originais (mesma lógica que estava no Jogar original)
        camTransition.StartMonitorCurveTransition(0f, 2f);
        camTransition.StartDitheringTransition(0f, 2f);
        StartCoroutine(CurveTransitionRoutine(2f));
    }
}
