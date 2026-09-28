using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Vida (em corações)")]
    public float maxHealth = 5f;
    public float currentHealth;

    [Header("Invulnerabilidade (i-frames)")]
    public float invulnerabilityTime = 0.8f;
    private float lastDamageTime = -999f;

    [Header("Batimento — vida baixa")]
    public AudioSource heartbeatSource;               // AudioSource c/ som de batida (Play On Awake OFF)
    [Range(0f, 1f)] public float lowHealthThreshold = 0.3f;
    public float minInterval = 0.35f;                 // intervalo com vida ~0 (rápido)
    public float maxInterval = 1.1f;                  // intervalo no limiar (lento)

    [Header("Debug (teste sem inimigos)")]
    public bool debugDamageKey = true;                // Tecla K = -1 coração

    public event Action<float, float> OnHealthChanged; // (atual, max)
    public event Action OnDeath;

    public bool IsDead { get; private set; }

    private Coroutine heartbeatRoutine;

    private void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    private void Start() => OnHealthChanged?.Invoke(currentHealth, maxHealth);

    private void Update()
    {
        if (debugDamageKey && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            TakeDamage(1f);
    }

    public void TakeDamage(float hearts)
    {
        if (IsDead) return;
        if (Time.time - lastDamageTime < invulnerabilityTime) return; // i-frames

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0f, currentHealth - hearts);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHeartbeat();

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float hearts)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + hearts);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHeartbeat();
    }

    private void UpdateHeartbeat()
    {
        float fraction = currentHealth / maxHealth;
        bool shouldBeat = fraction <= lowHealthThreshold && heartbeatSource != null;

        if (shouldBeat && heartbeatRoutine == null)
            heartbeatRoutine = StartCoroutine(HeartbeatLoop());
        else if (!shouldBeat && heartbeatRoutine != null)
            StopHeartbeat();
    }

    private IEnumerator HeartbeatLoop()
    {
        while (!IsDead)
        {
            heartbeatSource.Play();
            float fraction = Mathf.Clamp01(currentHealth / maxHealth / lowHealthThreshold);
            float interval = Mathf.Lerp(minInterval, maxInterval, fraction);
            yield return new WaitForSeconds(interval);
        }
    }

    private void StopHeartbeat()
    {
        if (heartbeatRoutine != null) { StopCoroutine(heartbeatRoutine); heartbeatRoutine = null; }
    }

    private void Die()
    {
        IsDead = true;
        StopHeartbeat();
        OnDeath?.Invoke();
    }
}
