using UnityEngine;

public class MonkeyEnemy : MonoBehaviour, IDamageable
{
    [Header("Vida do Inimigo")]
    public float health = 20f;

    [Header("Configurações Neutras")]
    public float aggroDistance = 3.5f;
    public bool isAggroed = false;

    [Header("Ataque à Distância")]
    public GameObject stonePrefab;
    public Transform throwPoint;
    public float throwInterval = 2f;
    public float stoneSpeed = 6f;

    [Header("Feedback Sonoro")]
    public AudioSource audioSource;
    public AudioClip aggroSound;

    [Header("Som ao receber dano")]
    public AudioClip damageSound;
    [Range(0f, 1f)] public float damageSoundVolume = 0.8f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private EnemyNametag nametag;
    private float maxHealth;
    private float throwTimer;
    private bool playerWasInAggroRange;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        nametag = GetComponent<EnemyNametag>();
        maxHealth = health;

        // O macaco é um inimigo neutro parado; o jogador não deve empurrá-lo.
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (nametag != null)
            nametag.UpdateHealthBar(health, maxHealth);
    }

    private void Update()
    {
        // Também cobre o caso em que o Player é criado depois do macaco na cena.
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            playerTransform = player.transform;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        bool playerIsInAggroRange = distance <= aggroDistance;
        if (playerIsInAggroRange && !playerWasInAggroRange)
        {
            if (isAggroed)
                PlayAggroSound();
            else
                TriggerAggro(); // A primeira aproximação também inicia o estado hostil.
        }
        playerWasInAggroRange = playerIsInAggroRange;

        if (isAggroed)
        {
            throwTimer += Time.deltaTime;
            if (throwTimer >= Mathf.Max(0.1f, throwInterval))
            {
                throwTimer = 0f;
                ThrowStone();
            }
        }
    }

    public void TriggerAggro()
    {
        if (isAggroed) return;

        isAggroed = true;
        // Dá um intervalo completo antes do primeiro arremesso, deixando o estado
        // neutro/hostil previsível e evitando disparo no mesmo frame da provocação.
        throwTimer = 0f;
        PlayAggroSound();
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
        playerWasInAggroRange = false;
    }

    private void ThrowStone()
    {
        if (stonePrefab == null || throwPoint == null || playerTransform == null) return;

        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)throwPoint.position).normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.left;

        GameObject stone = Instantiate(stonePrefab, throwPoint.position, Quaternion.identity);

        MonkeyStone stoneScript = stone.GetComponent<MonkeyStone>();
        if (stoneScript != null)
        {
            stoneScript.Launch(direction, stoneSpeed);
        }
        else
        {
            Debug.LogError("O prefab da pedra do macaco não tem o componente MonkeyStone.", stone);
            Destroy(stone);
        }
    }

    public void TakeDamage(float amount)
    {
        TriggerAggro(); // Provoca o macaco ao tomar tiro
        PlayDamageSound();
        health -= amount;
        if (nametag != null)
            nametag.UpdateHealthBar(health, maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    private void PlayDamageSound()
    {
        if (damageSound == null) return;
        AudioSource.PlayClipAtPoint(damageSound, transform.position, damageSoundVolume);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroDistance);
    }
}
