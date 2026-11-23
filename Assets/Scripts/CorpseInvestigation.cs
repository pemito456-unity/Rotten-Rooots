using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Permite abrir e ler as anotações do cadáver.
/// Os textos são configurados no Inspector; o painel fica na cena e o aviso fica no prefab.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(EvidencePhotoTarget))]
public class CorpseInvestigation : MonoBehaviour
{
    [Serializable]
    public class NotePage
    {
        public string title = "Anotação";
        [TextArea(5, 14)] public string text;
    }

    [Header("Investigação")]
    public string corpseName = "Cadáver";
    [Tooltip("Ordem narrativa do cadáver: 1, 2, 3 (penúltimo) ou 4 (último).")]
    [Range(0, 4)] public int storyOrder;
    [Min(0.1f)] public float interactionRange = 1.5f;
    [Tooltip("Ativa o incêndio final quando o jogador abre as anotações deste cadáver.")]
    public bool startsFinalFire;
    public List<NotePage> pages = new List<NotePage>();

    [Header("Aviso de interação (hierarquia do prefab)")]
    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Sprite gamepadAIcon;

    [Header("Foto de evidência")]
    public EvidencePhotoTarget photoEvidence;

    private static readonly List<CorpseInvestigation> activeCorpses =
        new List<CorpseInvestigation>();
    private Transform playerTransform;
    private Image promptGamepadIcon;

    private void Awake()
    {
        if (photoEvidence == null)
            photoEvidence = GetComponent<EvidencePhotoTarget>();

        if (photoEvidence != null)
        {
            photoEvidence.category = EvidencePhotoTarget.EvidenceCategory.Corpse;
            photoEvidence.evidenceName = corpseName;
        }

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
        SetupPromptIcon();
        RefreshPrompt();

        FindPlayer();
    }

    private void OnEnable()
    {
        if (!activeCorpses.Contains(this))
            activeCorpses.Add(this);
        InteractionPromptInput.DeviceChanged += OnInputDeviceChanged;
    }

    private void OnDisable()
    {
        activeCorpses.Remove(this);
        InteractionPromptInput.DeviceChanged -= OnInputDeviceChanged;
        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void SetupPromptIcon()
    {
        if (promptText == null || gamepadAIcon == null || promptText.transform.parent == null)
            return;

        GameObject iconObject = new GameObject("Gamepad A Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(promptText.transform.parent, false);
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(35f, -12f);
        rect.sizeDelta = new Vector2(18f, 18f);

        promptGamepadIcon = iconObject.GetComponent<Image>();
        promptGamepadIcon.sprite = gamepadAIcon;
        promptGamepadIcon.preserveAspect = true;
        promptGamepadIcon.raycastTarget = false;
        promptGamepadIcon.gameObject.SetActive(InteractionPromptInput.LastInputWasGamepad);
    }

    private void OnInputDeviceChanged(bool gamepad) => RefreshPrompt();

    private void RefreshPrompt()
    {
        if (promptText != null)
            promptText.text = $"{corpseName}\n{(InteractionPromptInput.LastInputWasGamepad ? "Examinar" : "F Examinar")}";

        if (promptGamepadIcon != null)
            promptGamepadIcon.gameObject.SetActive(InteractionPromptInput.LastInputWasGamepad);
    }

    private void Update()
    {
        if (promptCanvas == null)
            return;

        if (playerTransform == null)
            FindPlayer();

        CorpseNotePanelController panel = CorpseNotePanelController.Instance;
        bool shouldShow = false;
        if (playerTransform != null && (panel == null || !panel.IsOpen) && Time.timeScale > 0f)
            shouldShow = FindNearestNearby(playerTransform.position) == this;

        if (promptCanvas.activeSelf != shouldShow)
            promptCanvas.SetActive(shouldShow);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void OnValidate()
    {
        interactionRange = Mathf.Max(0.1f, interactionRange);
        if (photoEvidence == null)
            photoEvidence = GetComponent<EvidencePhotoTarget>();

        if (photoEvidence != null)
        {
            photoEvidence.category = EvidencePhotoTarget.EvidenceCategory.Corpse;
            photoEvidence.evidenceName = corpseName;
        }
    }

    public static CorpseInvestigation FindNearestNearby(Vector2 playerPosition)
    {
        CorpseInvestigation nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = activeCorpses.Count - 1; i >= 0; i--)
        {
            CorpseInvestigation corpse = activeCorpses[i];
            if (corpse == null || !corpse.isActiveAndEnabled)
            {
                activeCorpses.RemoveAt(i);
                continue;
            }

            float distance = Vector2.Distance(playerPosition, corpse.transform.position);
            if (distance <= corpse.interactionRange && distance < nearestDistance)
            {
                nearest = corpse;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    public static bool TryOpenNearby(Vector2 playerPosition)
    {
        CorpseInvestigation nearest = FindNearestNearby(playerPosition);
        if (nearest == null)
            return false;

        CorpseNotePanelController panel = CorpseNotePanelController.Instance;
        if (panel == null)
        {
            Debug.LogError(
                "CorpseInvestigation: não há CorpseNotePanelController na cena. " +
                "Adicione o painel de anotações à hierarquia da MainGame."
            );
            return false;
        }

        panel.Open(nearest);
        if (nearest.startsFinalFire && panel.IsOpen)
        {
            if (FinalFireSequence.Instance != null)
                FinalFireSequence.Instance.BeginFinalSequence();
            else
                Debug.LogError("CorpseInvestigation: não há FinalFireSequence configurado na cena.", nearest);
        }
        return true;
    }

}
