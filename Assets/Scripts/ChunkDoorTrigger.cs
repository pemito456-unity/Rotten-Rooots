using UnityEngine;

public class ChunkDoorTrigger : MonoBehaviour
{
    [Header("Configuração de Destino")]
    public string targetChunkID;

    [Header("Qual Spawn Point do Chunk é o Alvo")]
    public int spawnPointIndex = 0;

    [Header("Mecânica (Ira da Floresta / Trancas)")]
    public bool isLocked = false;

    [Header("Bloqueio por ira da floresta")]
    public bool useWrathBlocker;
    [Range(0f, 100f)] public float wrathThresholdPercent = 50f;
    [Tooltip("Use o mesmo ID nos dois lados da passagem, por exemplo C3-C4.")]
    public string passageBlockerId;
    public GameObject hostileTreePrefab;
    [Tooltip("Ative apenas na árvore especial de 60%, que só pode ser removida com kit.")]
    public bool blockerOnlyRemovableByKit;

    private bool playerInside = false;
    private bool waitingAtBlockedPassage;
    private bool blockedNoticeShown;
    private ArvoreSinistra spawnedBlocker;

    private bool IsPassageBlocked
    {
        get
        {
            if (isLocked)
                return true;

            return useWrathBlocker &&
                   ForestWrathManager.IsPassageBlocked(passageBlockerId);
        }
    }

    private void OnEnable()
    {
        ForestWrathManager.WrathChanged += HandleWrathChanged;
        ForestWrathManager.PassageCleared += HandlePassageCleared;
        RefreshWrathBlocker();
    }

    private void Start()
    {
        // Re-check after all scene singletons have completed Awake.
        RefreshWrathBlocker();
    }

    private void OnDisable()
    {
        ForestWrathManager.WrathChanged -= HandleWrathChanged;
        ForestWrathManager.PassageCleared -= HandlePassageCleared;
        playerInside = false;
        waitingAtBlockedPassage = false;
        blockedNoticeShown = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerInside) return;

        if (IsPassageBlocked)
        {
            waitingAtBlockedPassage = true;
            LogBlockedNotice();
            return;
        }

        TryTransition(spawnPointIndex);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Permite atravessar logo que a árvore seja removida, sem exigir que o jogador
        // saia e entre novamente no trigger.
        if (other.CompareTag("Player") && waitingAtBlockedPassage &&
            !playerInside && !IsPassageBlocked)
            TryTransition(spawnPointIndex);
    }

    private void TryTransition(int spawnPointIndex)
    {
        if (ChunkManager.Instance != null && ChunkManager.Instance.TransitionToChunk(targetChunkID, spawnPointIndex))
        {
            playerInside = true;
            waitingAtBlockedPassage = false;
        }
    }

    private void LogBlockedNotice()
    {
        if (blockedNoticeShown)
            return;

        Debug.Log($"A passagem para {targetChunkID} está bloqueada por uma árvore sinistra!");
        blockedNoticeShown = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            waitingAtBlockedPassage = false;
            blockedNoticeShown = false;
        }
    }

    // Método público para ser chamado pelo script da Ira da Floresta
    public void SetDoorLock(bool locked)
    {
        isLocked = locked;
    }

    private void HandleWrathChanged(float wrathPercent)
    {
        RefreshWrathBlocker();
    }

    private void HandlePassageCleared(string clearedPassageId)
    {
        if (string.IsNullOrEmpty(passageBlockerId) || passageBlockerId != clearedPassageId)
            return;

        if (spawnedBlocker != null)
            Destroy(spawnedBlocker.gameObject);
        spawnedBlocker = null;
    }

    private void RefreshWrathBlocker()
    {
        if (!useWrathBlocker || ForestWrathManager.Instance == null ||
            ForestWrathManager.IsPassageCleared(passageBlockerId))
            return;

        float maxWrath = ForestWrathManager.Instance.maxWrath;
        float wrathPercent = maxWrath > 0f
            ? ForestWrathManager.Instance.currentWrath / maxWrath * 100f
            : 0f;
        if (wrathPercent >= wrathThresholdPercent)
            ForestWrathManager.MarkPassageBlocked(passageBlockerId);

        if (!ForestWrathManager.IsPassageBlocked(passageBlockerId))
            return;

        if (spawnedBlocker != null || ForestWrathManager.IsPassageCleared(passageBlockerId))
            return;

        if (hostileTreePrefab == null)
        {
            Debug.LogWarning($"A passagem {passageBlockerId} atingiu o limite de ira, mas não tem prefab de árvore configurado.", this);
            return;
        }

        GameObject treeObject = Instantiate(
            hostileTreePrefab,
            transform.position,
            Quaternion.identity,
            transform.parent
        );

        spawnedBlocker = treeObject.GetComponent<ArvoreSinistra>();
        if (spawnedBlocker == null)
        {
            Debug.LogError("O prefab de bloqueio precisa ter o componente ArvoreSinistra.", treeObject);
            Destroy(treeObject);
            return;
        }

        spawnedBlocker.ConfigureAsPassageBlocker(
            passageBlockerId,
            blockerOnlyRemovableByKit
        );
    }
}
