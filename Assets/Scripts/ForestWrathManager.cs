using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForestWrathManager : MonoBehaviour
{
    public static ForestWrathManager Instance;

    [Header("Configurações da Ira")]
    public float currentWrath = 0f;
    public float maxWrath = 100f;

    [Header("UI (Placeholder)")]
    public Slider wrathBar;             // (Opcional) Slider da UI para mostrar a barra
    public TextMeshProUGUI wrathText;   // (Opcional) Texto mostrando a porcentagem

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    // Aumenta a Ira (Disparos, descarte, destruição de arbustos/inimigos)
    public void AddWrath(float amount)
    {
        currentWrath = Mathf.Clamp(currentWrath + amount, 0f, maxWrath);
        Debug.Log($"[Floresta] Ira aumentada para: {currentWrath}/{maxWrath}");
        UpdateUI();
        CheckWrathEvents();
    }

    // Diminui a Ira (Uso do Kit de Descontaminação)
    public void ReduceWrath(float amount)
    {
        currentWrath = Mathf.Clamp(currentWrath - amount, 0f, maxWrath);
        Debug.Log($"[Floresta] Ira reduzida para: {currentWrath}/{maxWrath}");
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (wrathBar != null)
        {
            wrathBar.maxValue = maxWrath;
            wrathBar.value = currentWrath;
        }

        if (wrathText != null)
        {
            wrathText.text = $"Ira da Floresta: {Mathf.RoundToInt((currentWrath / maxWrath) * 100)}%";
        }
    }

    private void CheckWrathEvents()
    {
        // Placeholders para futuros comportamentos dos inimigos
        if (currentWrath >= 80f)
        {
            Debug.LogWarning("ALERT: A floresta está furiossa! Mutações mais agressivas se aproximando!");
        }
    }
}