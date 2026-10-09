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

    [Header("Som ao receber dano")]
    public AudioClip damageSound;
    [Range(0f, 1f)] public float damageSoundVolume = 1f;

    [Header("Feedback Visual (Dano)")]
    public SpriteRenderer playerSprite;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Vinheta vermelha ao receber dano")]
    public Color screenDamageColor = new Color(1f, 0f, 0f, 0.72f);
    [Min(0.05f)] public float screenDamagePulseDuration = 0.18f;
    [Min(1)] public int screenDamagePulseCount = 2;

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
    private Coroutine screenDamageCoroutine;
    private Coroutine knockbackCoroutine;
    private UnityEngine.UI.Image screenDamageImage;
    private Texture2D screenDamageTexture;
    private Sprite screenDamageSprite;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        if (playerSprite != null)
            originalColor = playerSprite.color;

        CreateScreenDamageVignette();
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

        PlayDamageSound();

        if (heartHUD != null)
            heartHUD.UpdateHeartUI(currentHealth, maxHealth);

        // Aplica o Knockback
        float finalForce = knockbackForce < 0 ? defaultKnockbackForce : knockbackForce;
        ApplyKnockback(damageSourcePosition, finalForce);

        // Aplica o Flash Vermelho
        TriggerSpriteFlash();
        TriggerScreenDamageFlash();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void PlayDamageSound()
    {
        if (heartbeatAudioSource == null || damageSound == null)
            return;

        heartbeatAudioSource.PlayOneShot(damageSound, damageSoundVolume);
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

    private void CreateScreenDamageVignette()
    {
        GameObject canvasObject = new GameObject(
            "PlayerDamageVignetteCanvas",
            typeof(RectTransform),
            typeof(UnityEngine.Canvas),
            typeof(UnityEngine.UI.CanvasScaler)
        );

        UnityEngine.Canvas canvas = canvasObject.GetComponent<UnityEngine.Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10000;

        UnityEngine.UI.CanvasScaler scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject vignetteObject = new GameObject(
            "RedScreenEdges",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(UnityEngine.UI.Image)
        );
        vignetteObject.transform.SetParent(canvasObject.transform, false);

        RectTransform vignetteRect = vignetteObject.GetComponent<RectTransform>();
        vignetteRect.anchorMin = Vector2.zero;
        vignetteRect.anchorMax = Vector2.one;
        vignetteRect.offsetMin = Vector2.zero;
        vignetteRect.offsetMax = Vector2.zero;

        screenDamageImage = vignetteObject.GetComponent<UnityEngine.UI.Image>();
        screenDamageImage.sprite = CreateVignetteSprite();
        screenDamageImage.color = new Color(screenDamageColor.r, screenDamageColor.g,
            screenDamageColor.b, 0f);
        screenDamageImage.raycastTarget = false;
    }

    private Sprite CreateVignetteSprite()
    {
        const int textureSize = 256;
        const float fadeWidth = 0.16f;
        screenDamageTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        screenDamageTexture.name = "GeneratedRedDamageVignette";
        screenDamageTexture.wrapMode = TextureWrapMode.Clamp;
        screenDamageTexture.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
        {
            float verticalEdgeDistance = Mathf.Min(y, textureSize - 1 - y) / (textureSize * 0.5f);
            for (int x = 0; x < textureSize; x++)
            {
                float horizontalEdgeDistance = Mathf.Min(x, textureSize - 1 - x) / (textureSize * 0.5f);
                float edgeDistance = Mathf.Min(horizontalEdgeDistance, verticalEdgeDistance);
                float fade = Mathf.Clamp01(edgeDistance / fadeWidth);
                fade = fade * fade * (3f - 2f * fade);
                byte alpha = (byte)Mathf.RoundToInt((1f - fade) * 255f);
                pixels[y * textureSize + x] = new Color32(255, 255, 255, alpha);
            }
        }

        screenDamageTexture.SetPixels32(pixels);
        screenDamageTexture.Apply(false, true);

        screenDamageSprite = Sprite.Create(screenDamageTexture,
            new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);
        screenDamageSprite.name = "GeneratedRedDamageVignetteSprite";
        return screenDamageSprite;
    }

    private void TriggerScreenDamageFlash()
    {
        if (screenDamageImage == null) return;

        if (screenDamageCoroutine != null)
            StopCoroutine(screenDamageCoroutine);

        screenDamageCoroutine = StartCoroutine(ScreenDamageFlashRoutine());
    }

    private IEnumerator ScreenDamageFlashRoutine()
    {
        int pulseCount = Mathf.Max(1, screenDamagePulseCount);
        float pulseDuration = Mathf.Max(0.05f, screenDamagePulseDuration);

        for (int i = 0; i < pulseCount; i++)
        {
            yield return AnimateScreenDamageAlpha(0f, screenDamageColor.a, pulseDuration * 0.35f);
            yield return AnimateScreenDamageAlpha(screenDamageColor.a, 0f, pulseDuration * 0.65f);
        }

        screenDamageImage.color = new Color(screenDamageColor.r, screenDamageColor.g,
            screenDamageColor.b, 0f);
        screenDamageCoroutine = null;
    }

    private IEnumerator AnimateScreenDamageAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            screenDamageImage.color = new Color(screenDamageColor.r, screenDamageColor.g,
                screenDamageColor.b, alpha);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        screenDamageImage.color = new Color(screenDamageColor.r, screenDamageColor.g,
            screenDamageColor.b, endAlpha);
    }

    private void OnDestroy()
    {
        if (screenDamageSprite != null)
            Destroy(screenDamageSprite);
        if (screenDamageTexture != null)
            Destroy(screenDamageTexture);
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
