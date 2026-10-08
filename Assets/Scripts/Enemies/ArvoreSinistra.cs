using System.Collections;
using UnityEngine;

public class ArvoreSinistra : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Perseguição (mesma lógica da onça)")]
    public float moveSpeed = 3.5f;
    public float detectionRadius = 6f;

    [Header("Ataque")]
    public float damage = 25f;
    public float attackCooldown = 1f;

    [Header("Ira da Floresta")]
    public float wrathOnDestroy = 15f;

    [Header("Feedback de dano")]
    public SpriteRenderer spriteRenderer;
    public Color damageFlashColor = Color.red;
    private Color originalColor;

    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private EnemyNametag nametag;
    private float lastAttackTime;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        nametag = GetComponent<EnemyNametag>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (nametag != null)
            nametag.UpdateHealthBar(currentHealth, maxHealth);
    }

    private void FixedUpdate()
    {
        // Segue o mesmo comportamento de perseguição do JaguarEnemy.
        if (playerTransform == null || rb == null)
            return;

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
        if (!collision.gameObject.CompareTag("Player") || Time.time < lastAttackTime + attackCooldown)
            return;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform.position);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (nametag != null)
            nametag.UpdateHealthBar(currentHealth, maxHealth);

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0f)
            DestroyTree();
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void DestroyTree()
    {
        if (KillFeedbackManager.Instance != null)
            KillFeedbackManager.Instance.TriggerFirstKillFeedback();

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.AddWrath(wrathOnDestroy);

        Debug.Log("[Árvore Sinistra] A floresta sentiu a destruição da árvore!");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
