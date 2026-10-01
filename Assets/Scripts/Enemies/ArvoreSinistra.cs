using UnityEngine;

public class ArvoreSinistra : MonoBehaviour
{
    [Header("Atributos do Obstáculo")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Ira da Floresta")]
    public float wrathOnDestroy = 15f; // Quanto aumenta a ira se destruir a árvore

    [Header("Feedbacks")]
    public SpriteRenderer spriteRenderer;
    public Color damageFlashColor = Color.red;
    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;

        // Se o SpriteRenderer não estiver atribuído, tenta obter o componente
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // Método de dano genérico (para faca e tiros)
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"[Árvore Sinistra] Vida restante: {currentHealth}/{maxHealth}");

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            DestroyTree();
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void DestroyTree()
    {
        // Notifica o ForestWrathManager se existir na cena
        if (ForestWrathManager.Instance != null)
        {
            ForestWrathManager.Instance.AddWrath(wrathOnDestroy);
        }

        Debug.Log("[Árvore Sinistra] A floresta sentiu a destruição da árvore!");
        Destroy(gameObject);
    }
}