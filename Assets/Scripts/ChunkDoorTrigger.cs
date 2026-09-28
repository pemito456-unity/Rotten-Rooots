using UnityEngine;

public class ChunkDoorTrigger : MonoBehaviour
{
    public string targetChunkID;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerInside) return;

        // Só trava a porta se a transição foi ACEITA
        if (ChunkManager.Instance != null &&
            ChunkManager.Instance.TransitionToChunk(targetChunkID))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    // Chunk desativado derruba a colisão sem gerar Exit -> reset manual
    private void OnDisable() => playerInside = false;
}
