using UnityEngine;

public class SimpleBullet2D : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 1; // Cada tiro tira 1 de vida

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

        // Tenta pegar o script de inimigo do objeto atingido
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Destrói a bala ao acertar o inimigo
            return;
        }

        // Destrói a bala se bater em paredes/cenário
        Destroy(gameObject);
    }
}