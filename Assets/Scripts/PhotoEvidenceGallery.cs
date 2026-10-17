using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controls the evidence gallery authored in the MainGame scene.
/// Unlocked evidence persists while MainGame is reloaded during the current play session.
/// </summary>
public class PhotoEvidenceGallery : MonoBehaviour
{
    public static PhotoEvidenceGallery Instance { get; private set; }

    [Header("Galeria na cena")]
    [SerializeField, Min(1)] private int evidenceSlotCount = 10;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private GameObject galleryMenuButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI countLabel;
    [SerializeField] private Image[] evidenceImages;
    [SerializeField] private TextMeshProUGUI[] evidenceLabels;

    private static readonly HashSet<int> collectedEvidence = new HashSet<int>();
    private bool initialized;

    public bool IsOpen => galleryPanel != null && galleryPanel.activeSelf;
    public int CollectedCount => CountCollectedEvidence();
    public int EvidenceSlotCount => evidenceSlotCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCollectedEvidence()
    {
        collectedEvidence.Clear();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Initialize()
    {
        if (initialized)
            return;

        evidenceSlotCount = Mathf.Max(1, evidenceSlotCount);
        if (pausePanel == null || galleryPanel == null || galleryMenuButton == null ||
            backButton == null || countLabel == null || evidenceImages == null ||
            evidenceLabels == null || evidenceImages.Length < evidenceSlotCount ||
            evidenceLabels.Length < evidenceSlotCount)
        {
            Debug.LogError(
                "PhotoEvidenceGallery: confira no Inspector as referências da galeria " +
                "e os dez espaços de evidência."
            );
            return;
        }

        galleryPanel.SetActive(false);
        initialized = true;
        RefreshGallery();
    }

    public bool TryUnlockEvidence(EvidencePhotoTarget target)
    {
        if (target == null || !target.IsSupportedCategory)
            return false;

        if (target.evidenceIndex < 1 || target.evidenceIndex > evidenceSlotCount)
        {
            Debug.LogWarning(
                $"A evidência '{target.evidenceName}' usa o índice {target.evidenceIndex}, " +
                $"mas a galeria aceita índices de 1 a {evidenceSlotCount}.",
                target
            );
            return false;
        }

        bool isNewEvidence = collectedEvidence.Add(target.evidenceIndex);
        RefreshGallery();
        return isNewEvidence;
    }

    public static void ResetProgress()
    {
        collectedEvidence.Clear();
    }

    public void OpenGallery()
    {
        if (galleryPanel == null)
            return;

        RefreshGallery();
        galleryPanel.SetActive(true);
        SetSelectedObject(backButton != null ? backButton.gameObject : null);
    }

    public void CloseGallery()
    {
        if (galleryPanel == null || !galleryPanel.activeSelf)
            return;

        galleryPanel.SetActive(false);
        SetSelectedObject(galleryMenuButton);
    }

    public void CloseGalleryWithoutChangingSelection()
    {
        if (galleryPanel != null)
            galleryPanel.SetActive(false);
    }

    private void RefreshGallery()
    {
        if (galleryPanel == null || countLabel == null || evidenceImages == null ||
            evidenceLabels == null)
        {
            return;
        }

        Dictionary<int, EvidencePhotoTarget> targetsByIndex = FindTargetsByIndex();
        countLabel.text = $"Evidências: {CollectedCount}/{evidenceSlotCount}";

        for (int i = 0; i < evidenceSlotCount; i++)
        {
            int evidenceIndex = i + 1;
            bool isUnlocked = collectedEvidence.Contains(evidenceIndex);
            EvidencePhotoTarget target;
            targetsByIndex.TryGetValue(evidenceIndex, out target);

            if (evidenceImages[i] == null || evidenceLabels[i] == null)
                continue;

            if (!isUnlocked)
            {
                evidenceImages[i].color = Color.black;
                evidenceLabels[i].text = "???";
                continue;
            }

            if (target != null && target.galleryImage != null)
            {
                evidenceImages[i].sprite = target.galleryImage;
                evidenceImages[i].color = Color.white;
                evidenceLabels[i].text = target.evidenceName;
            }
            else
            {
                evidenceImages[i].color = new Color(0.2f, 0.24f, 0.2f, 1f);
                evidenceLabels[i].text = "Imagem pendente";
            }
        }
    }

    private Dictionary<int, EvidencePhotoTarget> FindTargetsByIndex()
    {
        Dictionary<int, EvidencePhotoTarget> targetsByIndex =
            new Dictionary<int, EvidencePhotoTarget>();
        EvidencePhotoTarget[] allTargets = Resources.FindObjectsOfTypeAll<EvidencePhotoTarget>();

        foreach (EvidencePhotoTarget target in allTargets)
        {
            if (target == null || !target.gameObject.scene.IsValid() ||
                !target.gameObject.scene.isLoaded ||
                !target.IsSupportedCategory ||
                target.evidenceIndex < 1 || target.evidenceIndex > evidenceSlotCount)
            {
                continue;
            }

            if (!targetsByIndex.ContainsKey(target.evidenceIndex))
                targetsByIndex.Add(target.evidenceIndex, target);
        }

        return targetsByIndex;
    }

    private int CountCollectedEvidence()
    {
        int count = 0;
        foreach (int index in collectedEvidence)
        {
            if (index >= 1 && index <= evidenceSlotCount)
                count++;
        }

        return count;
    }

    private void SetSelectedObject(GameObject selected)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        if (selected != null && selected.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(selected);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
