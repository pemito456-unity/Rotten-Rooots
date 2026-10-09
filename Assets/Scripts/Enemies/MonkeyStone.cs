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
        Launch(direction, speed);
    }

    public void Launch(Vector2 direction, float speedOverride)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("A pedra do macaco precisa de um Rigidbody2D.", this);
            return;
        }

        // Garante que o projétil viaje com a velocidade configurada no macaco.
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = direction.normalized * speedOverride;
#else
        rb.velocity = direction.normalized * speedOverride;
#endif
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
