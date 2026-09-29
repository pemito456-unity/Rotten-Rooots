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

    [Header("Ritmo do Batimento Cardíaco")]
    [Tooltip("Tempo máximo de espera entre batidas quando a vida está cheia (ex: 1.2 segundos).")]
    public float maxHeartbeatInterval = 1.2f;
    [Tooltip("Tempo mínimo de espera entre batidas quando a vida está crítica (ex: 0.3 segundos).")]
    public float minHeartbeatInterval = 0.3f;

    private float heartbeatTimer = 0f;

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

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (heartHUD != null)
            heartHUD.UpdateHeartUI(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
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

        // Calcula a porcentagem de vida (0.0 até 1.0)
        float healthPercent = currentHealth / maxHealth;

        // Quanto menor a vida, menor o intervalo entre as batidas (mais rápido fica)
        // Mathf.Lerp faz a transição suave baseada na vida atual
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

        // Reinicia a partida na cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}