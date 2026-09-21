using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForestWrathManager : MonoBehaviour
{
    public static ForestWrathManager Instance;

    [Header("Configurações de Ira")]
    public float maxWrath = 100f;
    public Slider wrathBar;
    public TextMeshProUGUI wrathText;

    // --- DADO PERSISTENTE ENTRE CENAS ---
    private static float persistentWrath = 0f;

    public float currentWrath
    {
        get => persistentWrath;
        set
        {
            persistentWrath = Mathf.Clamp(value, 0f, maxWrath);
            UpdateUI();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetWrathOnPlay()
    {
        persistentWrath = 0f;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    // Aumenta a Ira (Tiros/Ataques)
    public void AddWrath(float amount)
    {
        currentWrath += amount;
    }

    // Reduz a Ira (Kits de Descontaminação)
    public void ReduceWrath(float amount)
    {
        currentWrath -= amount;
    }

    public void UpdateUI()
    {
        if (wrathBar != null)
        {
            wrathBar.maxValue = maxWrath;
            wrathBar.value = persistentWrath;
        }

        if (wrathText != null)
        {
            wrathText.text = $"{Mathf.RoundToInt(persistentWrath)}%";
        }
    }
}