using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configurações do Inimigo")]
    public float speed = 3.5f;
    public int maxHealth = 2; // Vida total (2 tiros)

    private int currentHealth;
    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth; // Inicializa a vida cheia

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Método para receber dano dos tiros
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Inimigo atingido! Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Inimigo derrotado!");
        Destroy(gameObject);
    }
}