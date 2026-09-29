using UnityEngine;

public class MonkeyStone : MonoBehaviour
{
    public float damage = 15f;
    public float speed = 8f;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Define a velocidade diretamente no Rigidbody2D
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora colisão com Inimigos e com o próprio ThrowPoint/Macaco
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
        else
        {
            // Destrói ao bater em paredes ou cenários
            Destroy(gameObject);
        }
    }
}