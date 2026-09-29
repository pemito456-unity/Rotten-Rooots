using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Referências de UI e Áudio")]
    public AnatomicalHeartHUD heartHUD;
    public AudioSource heartbeatAudioSource;

    [Header("Feedback Visual (Dano)")]
    public SpriteRenderer playerSprite;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Feedback Físico (Knockback)")]
    public float defaultKnockbackForce = 8f;
    public float knockbackDuration = 0.15f; // Duração em segundos do controle bloqueado
    public bool isKnockbacked { get; private set; }

    [Header("Ritmo do Batimento Cardíaco")]
    public float maxHeartbeatInterval = 1.2f;
    public float minHeartbeatInterval = 0.3f;

    private float heartbeatTimer = 0f;
    private Rigidbody2D rb;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        if (playerSprite != null)
            originalColor = playerSprite.color;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (heartHUD != null)
            heartHUD.UpdateHeartUI(currentHealth, maxHealth);
    }

    private void Update()
    {
        HandleDynamicHeartbeat();
    }

    public void TakeDamage(float amount, Vector2 damageSourcePosition = default, float knockbackForce = -1f)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (heartHUD != null)
            heartHUD.UpdateHeartUI(currentHealth, maxHealth);

        // Aplica o Knockback
        float finalForce = knockbackForce < 0 ? defaultKnockbackForce : knockbackForce;
        ApplyKnockback(damageSourcePosition, finalForce);

        // Aplica o Flash Vermelho
        TriggerSpriteFlash();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector2 attackerPos, float force)
    {
        if (rb == null) return;

        // Se a posição de origem não for informada, empurra na direção oposta ao olhar do player
        Vector2 knockbackDir;
        if (attackerPos == Vector2.zero)
        {
            PlayerController2D pc = GetComponent<PlayerController2D>();
            knockbackDir = pc != null ? -pc.lastFacingDirection : Vector2.left;
        }
        else
        {
            knockbackDir = ((Vector2)transform.position - attackerPos).normalized;
        }

        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(KnockbackRoutine(knockbackDir, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockbacked = true;

        // Limpa a velocidade atual e aplica o impulso seco
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockbacked = false;
    }

    private void TriggerSpriteFlash()
    {
        if (playerSprite == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashDamageRoutine());
    }

    private IEnumerator FlashDamageRoutine()
    {
        playerSprite.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        playerSprite.color = originalColor;
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        if (heartHUD != null)
            heartHUD.UpdateHeartUI(currentHealth, maxHealth);
    }

    private void HandleDynamicHeartbeat()
    {
        if (heartbeatAudioSource == null) return;

        float healthPercent = currentHealth / maxHealth;
        float currentInterval = Mathf.Lerp(minHeartbeatInterval, maxHeartbeatInterval, healthPercent);

        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer >= currentInterval)
        {
            heartbeatTimer = 0f;
            heartbeatAudioSource.PlayOneShot(heartbeatAudioSource.clip);
        }
    }

    private void Die()
    {
        Debug.Log("Player morreu!");

        if (heartbeatAudioSource != null)
            heartbeatAudioSource.Stop();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}