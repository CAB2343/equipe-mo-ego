using UnityEngine;
using System.Collections;
using BrewedInk.CRT; // importante para acessar CRTCameraBehaviour

public class CRTCurveTransition : MonoBehaviour
{
    public CRTCameraBehaviour crtCam;

    /// <summary>
    /// Faz uma transição suave no monitorCurve.
    /// </summary>
    public void StartCurveTransition(float novoValor, float duracao)
    {
        //StopAllCoroutines(); // cancela transições anteriores
        StartCoroutine(CurveTransitionRoutine(novoValor, duracao));
    }

    private IEnumerator CurveTransitionRoutine(float alvo, float duracao)
    {
        if (crtCam == null || crtCam.data == null)
            yield break;

        float inicial = crtCam.data.monitorCurve;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            // interpolação suave
            crtCam.data.monitorCurve = Mathf.Lerp(inicial, alvo, t);

            // opcional: aplica no material imediatamente
            if (crtCam._runtimeMaterial != null)
                crtCam._runtimeMaterial.SetFloat(Shader.PropertyToID("_Curvature2"), crtCam.data.monitorCurve);

            yield return null;
        }

        // garante o valor final
        crtCam.data.monitorCurve = alvo;
    }
}
