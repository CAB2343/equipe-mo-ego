using System.Collections;
using UnityEngine;

public class Poco : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Quanto a ferradura sobe em relação à posição atual")]
    public float alturaSubida = 2f;

    private bool permiteSubir = true;

    [Tooltip("Tempo que leva para a ferradura subir")]
    public float duracao = 1f;

    [Header("Referências")]
    public UiManager uiManager;
    public GameObject ferradura;

    private bool emTransicao = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiManager != null && uiManager.corda && !emTransicao)
            {
                Debug.Log("SOBE");
                MoverAteAltura();
            }
        }
    }

    public void MoverAteAltura()
    {
        if (ferradura != null && permiteSubir)
            StartCoroutine(MoverFerradura());
        else
            Debug.LogWarning("Ferradura não atribuída no Inspector!");
    }

    private IEnumerator MoverFerradura()
    {
        emTransicao = true;

        Vector3 inicio = ferradura.transform.localPosition;
        Vector3 destino = inicio + new Vector3(0f, alturaSubida, 0f);

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);
            ferradura.transform.localPosition = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        ferradura.transform.localPosition = destino;
        emTransicao = false;
        permiteSubir = false;
    }
}