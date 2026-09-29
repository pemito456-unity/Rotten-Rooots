using UnityEngine;

public class MonkeyEnemy : MonoBehaviour, IDamageable
{
    [Header("Vida do Inimigo")]
    public float health = 20f;

    [Header("Configurações Neutras")]
    public float aggroDistance = 3.5f;
    public bool isAggroed = false;

    [Header("Ataque à Distância")]
    public GameObject stonePrefab;
    public Transform throwPoint;
    public float throwInterval = 2f;
    public float stoneSpeed = 6f;

    [Header("Feedback Sonoro")]
    public AudioSource audioSource;
    public AudioClip aggroSound;

    private Transform playerTransform;
    private float throwTimer;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (!isAggroed && distance <= aggroDistance)
        {
            TriggerAggro();
        }

        if (isAggroed)
        {
            throwTimer += Time.deltaTime;
            if (throwTimer >= throwInterval)
            {
                throwTimer = 0f;
                ThrowStone();
            }
        }
    }

    public void TriggerAggro()
    {
        if (isAggroed) return;

        isAggroed = true;
        if (audioSource != null && aggroSound != null)
        {
            audioSource.PlayOneShot(aggroSound);
        }
    }

    private void ThrowStone()
    {
        if (stonePrefab == null || throwPoint == null || playerTransform == null) return;

        GameObject stone = Instantiate(stonePrefab, throwPoint.position, Quaternion.identity);
        Vector2 dir = ((Vector2)playerTransform.position - (Vector2)throwPoint.position).normalized;

        MonkeyStone stoneScript = stone.GetComponent<MonkeyStone>();
        if (stoneScript != null)
        {
            stoneScript.Launch(dir);
        }  
    }

    public void TakeDamage(float amount)
    {
        TriggerAggro(); // Provoca o macaco ao tomar tiro
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (KillFeedbackManager.Instance != null)
        {
            KillFeedbackManager.Instance.TriggerFirstKillFeedback();
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroDistance);
    }
}