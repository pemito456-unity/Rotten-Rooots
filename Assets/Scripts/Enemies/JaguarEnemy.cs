using UnityEngine;

public class JaguarEnemy : MonoBehaviour, IDamageable
{
    [Header("Vida do Inimigo")]
    public float health = 30f;

    [Header("Configurações de Movimento")]
    public float moveSpeed = 3.5f;
    public float detectionRadius = 6f;

    [Header("Configurações de Ataque")]
    public float damage = 25f;
    public float attackCooldown = 1f;

    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private float lastAttackTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            if (playerHealth != null)
            {
                // Envia a posição da Onça para o Knockback jogar o Player na direção oposta
                playerHealth.TakeDamage(damage, transform.position);
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(float amount)
    {
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}