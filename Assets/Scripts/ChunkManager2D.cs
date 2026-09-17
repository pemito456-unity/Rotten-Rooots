using System.Collections.Generic;
using UnityEngine;

public class ChunkManager2D : MonoBehaviour
{
    [Header("Configurações do Jogador e Chunks")]
    public Transform player;
    public GameObject[] chunkPrefabs; // Arraste seu Chunk_Prototype para cá no Inspector!
    public int chunkSize = 20;        // Tamanho exato em unidades do seu Sprite/Chunk
    public int viewDistance = 2;      // Raio de chunks visíveis ao redor do player

    private Vector2Int currentChunkCoord;
    private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        if (player == null) return;

        // Calcula em qual coordenada de chunk o player está
        Vector2Int newChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.y / chunkSize)
        );

        // Se o player mudou de chunk, atualiza os chunks visíveis
        if (newChunkCoord != currentChunkCoord || loadedChunks.Count == 0)
        {
            currentChunkCoord = newChunkCoord;
            UpdateChunks();
        }
    }

    void UpdateChunks()
    {
        List<Vector2Int> toRemove = new List<Vector2Int>(loadedChunks.Keys);

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int coord = new Vector2Int(currentChunkCoord.x + x, currentChunkCoord.y + y);
                
                if (!loadedChunks.ContainsKey(coord))
                {
                    // Instancia um chunk provisório na posição exata do grid
                    int randomIndex = Random.Range(0, chunkPrefabs.Length);
                    Vector3 spawnPos = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);
                    GameObject newChunk = Instantiate(chunkPrefabs[randomIndex], spawnPos, Quaternion.identity);
                    loadedChunks.Add(coord, newChunk);
                }
                else
                {
                    toRemove.Remove(coord);
                }
            }
        }

        // Remove chunks antigos que ficaram longe
        foreach (var coord in toRemove)
        {
            Destroy(loadedChunks[coord]);
            loadedChunks.Remove(coord);
        }
    }
}