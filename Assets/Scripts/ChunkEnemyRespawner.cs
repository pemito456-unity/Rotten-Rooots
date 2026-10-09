using UnityEngine;

/// <summary>
/// Recreates the configured enemy whenever this chunk is activated again.
/// Attach this component to the chunk root; it is used only by B4.
/// </summary>
public class ChunkEnemyRespawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private GameObject currentEnemy;
    private Vector3 spawnLocalPosition;
    private Quaternion spawnLocalRotation;
    private Vector3 spawnLocalScale;
    private bool hasSpawnPoint;

    private void Awake()
    {
        MonkeyEnemy existingEnemy = GetComponentInChildren<MonkeyEnemy>(true);
        if (existingEnemy == null)
        {
            Debug.LogError("B4EnemyRespawner: não encontrei o macaco original dentro da chunk B4.", this);
            return;
        }

        currentEnemy = existingEnemy.gameObject;
        spawnLocalPosition = existingEnemy.transform.localPosition;
        spawnLocalRotation = existingEnemy.transform.localRotation;
        spawnLocalScale = existingEnemy.transform.localScale;
        hasSpawnPoint = true;
    }

    private void OnEnable()
    {
        // O primeiro OnEnable encontra o macaco original. Nos retornos, um macaco
        // derrotado já foi destruído e será recriado no ponto inicial da sala.
        if (currentEnemy != null || !hasSpawnPoint)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogError("B4EnemyRespawner: atribua o prefab Enemy_Macaco.", this);
            return;
        }

        currentEnemy = Instantiate(enemyPrefab, transform);
        currentEnemy.transform.localPosition = spawnLocalPosition;
        currentEnemy.transform.localRotation = spawnLocalRotation;
        currentEnemy.transform.localScale = spawnLocalScale;
    }
}
