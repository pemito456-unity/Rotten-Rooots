using UnityEngine;

public class BatEnemy : MonoBehaviour, IDamageable
{
    [Header("Vida do Inimigo")]
    public float health = 15f;

    [Header("Configurações de Ataque e Repulsão")]
    public float moveSpeed = 4f;
    public float pushForce = 8f;
    public float damage = 10f;
    public float proximitySoundDistance = 6f;

    [Header("Som de aproximação")]
    public AudioClip aggroSound;
    public AudioSource audioSource;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private EnemyNametag nametag;
    private float maxHealth;
    private bool playerWasInSoundRange;

    private void Start()
    {
        maxHealth = health;
        rb = GetComponent<Rigidbody2D>();
        nametag = GetComponent<EnemyNametag>();

        if (nametag != null)
            nametag.UpdateHealthBar(health, maxHealth);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        UpdateProximitySound();

        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void UpdateProximitySound()
    {
        bool playerIsInRange = Vector2.Distance(transform.position, playerTransform.position)
            <= proximitySoundDistance;
        if (playerIsInRange && !playerWasInSoundRange)
            PlayAggroSound();

        playerWasInSoundRange = playerIsInRange;
    }

    private void PlayAggroSound()
    {
        if (aggroSound == null) return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.PlayOneShot(aggroSound);
    }

    private void OnDisable()
    {
        playerWasInSoundRange = false;
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
        if (nametag != null)
            nametag.UpdateHealthBar(health, maxHealth);

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
