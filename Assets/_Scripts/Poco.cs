using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poco : MonoBehaviour
{
    [Header("Configuração")]
    public float posicaoAlvoY = 0.7088f;   // posição final no eixo Y
    public float duracao = 1f;        // tempo da transição

    private bool emTransicao = false;
    public UiManager uiManager;
    public GameObject ferradura;
    
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiManager.corda)
            {
                MoverAtePosicaoY();
            }
            
        }
    }
    
    public void MoverAtePosicaoY()
    {
        if (!emTransicao)
            StartCoroutine(MoverFerradura(posicaoAlvoY));
    }

    private IEnumerator MoverFerradura(float destinoY)
    {
        emTransicao = true;

        Vector3 inicio = transform.localPosition;
        Vector3 destino = new Vector3(inicio.x, destinoY, inicio.z);

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);
            transform.localPosition = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        transform.localPosition = destino;
        emTransicao = false;
    }
}
