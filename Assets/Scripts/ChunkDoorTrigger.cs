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
    [Tooltip("Armas e faca não causam dano; se o kit também não puder remover, ligue a opção permanente abaixo.")]
    public bool blockerOnlyRemovableByKit;
    [Tooltip("Impede que o kit remova a árvore deste bloqueio.")]
    public bool blockerCannotBeRemovedByKit;

    [Header("Saída final")]
    [Tooltip("Este trigger decide o final em vez de trocar de chunk.")]
    public bool isFinalEndingExit;

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

            if (!useWrathBlocker)
                return false;

            return ForestWrathManager.IsPassageBlocked(passageBlockerId);
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
        // Confere de novo depois que os objetos principais da cena inicializam.
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
        if (!other.CompareTag("Player") || playerInside)
            return;

        if (IsPassageBlocked)
        {
            waitingAtBlockedPassage = true;
            LogBlockedNotice();
            if (isFinalEndingExit)
                ResolveFinalExitAttempt();
            return;
        }

        if (isFinalEndingExit)
        {
            ResolveFinalExitAttempt();
            return;
        }

        TryTransition(spawnPointIndex);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isFinalEndingExit)
        {
            if (FinalFireSequence.Instance != null && FinalFireSequence.Instance.IsSequenceActive)
                ResolveFinalExitAttempt();
            return;
        }

        // Permite atravessar logo que a árvore seja removida, sem exigir que o jogador
        // saia e entre novamente no trigger.
        if (waitingAtBlockedPassage && !playerInside && !IsPassageBlocked)
            TryTransition(spawnPointIndex);
    }

    private void ResolveFinalExitAttempt()
    {
        if (FinalFireSequence.Instance == null)
            return;

        FinalFireSequence.Instance.ResolveFinalExitAttempt(IsPassageBlocked);
    }

    private void TryTransition(int spawnPointIndex)
    {
        if (ChunkManager.Instance == null)
            return;

        bool transitionStarted = ChunkManager.Instance.TransitionToChunk(
            targetChunkID,
            spawnPointIndex
        );

        if (transitionStarted)
        {
            playerInside = true;
            waitingAtBlockedPassage = false;
        }
    }

    private void LogBlockedNotice()
    {
        if (blockedNoticeShown)
            return;

        string destination = isFinalEndingExit ? "a saída final" : $"a chunk {targetChunkID}";
        Debug.Log($"A passagem para {destination} está bloqueada por uma árvore sinistra!");
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
        float wrathPercent = 0f;
        if (maxWrath > 0f)
            wrathPercent = ForestWrathManager.Instance.currentWrath / maxWrath * 100f;
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
            blockerOnlyRemovableByKit,
            blockerCannotBeRemovedByKit
        );
    }
}
