using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the pause map with explored chunks, the current room and passage states.
/// </summary>
public class PauseMinimapController : MonoBehaviour
{
    [Header("Room colors")]
    [SerializeField] private Color undiscoveredColor = new Color(0.09f, 0.11f, 0.10f, 1f);
    [SerializeField] private Color visitedRoomColor = new Color(0.12f, 0.24f, 0.14f, 1f);
    [SerializeField] private Color currentRoomColor = new Color(0.24f, 0.72f, 0.28f, 1f);

    [Header("Passage colors")]
    [SerializeField] private Color openPassageColor = new Color(0.58f, 0.82f, 0.60f, 1f);
    [SerializeField] private Color blockedPassageColor = new Color(0.55f, 0.18f, 0.16f, 1f);

    private Transform grid;
    private Image playerMarker;
    private Transform currentRoomHighlight;
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
            Debug.LogError("PauseMinimapController: child 'Grid' was not found.", this);
            return;
        }

        ChunkManager chunkManager = ChunkManager.Instance;
        if (chunkManager == null)
        {
            Debug.LogWarning("PauseMinimapController: ChunkManager is not ready.", this);
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

        UpdatePassageMarkers(chunkManager);

        if (string.IsNullOrEmpty(currentChunkID))
            return;

        Transform currentRoom = grid.Find(currentChunkID);
        if (currentRoom == null)
        {
            Debug.LogWarning($"PauseMinimapController: no slot for chunk '{currentChunkID}'.", this);
            return;
        }

        if (currentRoomHighlight == null)
            currentRoomHighlight = grid.Find("A2/CurrentRoomHighlight");

        if (currentRoomHighlight != null)
        {
            currentRoomHighlight.SetParent(currentRoom, false);
            currentRoomHighlight.SetAsFirstSibling();
        }

        if (playerMarker == null)
        {
            Transform markerTransform = grid.Find("A2/PlayerMarker");
            if (markerTransform != null)
                playerMarker = markerTransform.GetComponent<Image>();
        }

        if (playerMarker == null)
        {
            Debug.LogWarning("PauseMinimapController: 'A2/PlayerMarker' was not found.", this);
            return;
        }

        RectTransform markerRect = playerMarker.rectTransform;
        markerRect.SetParent(currentRoom, false);
        markerRect.anchoredPosition = Vector2.zero;
        markerRect.SetAsLastSibling();
    }

    private void UpdatePassageMarkers(ChunkManager chunkManager)
    {
        foreach (Transform room in grid)
        {
            foreach (string direction in new[] { "N", "E", "S", "W" })
            {
                Transform marker = room.Find("Connection_" + direction);
                if (marker != null)
                    marker.gameObject.SetActive(false);
            }
        }

        foreach (ChunkManager.ChunkData chunk in chunkManager.chunks)
        {
            if (chunk == null || chunk.chunkObject == null ||
                !chunkManager.HasVisitedChunk(chunk.chunkID))
                continue;

            Transform room = grid.Find(chunk.chunkID);
            if (room == null)
                continue;

            ChunkDoorTrigger[] doors = chunk.chunkObject.GetComponentsInChildren<ChunkDoorTrigger>(true);
            foreach (ChunkDoorTrigger door in doors)
            {
                if (door == null || string.IsNullOrEmpty(door.targetChunkID) ||
                    !chunkManager.IsChunkRegistered(door.targetChunkID))
                    continue;

                string direction = GetDirection(chunk.chunkID, door.targetChunkID);
                if (string.IsNullOrEmpty(direction))
                    continue;

                Transform marker = room.Find("Connection_" + direction);
                if (marker == null)
                    continue;

                Image markerImage = marker.GetComponent<Image>();
                if (markerImage != null)
                {
                    bool isBlocked = door.isLocked ||
                        (door.useWrathBlocker &&
                         ForestWrathManager.IsPassageBlocked(door.passageBlockerId));

                    if (!marker.gameObject.activeSelf || isBlocked)
                        markerImage.color = isBlocked ? blockedPassageColor : openPassageColor;
                }

                marker.gameObject.SetActive(true);
            }
        }
    }

    private static string GetDirection(string sourceChunk, string targetChunk)
    {
        if (string.IsNullOrEmpty(sourceChunk) || string.IsNullOrEmpty(targetChunk) ||
            sourceChunk.Length < 2 || targetChunk.Length < 2)
            return null;

        int sourceRow;
        int targetRow;
        if (!int.TryParse(sourceChunk.Substring(1), out sourceRow) ||
            !int.TryParse(targetChunk.Substring(1), out targetRow))
            return null;

        char sourceColumn = char.ToUpperInvariant(sourceChunk[0]);
        char targetColumn = char.ToUpperInvariant(targetChunk[0]);

        if (sourceRow == targetRow && targetColumn == sourceColumn + 1)
            return "E";
        if (sourceRow == targetRow && targetColumn == sourceColumn - 1)
            return "W";
        if (sourceColumn == targetColumn && targetRow == sourceRow + 1)
            return "S";
        if (sourceColumn == targetColumn && targetRow == sourceRow - 1)
            return "N";

        return null;
    }
}
