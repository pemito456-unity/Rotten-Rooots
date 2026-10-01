using UnityEngine;

public class JaguarEnemy : MonoBehaviour, IDamageable
{
    [Header("Vida do Inimigo")]
    public float health = 30f;
    private float maxHealth;

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
    private EnemyNametag nametag;

    private void Start()
    {
        maxHealth = health; // Guarda a vida total inicial
        rb = GetComponent<Rigidbody2D>();
        nametag = GetComponent<EnemyNametag>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Garante que a barra comece 100% cheia
        if (nametag != null)
        {
            nametag.UpdateHealthBar(health, maxHealth);
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = direction * moveSpeed;
#else
            rb.velocity = direction * moveSpeed;
#endif
        }
        else
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        // Atualiza o preenchimento da barra na tela
        if (nametag != null)
        {
            nametag.UpdateHealthBar(health, maxHealth);
        }

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