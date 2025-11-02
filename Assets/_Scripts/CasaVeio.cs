using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasaVeio : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao Transform do jogador.")]
    public Transform player;
    public MulaController mula;
    public GameObject canvas;

    [Header("Configurações de Áudio")]
    [Tooltip("Distância máxima em que o som ainda é audível.")]
    public float maxDistance = 50f;

    [Tooltip("Volume máximo do som quando o player está muito próximo.")]
    [Range(0f, 1f)] 
    public float maxVolume = 1f;

    public AudioSource audioSource;

    void Start()
    {
    }

    void Update()
    {
        if (player == null) return;

        // Calcula a distância entre o player e o som
        float distancia = Vector3.Distance(player.position, transform.position);

        // Calcula o volume proporcional à distância (quanto mais perto, mais alto)
        float novoVolume = Mathf.Clamp01(1 - (distancia / maxDistance)) * maxVolume;

        if (mula.veio && !canvas.activeSelf)
        {
            audioSource.volume = novoVolume;
        }
        // Atualiza o volume em tempo real
        
    }
}
