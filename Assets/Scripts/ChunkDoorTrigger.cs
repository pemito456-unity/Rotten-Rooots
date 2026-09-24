using UnityEngine;

public class ChunkDoorTrigger : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("Digite a ID exata da chunk destino (Ex: A3, B4)")]
    public string targetChunkID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ChunkManager.Instance != null)
            {
                ChunkManager.Instance.TransitionToChunk(targetChunkID);
            }
        }
    }
}