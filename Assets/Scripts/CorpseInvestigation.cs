using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Investigable corpse. Its note pages are authored directly in the Inspector.
/// Note pages use the scene-authored panel; the proximity prompt is authored in the prefab.
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
    [Min(0.1f)] public float interactionRange = 1.5f;
    public List<NotePage> pages = new List<NotePage>();

    [Header("Aviso de interação (hierarquia do prefab)")]
    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Foto de evidência")]
    public EvidencePhotoTarget photoEvidence;

    private static readonly List<CorpseInvestigation> activeCorpses =
        new List<CorpseInvestigation>();
    private Transform playerTransform;

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
        if (promptText != null)
            promptText.text = $"{corpseName}\n[E / Y] Examinar";

        FindPlayer();
    }

    private void OnEnable()
    {
        if (!activeCorpses.Contains(this))
            activeCorpses.Add(this);
    }

    private void OnDisable()
    {
        activeCorpses.Remove(this);
        if (promptCanvas != null)
            promptCanvas.SetActive(false);
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
        return true;
    }
}
