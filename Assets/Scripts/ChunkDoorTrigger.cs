using UnityEngine;

public class ChunkDoorTrigger : MonoBehaviour
{
    [Header("Configuração de Destino")]
    public string targetChunkID;

    [Header("Mecânica (Ira da Floresta / Trancas)")]
    public bool isLocked = false;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerInside) return;

        // Se a passagem estiver bloqueada pela Ira da Floresta, não faz nada
        if (isLocked)
        {
            Debug.Log($"A passagem para {targetChunkID} está bloqueada!");
            return;
        }

        // Tenta realizar a transição no ChunkManager
        if (ChunkManager.Instance != null && ChunkManager.Instance.TransitionToChunk(targetChunkID))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    private void OnDisable() => playerInside = false;

    // Método público para ser chamado pelo script da Ira da Floresta
    public void SetDoorLock(bool locked)
    {
        isLocked = locked;
    }
}