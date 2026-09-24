using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    [System.Serializable]
    public class ChunkData
    {
        public string chunkID; // Ex: "A2", "A3"
        public GameObject chunkObject; // GameObject pai da sala
        public Transform spawnPoint; // Ponto de entrada da sala
    }

    [Header("Configuração das Chunks")]
    public List<ChunkData> chunks = new List<ChunkData>();
    public string startingChunkID = "A2";

    private ChunkData currentActiveChunk;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        InitializeMap();
    }

    // Liga apenas a chunk inicial e desliga todas as outras
    private void InitializeMap()
    {
        foreach (var chunk in chunks)
        {
            if (chunk.chunkID == startingChunkID)
            {
                chunk.chunkObject.SetActive(true);
                currentActiveChunk = chunk;

                if (playerTransform != null && chunk.spawnPoint != null)
                {
                    playerTransform.position = chunk.spawnPoint.position;
                }
            }
            else
            {
                chunk.chunkObject.SetActive(false);
            }
        }
    }

    // Função chamada pelas portas/portais para trocar a sala visível
    public void TransitionToChunk(string targetChunkID)
    {
        ChunkData targetChunk = chunks.Find(c => c.chunkID == targetChunkID);

        if (targetChunk == null)
        {
            Debug.LogError($"Chunk {targetChunkID} não foi encontrada no ChunkManager!");
            return;
        }

        // Ativa a nova sala
        targetChunk.chunkObject.SetActive(true);

        // Move o jogador para o SpawnPoint da nova sala
        if (playerTransform != null && targetChunk.spawnPoint != null)
        {
            playerTransform.position = targetChunk.spawnPoint.position;
        }

        // Desativa a sala anterior para otimizar desempenho
        if (currentActiveChunk != null && currentActiveChunk != targetChunk)
        {
            currentActiveChunk.chunkObject.SetActive(false);
        }

        currentActiveChunk = targetChunk;
        Debug.Log($"Transição concluída para: {targetChunkID}");
    }
}