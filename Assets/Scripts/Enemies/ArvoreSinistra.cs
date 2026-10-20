using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class ArvoreSinistra : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Comportamento fixo")]
    [Tooltip("Mantido para compatibilidade com valores antigos do Inspector. A árvore não se move.")]
    public float moveSpeed = 3.5f;
    [Tooltip("Raio em que a árvore percebe o jogador e pode tocar o som de alerta.")]
    public float detectionRadius = 6f;

    [Header("Ataque")]
    public float damage = 25f;
    public float attackCooldown = 1f;
    public float attackRange = 1.4f;

    [Header("Ira da Floresta")]
    public float wrathOnDestroy = 15f;

    [Header("Bloqueio de passagem")]
    [Tooltip("Árvores de passagem informam à porta quando são removidas.")]
    public bool isPassageBlocker;
    [Tooltip("Quando ativo, arma de fogo e faca não causam dano; só o kit remove a árvore.")]
    public bool onlyRemovableByKit;
    private string passageId;
    private bool isBeingRemoved;

    public bool OnlyRemovableByKit => onlyRemovableByKit;

    [Header("Feedback de dano")]
    public SpriteRenderer spriteRenderer;
    public Color damageFlashColor = Color.red;
    private Color originalColor;

    [Header("Som de aproximação")]
    public AudioClip aggroSound;
    public AudioSource audioSource;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Som ao receber dano")]
    public AudioClip damageSound;
    [Range(0f, 1f)] public float damageSoundVolume = 0.8f;

    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private EnemyNametag nametag;
    private float lastAttackTime;
    private bool playerWasInSoundRange;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX |
                              RigidbodyConstraints2D.FreezePositionY;
            rb.linearVelocity = Vector2.zero;
        }

        nametag = GetComponent<EnemyNametag>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        FindPlayer();

        if (nametag != null)
            nametag.UpdateHealthBar(currentHealth, maxHealth);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        playerTransform = player.transform;
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void FixedUpdate()
    {
        // A árvore fica ancorada na passagem; ela ataca sem perseguir.
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
                return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        UpdateProximitySound(distanceToPlayer);

        if (distanceToPlayer <= attackRange &&
            Time.time >= lastAttackTime + attackCooldown &&
            playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform.position);
            lastAttackTime = Time.time;
        }
    }

    private void UpdateProximitySound(float distanceToPlayer)
    {
        bool playerIsInRange = distanceToPlayer <= detectionRadius;
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
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.PlayOneShot(aggroSound);
    }

    private void OnDisable()
    {
        playerWasInSoundRange = false;
    }

    public void TakeDamage(float amount)
    {
        if (onlyRemovableByKit || isBeingRemoved)
            return;

        PlayDamageSound();
        currentHealth -= amount;

        if (nametag != null)
            nametag.UpdateHealthBar(currentHealth, maxHealth);

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0f)
            DestroyTree();
    }

    public void ConfigureAsPassageBlocker(string id, bool kitOnly)
    {
        isPassageBlocker = true;
        passageId = id;
        onlyRemovableByKit = kitOnly;
    }

    public bool DispelByKit()
    {
        if (isBeingRemoved)
            return false;

        RemoveTree(false);
        return true;
    }

    private void PlayDamageSound()
    {
        if (damageSound == null) return;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.PlayOneShot(damageSound, damageSoundVolume);
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
        RemoveTree(true);
    }

    private void RemoveTree(bool defeatedByCombat)
    {
        if (isBeingRemoved)
            return;

        isBeingRemoved = true;

        if (defeatedByCombat && KillFeedbackManager.Instance != null)
            KillFeedbackManager.Instance.TriggerFirstKillFeedback();

        if (defeatedByCombat && ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.AddWrath(wrathOnDestroy);

        if (isPassageBlocker)
            ForestWrathManager.MarkPassageCleared(passageId);

        Debug.Log(defeatedByCombat
            ? "[Árvore Sinistra] A floresta sentiu a destruição da árvore!"
            : "[Árvore Sinistra] Árvore removida pelo kit de descontaminação.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
