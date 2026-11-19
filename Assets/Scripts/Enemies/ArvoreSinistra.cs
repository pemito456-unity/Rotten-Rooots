using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(EvidencePhotoTarget))]
public class ArvoreSinistra : MonoBehaviour, IDamageable
{
    [Header("Evidência fotográfica")]
    [SerializeField, Min(1)] private int photoEvidenceIndex = 4;
    [SerializeField] private string photoEvidenceName = "Árvore mutante";
    private EvidencePhotoTarget photoEvidence;

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
    public float attackRange = 0.25f;

    [Header("Ira da Floresta")]
    public float wrathOnDestroy = 15f;

    [Header("Bloqueio de passagem")]
    [Tooltip("Árvores de passagem informam à porta quando são removidas.")]
    public bool isPassageBlocker;
    [Tooltip("Quando ativo, arma de fogo e faca não causam dano; só o kit remove a árvore.")]
    public bool onlyRemovableByKit;
    [Tooltip("Bloqueios permanentes não podem ser removidos nem pelo kit.")]
    public bool cannotBeRemovedByKit;
    private string passageId;
    private bool isBeingRemoved;

    public bool OnlyRemovableByKit
    {
        get
        {
            return onlyRemovableByKit;
        }
    }

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
    private Animator animator;
    private BoxCollider2D spriteDamageArea;
    private EnemyNametag nametag;
    private float lastAttackTime;
    private bool playerWasInSoundRange;

    private void Awake()
    {
        if (GetComponent<EvidencePhotoTarget>() == null)
            gameObject.AddComponent<EvidencePhotoTarget>();

        ConfigurePhotoEvidence();
    }

    private void OnValidate()
    {
        photoEvidenceIndex = Mathf.Max(1, photoEvidenceIndex);
        ConfigurePhotoEvidence();
    }

    private void ConfigurePhotoEvidence()
    {
        if (photoEvidence == null)
            photoEvidence = GetComponent<EvidencePhotoTarget>();

        if (photoEvidence == null)
            return;

        photoEvidence.category = EvidencePhotoTarget.EvidenceCategory.MutantTree;
        photoEvidence.evidenceIndex = photoEvidenceIndex;
        photoEvidence.evidenceName = photoEvidenceName;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        {
            originalColor = spriteRenderer.color;
            CreateSpriteDamageArea();
        }

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

        UpdateSpriteDamageArea();

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
                return;
        }

        float centerDistance = Vector2.Distance(transform.position, playerTransform.position);
        UpdateProximitySound(centerDistance);
    }

    private void CreateSpriteDamageArea()
    {
        if (spriteRenderer.sprite == null)
            return;

        spriteDamageArea = gameObject.AddComponent<BoxCollider2D>();
        spriteDamageArea.isTrigger = true;
        UpdateSpriteDamageArea();
    }

    private void UpdateSpriteDamageArea()
    {
        if (spriteDamageArea == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        spriteDamageArea.offset = spriteBounds.center;
        spriteDamageArea.size = (Vector2)spriteBounds.size + Vector2.one * (attackRange * 2f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || Time.time < lastAttackTime + attackCooldown)
            return;

        if (animator != null)
            animator.SetTrigger("Attack");

        health.TakeDamage(damage, transform.position);
        lastAttackTime = Time.time;
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

    public void ConfigureAsPassageBlocker(string id, bool kitOnly, bool cannotBeRemovedByKit)
    {
        isPassageBlocker = true;
        passageId = id;
        onlyRemovableByKit = kitOnly;
        this.cannotBeRemovedByKit = cannotBeRemovedByKit;
    }

    public bool DispelByKit()
    {
        if (isBeingRemoved || cannotBeRemovedByKit)
        {
            if (cannotBeRemovedByKit)
                Debug.Log("[Árvore Sinistra] Este bloqueio é permanente e não pode ser removido pelo kit.");
            return false;
        }

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
    }
}
