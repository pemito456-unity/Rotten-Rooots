using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;

    [System.Serializable]
    public class ChunkData
    {
        public string chunkID;       // Exemplo: "A2"
        public GameObject chunkObject; // Objeto raiz da sala
        public Transform[] spawnPoints; // Pontos de entrada da sala
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
    private readonly HashSet<string> visitedChunkIDs = new HashSet<string>();

    public string CurrentChunkID
    {
        get
        {
            if (currentActiveChunk == null)
                return null;

            return currentActiveChunk.chunkID;
        }
    }

    public bool HasVisitedChunk(string chunkID)
    {
        if (string.IsNullOrEmpty(chunkID))
            return false;

        return visitedChunkIDs.Contains(chunkID);
    }

    public bool IsChunkRegistered(string chunkID)
    {
        if (string.IsNullOrEmpty(chunkID))
            return false;

        foreach (ChunkData chunk in chunks)
        {
            if (chunk != null && chunk.chunkObject != null && chunk.chunkID == chunkID)
                return true;
        }

        return false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
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
        visitedChunkIDs.Clear();

        foreach (ChunkData chunk in chunks)
        {
            if (chunk == null || chunk.chunkObject == null)
                continue;

            if (chunk.chunkID == startingChunkID)
            {
                chunk.chunkObject.SetActive(true);
                currentActiveChunk = chunk;
                visitedChunkIDs.Add(chunk.chunkID);
                TeleportPlayer(chunk.spawnPoints[0]);
            }
            else
            {
                chunk.chunkObject.SetActive(false);
            }
        }
    }

    // Retorna FALSE se recusada (em transição ou chunk inexistente)
    public bool TransitionToChunk(string targetChunkID, int spawnPointIndex)
    {
        if (isTransitioning) return false;

        ChunkData targetChunk = null;
        foreach (ChunkData chunk in chunks)
        {
            if (chunk.chunkID == targetChunkID)
            {
                targetChunk = chunk;
                break;
            }
        }

        if (targetChunk == null || targetChunk.chunkObject == null) return false;

        StartCoroutine(PerformTransition(targetChunk, spawnPointIndex));
        return true;
    }

    private IEnumerator PerformTransition(ChunkData targetChunk, int spawnPointIndex)
    {
        isTransitioning = true;

        targetChunk.chunkObject.SetActive(true);

        if (playerRb == null) FindPlayer(); // garante referência
        TeleportPlayer(targetChunk.spawnPoints[spawnPointIndex]);

        if (currentActiveChunk != null && currentActiveChunk != targetChunk)
            currentActiveChunk.chunkObject.SetActive(false);

        currentActiveChunk = targetChunk;
        visitedChunkIDs.Add(targetChunk.chunkID);

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
