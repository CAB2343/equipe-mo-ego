using UnityEngine;
using System.Collections;
using BrewedInk.CRT;

public class CRTCurveTransition : MonoBehaviour
{
    public CRTCameraBehaviour crtCam;

    /// <summary>
    /// Faz uma transição suave no monitorCurve.
    /// </summary>
    public void StartMonitorCurveTransition(float novoValor, float duracao)
    {
        StartCoroutine(FloatTransitionRoutine(
            valorGetter: () => crtCam.data.monitorCurve,
            valorSetter: v => {
                crtCam.data.monitorCurve = v;
                if (crtCam._runtimeMaterial != null)
                    crtCam._runtimeMaterial.SetFloat(Shader.PropertyToID("_Curvature2"), v);
            },
            alvo: novoValor,
            duracao: duracao
        ));
    }

    /// <summary>
    /// Faz uma transição suave no PropDitheringAmount8 (dithering8).
    /// </summary>
    public void StartDitheringTransition(float novoValor, float duracao)
    {
        StartCoroutine(FloatTransitionRoutine(
            valorGetter: () => crtCam.data.dithering8,
            valorSetter: v => {
                crtCam.data.dithering8 = v;
                if (crtCam._runtimeMaterial != null)
                    crtCam._runtimeMaterial.SetFloat(Shader.PropertyToID("_Spread8"), v);
            },
            alvo: novoValor,
            duracao: duracao
        ));
    }

    /// <summary>
    /// Coroutine genérica de transição de float.
    /// </summary>
    private IEnumerator FloatTransitionRoutine(System.Func<float> valorGetter, System.Action<float> valorSetter, float alvo, float duracao)
    {
        if (crtCam == null || crtCam.data == null)
            yield break;

        float inicial = valorGetter();
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);
            valorSetter(Mathf.Lerp(inicial, alvo, t));
            yield return null;
        }

        // garante o valor final
        valorSetter(alvo);
    }
}
