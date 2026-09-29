using UnityEngine;

public class BatEnemy : MonoBehaviour, IDamageable
{
    [Header("Vida do Inimigo")]
    public float health = 15f;

    [Header("Configurações de Ataque e Repulsão")]
    public float moveSpeed = 4f;
    public float pushForce = 8f;
    public float damage = 10f;

    private Transform playerTransform;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth healthScript = collision.gameObject.GetComponent<PlayerHealth>();

            // Envia a posição para o PlayerHealth calcular o knockback e acionar o flash
            if (healthScript != null)
            {
                healthScript.TakeDamage(damage, transform.position, pushForce);
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
}