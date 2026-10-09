using UnityEngine;

public class SimpleBullet2D : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public float damage = 10f; // Dano do disparo

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora colisão com o próprio Player
        if (other.CompareTag("Player")) return;

        // Tenta aplicar dano a qualquer objeto que implemente IDamageable
        // O collider pode estar num filho do inimigo; procura o componente no pai também.
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Destrói a bala se bater em paredes/cenário
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
