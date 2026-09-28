using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    [System.Serializable]
    public class ChunkData
    {
        public string chunkID;         // Ex: "A2", "A3"
        public GameObject chunkObject; // GameObject pai da sala
        public Transform spawnPoint;   // Ponto de entrada da sala
    }

    [Header("Configuração das Chunks")]
    public List<ChunkData> chunks = new List<ChunkData>();
    public string startingChunkID = "A2";

    [Header("Transição")]
    public float transitionLockTime = 0.3f;

    private ChunkData currentActiveChunk;
    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        FindPlayer();
        InitializeMap();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody2D>();
        }
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
                TeleportPlayer(chunk.spawnPoint);
            }
            else
            {
                chunk.chunkObject.SetActive(false);
            }
        }
    }

    // Retorna FALSE se recusada (em transição ou chunk inexistente)
    public bool TransitionToChunk(string targetChunkID)
    {
        if (isTransitioning) return false;

        ChunkData targetChunk = chunks.Find(c => c.chunkID == targetChunkID);
        if (targetChunk == null || targetChunk.chunkObject == null) return false;

        StartCoroutine(PerformTransition(targetChunk));
        return true;
    }

    private IEnumerator PerformTransition(ChunkData targetChunk)
    {
        isTransitioning = true;

        targetChunk.chunkObject.SetActive(true);

        if (playerRb == null) FindPlayer(); // garante referência
        TeleportPlayer(targetChunk.spawnPoint);

        if (currentActiveChunk != null && currentActiveChunk != targetChunk)
            currentActiveChunk.chunkObject.SetActive(false);

        currentActiveChunk = targetChunk;

        yield return new WaitForSeconds(transitionLockTime);
        isTransitioning = false;
    }

    // Teleporte correto p/ Rigidbody2D: rb.position (não transform.position)
    private void TeleportPlayer(Transform spawn)
    {
        if (playerRb == null || spawn == null)
        {
            Debug.LogWarning($"ChunkManager: teleporte falhou — playerRb: {playerRb != null}, spawn: {spawn != null}");
            return;
        }

        playerRb.linearVelocity = Vector2.zero; // não carrega momentum da sala anterior
        playerRb.position = spawn.position;
        playerTransform.position = spawn.position; // sincroniza p/ câmera/scripts no mesmo frame
    }
}
