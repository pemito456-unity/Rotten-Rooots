using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Atualiza o mapa quando o painel de pausa é aberto.
/// As salas já visitadas permanecem visíveis e o marcador acompanha a sala atual.
/// </summary>
public class PauseMinimapController : MonoBehaviour
{
    [Header("Cores do mapa")]
    [SerializeField] private Color undiscoveredColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color visitedRoomColor = new Color(0.12f, 0.24f, 0.14f, 1f);
    [SerializeField] private Color currentRoomColor = new Color(0.24f, 0.72f, 0.28f, 1f);

    private Transform grid;
    private Image playerMarker;
    private Coroutine refreshCoroutine;

    private void OnEnable()
    {
        if (ChunkManager.Instance != null &&
            !string.IsNullOrEmpty(ChunkManager.Instance.CurrentChunkID))
        {
            RefreshMap();
            return;
        }

        refreshCoroutine = StartCoroutine(RefreshWhenChunkManagerIsReady());
    }

    private IEnumerator RefreshWhenChunkManagerIsReady()
    {
        while (ChunkManager.Instance == null ||
               string.IsNullOrEmpty(ChunkManager.Instance.CurrentChunkID))
        {
            yield return null;
        }

        RefreshMap();
        refreshCoroutine = null;
    }

    private void OnDisable()
    {
        if (refreshCoroutine == null)
            return;

        StopCoroutine(refreshCoroutine);
        refreshCoroutine = null;
    }

    private void RefreshMap()
    {
        if (grid == null)
            grid = transform.Find("Grid");

        if (grid == null)
        {
            Debug.LogError("PauseMinimapController: não encontrei o filho 'Grid'.", this);
            return;
        }

        ChunkManager chunkManager = ChunkManager.Instance;
        if (chunkManager == null)
        {
            Debug.LogWarning("PauseMinimapController: ChunkManager ainda não está disponível.", this);
            return;
        }

        string currentChunkID = chunkManager.CurrentChunkID;

        foreach (Transform room in grid)
        {
            Image roomImage = room.GetComponent<Image>();
            if (roomImage == null)
                continue;

            if (!chunkManager.IsChunkRegistered(room.name) ||
                !chunkManager.HasVisitedChunk(room.name))
            {
                roomImage.color = undiscoveredColor;
                continue;
            }

            roomImage.color = room.name == currentChunkID
                ? currentRoomColor
                : visitedRoomColor;
        }

        if (string.IsNullOrEmpty(currentChunkID))
            return;

        Transform currentRoom = grid.Find(currentChunkID);
        if (currentRoom == null)
        {
            Debug.LogWarning($"PauseMinimapController: não existe um slot para a chunk '{currentChunkID}'.", this);
            return;
        }

        if (playerMarker == null)
        {
            Transform markerTransform = grid.Find("A2/PlayerMarker");
            if (markerTransform != null)
                playerMarker = markerTransform.GetComponent<Image>();
        }

        if (playerMarker == null)
        {
            Debug.LogWarning("PauseMinimapController: não encontrei 'A2/PlayerMarker'.", this);
            return;
        }

        RectTransform markerRect = playerMarker.rectTransform;
        markerRect.SetParent(currentRoom, false);
        markerRect.anchoredPosition = Vector2.zero;
        markerRect.SetAsLastSibling();
    }
}
