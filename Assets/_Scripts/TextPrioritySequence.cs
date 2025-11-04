using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TMPEntry
{
    public TMP_Text text;
    public int priority = 0;
    [TextArea]
    public string finalText;
}

public class TextPrioritySequence : MonoBehaviour
{
    [Tooltip("Entradas com prioridade. Prioridade menor aparece antes.")]
    public List<TMPEntry> entries = new List<TMPEntry>();

    [Tooltip("Tempo (segundos) entre o término de um texto e o início do próximo")]
    public float intervalo = 0.5f;

    [Tooltip("Se true, prioridades menores aparecem primeiro. Se false, maiores aparecem primeiro.")]
    public bool lowerPriorityFirst = true;

    [Header("Comportamento de reverso (após todos aparecerem)")]
    public bool reverseAfterAll = true;
    public float reverseDelayBeforeStart = 1f;
    public float reverseInterval = 0.5f;
    public bool useFadeOnReverse = true;
    public float fadeDuration = 0.5f;

    [Header("RectTransform animation (antes/após)")]
    [Tooltip("RectTransform alvo que será animado antes da sequência e após o reverse.")]
    public RectTransform reactTransformTarget;

    [Tooltip("Tempo para animar antes de aparecer (para levar top/bottom até 1)")]
    public float preAnimationDuration = 0.4f;

    [Tooltip("Tempo para animar depois (top -> 105, depois bottom -> 136)")]
    public float postAnimationPerStepDuration = 0.35f;

    [Tooltip("Delay entre animar top e animar bottom na fase pós-sequência")]
    public float postAnimationStepDelay = 0.05f;

    // armazenamento das cores originais para restaurar ao re-ativar
    private Dictionary<TMPEntry, Color> originalColors = new Dictionary<TMPEntry, Color>();

    void Awake()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e == null) continue;
            if (string.IsNullOrEmpty(e.finalText))
            {
                e.finalText = (e.text != null) ? e.text.text : "";
            }

            if (e.text != null)
            {
                originalColors[e] = e.text.color;
            }
        }
    }

    void Start()
    {
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // Antes de qualquer coisa: anima o RectTransform alvo para top=1 bottom=1 (se tiver alvo)
        if (reactTransformTarget != null && preAnimationDuration > 0f)
        {
            // Anima de valores atuais -> top=1, bottom=1
            yield return StartCoroutine(AnimateRectTransformTo(reactTransformTarget, targetTop: 1f, targetBottom: 1f, duration: preAnimationDuration));
        }
        else if (reactTransformTarget != null)
        {
            // aplica diretamente (sem animação)
            SetRectTopBottom(reactTransformTarget, 1f, 1f);
        }

        // desativa todos no começo
        foreach (var e in entries)
            if (e != null && e.text != null)
                e.text.gameObject.SetActive(false);

        var indexed = entries.Select((e, idx) => new { entry = e, idx });
        IEnumerable<TMPEntry> ordered = lowerPriorityFirst
            ? indexed.OrderBy(x => x.entry.priority).ThenBy(x => x.idx).Select(x => x.entry)
            : indexed.OrderByDescending(x => x.entry.priority).ThenBy(x => x.idx).Select(x => x.entry);

        // ---- sequência normal (aparecer) ----
        foreach (var e in ordered)
        {
            if (e == null || e.text == null) continue;

            if (originalColors.ContainsKey(e))
                e.text.color = originalColors[e];

            e.text.gameObject.SetActive(true);

            string target = e.finalText ?? "";

            if (!string.IsNullOrEmpty(target))
            {
                while (e.text.text != target)
                {
                    yield return null;
                }
            }

            if (intervalo > 0f)
                yield return new WaitForSeconds(intervalo);
            else
                yield return null;
        }

        // ---- após terminar toda sequência, opcionalmente iniciar reverse ----
        if (reverseAfterAll)
        {
            if (reverseDelayBeforeStart > 0f)
                yield return new WaitForSeconds(reverseDelayBeforeStart);
            else
                yield return null;

            var reversed = ordered.Reverse();

            foreach (var e in reversed)
            {
                if (e == null || e.text == null) continue;

                if (useFadeOnReverse && fadeDuration > 0f)
                {
                    yield return StartCoroutine(FadeOutAndDisable(e.text, fadeDuration));
                }
                else
                {
                    e.text.gameObject.SetActive(false);
                }

                if (reverseInterval > 0f)
                    yield return new WaitForSeconds(reverseInterval);
                else
                    yield return null;
            }

            // Depois que TODOS os textos sumirem: anima o RectTransform na ordem que você pediu
            if (reactTransformTarget != null)
            {
                // 1) anima top para 105
                yield return StartCoroutine(AnimateRectTransformTop(reactTransformTarget, targetTop: 105f, duration: postAnimationPerStepDuration));

                // opcional pequeno delay entre top e bottom
                if (postAnimationStepDelay > 0f)
                    yield return new WaitForSeconds(postAnimationStepDelay);

                // 2) anima bottom para 136
                yield return StartCoroutine(AnimateRectTransformBottom(reactTransformTarget, targetBottom: 136f, duration: postAnimationPerStepDuration));
            }
        }
    }

    // Método público para (re)iniciar a sequência manualmente
    public void StartSequenceManually()
    {
        StopAllCoroutines();
        StartCoroutine(RunSequence());
    }

    // Fade out coroutine
    IEnumerator FadeOutAndDisable(TMP_Text t, float duration)
    {
        if (t == null)
            yield break;

        t.gameObject.SetActive(true);

        Color startColor = t.color;
        float startAlpha = startColor.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float a = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            t.color = new Color(startColor.r, startColor.g, startColor.b, a);
            elapsed += Time.deltaTime;
            yield return null;
        }

        t.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        t.gameObject.SetActive(false);
    }

    // Helpers para RectTransform
    // Observação: no inspector do RectTransform, "Top" corresponde a -offsetMax.y internamente.
    void SetRectTopBottom(RectTransform rt, float top, float bottom)
    {
        if (rt == null) return;
        Vector2 om = rt.offsetMax;
        Vector2 im = rt.offsetMin;
        rt.offsetMax = new Vector2(om.x, -top);
        rt.offsetMin = new Vector2(im.x, bottom);
    }

    IEnumerator AnimateRectTransformTo(RectTransform rt, float targetTop, float targetBottom, float duration)
    {
        if (rt == null || duration <= 0f)
        {
            SetRectTopBottom(rt, targetTop, targetBottom);
            yield break;
        }

        Vector2 startOffsetMax = rt.offsetMax;
        Vector2 startOffsetMin = rt.offsetMin;

        float startTop = -startOffsetMax.y;   // valor do "Top" mostrado no inspector
        float startBottom = startOffsetMin.y; // valor do "Bottom" mostrado no inspector

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curTop = Mathf.Lerp(startTop, targetTop, t);
            float curBottom = Mathf.Lerp(startBottom, targetBottom, t);
            rt.offsetMax = new Vector2(startOffsetMax.x, -curTop);
            rt.offsetMin = new Vector2(startOffsetMin.x, curBottom);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // garantir valores finais exatos
        rt.offsetMax = new Vector2(startOffsetMax.x, -targetTop);
        rt.offsetMin = new Vector2(startOffsetMin.x, targetBottom);
    }

    IEnumerator AnimateRectTransformTop(RectTransform rt, float targetTop, float duration)
    {
        if (rt == null || duration <= 0f)
        {
            // aplica direto
            Vector2 om = rt.offsetMax;
            rt.offsetMax = new Vector2(om.x, -targetTop);
            yield break;
        }

        Vector2 startOffsetMax = rt.offsetMax;
        float startTop = -startOffsetMax.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curTop = Mathf.Lerp(startTop, targetTop, t);
            rt.offsetMax = new Vector2(startOffsetMax.x, -curTop);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.offsetMax = new Vector2(startOffsetMax.x, -targetTop);
    }

    IEnumerator AnimateRectTransformBottom(RectTransform rt, float targetBottom, float duration)
    {
        if (rt == null || duration <= 0f)
        {
            Vector2 im = rt.offsetMin;
            rt.offsetMin = new Vector2(im.x, targetBottom);
            yield break;
        }

        Vector2 startOffsetMin = rt.offsetMin;
        float startBottom = startOffsetMin.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curBottom = Mathf.Lerp(startBottom, targetBottom, t);
            rt.offsetMin = new Vector2(startOffsetMin.x, curBottom);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.offsetMin = new Vector2(startOffsetMin.x, targetBottom);
    }
}
