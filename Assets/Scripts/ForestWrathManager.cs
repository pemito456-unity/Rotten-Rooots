using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForestWrathManager : MonoBehaviour
{
    public static ForestWrathManager Instance;

    [Header("Configurações de Ira")]
    public float maxWrath = 100f;
    [Tooltip("Imagem Filled da árvore. A árvore esvazia conforme a ira aumenta.")]
    public Image wrathFillImage;
    public TextMeshProUGUI wrathText;

    // --- DADO PERSISTENTE ENTRE CENAS ---
    private static float persistentWrath = 0f;
    private static readonly HashSet<string> activePassages = new HashSet<string>();
    private static readonly HashSet<string> clearedPassages = new HashSet<string>();

    public static event Action<float> WrathChanged;
    public static event Action<string> PassageCleared;

    public static bool IsPassageBlocked(string passageId)
    {
        return !string.IsNullOrEmpty(passageId) &&
               activePassages.Contains(passageId) &&
               !clearedPassages.Contains(passageId);
    }

    public static void MarkPassageBlocked(string passageId)
    {
        if (!string.IsNullOrEmpty(passageId) && !clearedPassages.Contains(passageId))
            activePassages.Add(passageId);
    }

    public static bool IsPassageCleared(string passageId)
    {
        return !string.IsNullOrEmpty(passageId) && clearedPassages.Contains(passageId);
    }

    public static void MarkPassageCleared(string passageId)
    {
        if (string.IsNullOrEmpty(passageId) || !clearedPassages.Add(passageId))
            return;

        PassageCleared?.Invoke(passageId);
    }

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
        activePassages.Clear();
        clearedPassages.Clear();
        WrathChanged = null;
        PassageCleared = null;
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
        float wrathRatio = maxWrath > 0f
            ? Mathf.Clamp01(persistentWrath / maxWrath)
            : 0f;

        if (wrathFillImage != null)
            wrathFillImage.fillAmount = 1f - wrathRatio;

        if (wrathText != null)
        {
            wrathText.text = $"{Mathf.RoundToInt(persistentWrath)}%";
        }

        float wrathPercent = maxWrath > 0f
            ? Mathf.Clamp01(persistentWrath / maxWrath) * 100f
            : 0f;
        WrathChanged?.Invoke(wrathPercent);
    }
}
