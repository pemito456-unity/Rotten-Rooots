using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>Ensina o jogador a registrar a primeira evidência fotográfica.</summary>
public class PhotoEvidenceTutorialController : MonoBehaviour
{
    [Header("Chunk e duração")]
    [SerializeField] private string firstChunkID = "A2";
    [SerializeField, Min(0f)] private float firstMessageDuration = 9f;
    [SerializeField, Min(0f)] private float successMessageDuration = 2.5f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

    [Header("Interface")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private CanvasGroup tutorialGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private Light2D evidenceGlowLight;
    [SerializeField, Min(0f)] private float evidenceGlowIntensity = 0.9f;
    [SerializeField, Min(0.01f)] private float evidenceGlowTransitionSpeed = 5f;

    private ChunkManager chunkManager;
    private PhotoEvidenceCamera photoCamera;
    private PlayerInventory playerInventory;
    private EvidencePhotoTarget currentTarget;
    private Coroutine fadeCoroutine;
    private Coroutine successCoroutine;
    private bool started;
    private bool completed;
    private bool targetPromptShown;
    private bool initialMessageHidden;
    private bool evidenceGlowRequested;
    private float startedAt;
    private float nextTargetCheck;

    private void Awake()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        SetEvidenceGlow(false);
    }

    private void Update()
    {
        UpdateEvidenceGlowFade();

        if (completed)
            return;

        if (chunkManager == null)
            chunkManager = ChunkManager.Instance;

        if (chunkManager == null || string.IsNullOrEmpty(chunkManager.CurrentChunkID))
            return;

        if (!started)
        {
            if (chunkManager.CurrentChunkID != firstChunkID)
                return;

            BeginTutorial();
        }

        if (chunkManager.CurrentChunkID != firstChunkID)
        {
            FinishTutorial();
            return;
        }

        BindPhotoCamera();
        UpdateTargetRecognition();

        if (!targetPromptShown && !initialMessageHidden && Time.timeScale > 0f &&
            Time.unscaledTime - startedAt >= firstMessageDuration)
        {
            initialMessageHidden = true;
            HidePanel();
        }
    }

    private void BeginTutorial()
    {
        started = true;
        startedAt = Time.unscaledTime;
        SetMessage(
            "REGISTRO DE EVIDÊNCIAS",
            "A câmera registra provas da contaminação. Encontre uma evidência e mire nela.",
            "Selecione o slot 2: tecla 2 ou Q/E; no controle, LB/RB (L1/R1)."
        );
        ShowPanel();
    }

    private void BindPhotoCamera()
    {
        if (photoCamera != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        playerInventory = player.GetComponent<PlayerInventory>();
        photoCamera = player.GetComponent<PhotoEvidenceCamera>();
        if (photoCamera != null)
            photoCamera.EvidencePhotographed += OnEvidencePhotographed;
    }

    private void UpdateTargetRecognition()
    {
        if (photoCamera == null)
            return;

        InventorySlotData selectedSlot = playerInventory != null
            ? playerInventory.SelectedSlot
            : null;
        if (selectedSlot == null || !selectedSlot.isCamera)
        {
            currentTarget = null;
            SetEvidenceGlow(false);
            return;
        }

        if (Time.unscaledTime >= nextTargetCheck)
        {
            currentTarget = photoCamera.GetTargetInFrame();
            nextTargetCheck = Time.unscaledTime + 0.12f;
        }

        if (currentTarget == null)
        {
            SetEvidenceGlow(false);
            return;
        }

        if (!targetPromptShown)
        {
            targetPromptShown = true;
            SetMessage(
                "EVIDÊNCIA NO ENQUADRAMENTO",
                "Aponte para a evidência e tire a foto para registrá-la.",
                "Pressione Espaço (teclado) ou A / ✕ (controle)."
            );
            ShowPanel();
        }

        UpdateEvidenceGlow(currentTarget);
    }

    private void UpdateEvidenceGlow(EvidencePhotoTarget target)
    {
        if (evidenceGlowLight == null || target == null)
            return;

        Vector2 targetPosition = target.PhotoPosition;
        evidenceGlowLight.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0f);
        SetEvidenceGlow(true);
    }

    private void SetEvidenceGlow(bool visible)
    {
        evidenceGlowRequested = visible;
        if (visible && evidenceGlowLight != null)
            evidenceGlowLight.enabled = true;
    }

    private void UpdateEvidenceGlowFade()
    {
        if (evidenceGlowLight == null)
            return;

        float targetIntensity = evidenceGlowRequested ? evidenceGlowIntensity : 0f;
        evidenceGlowLight.intensity = Mathf.MoveTowards(
            evidenceGlowLight.intensity,
            targetIntensity,
            evidenceGlowTransitionSpeed * Time.unscaledDeltaTime
        );

        if (!evidenceGlowRequested && evidenceGlowLight.intensity <= 0f)
            evidenceGlowLight.enabled = false;
    }

    private void OnEvidencePhotographed(EvidencePhotoTarget target)
    {
        if (completed || target == null || chunkManager == null ||
            chunkManager.CurrentChunkID != firstChunkID)
            return;

        completed = true;
        if (photoCamera != null)
            photoCamera.EvidencePhotographed -= OnEvidencePhotographed;
        if (successCoroutine != null)
            StopCoroutine(successCoroutine);

        successCoroutine = StartCoroutine(ShowSuccessMessage());
    }

    private IEnumerator ShowSuccessMessage()
    {
        SetEvidenceGlow(false);

        SetMessage(
            "FOTO REGISTRADA",
            "A imagem foi adicionada às evidências. Ela ajuda a comprovar a contaminação da reserva.",
            string.Empty
        );
        ShowPanel();
        yield return new WaitForSecondsRealtime(successMessageDuration);
        yield return FadePanel(0f);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void FinishTutorial()
    {
        completed = true;
        SetEvidenceGlow(false);
        HidePanel();
    }

    private void SetMessage(string title, string instruction, string controls)
    {
        if (titleText != null)
            titleText.text = title;
        if (instructionText != null)
            instructionText.text = instruction;
        if (controlsText != null)
            controlsText.text = controls;
    }

    private void ShowPanel()
    {
        if (tutorialPanel == null)
            return;

        tutorialPanel.SetActive(true);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePanel(1f));
    }

    private void HidePanel()
    {
        if (tutorialPanel == null)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(HidePanelAfterFade());
    }

    private IEnumerator HidePanelAfterFade()
    {
        yield return FadePanel(0f);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private IEnumerator FadePanel(float targetAlpha)
    {
        if (tutorialGroup == null)
            yield break;

        float startAlpha = tutorialGroup.alpha;
        if (fadeDuration <= 0f)
        {
            tutorialGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            tutorialGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        tutorialGroup.alpha = targetAlpha;
        fadeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (photoCamera != null)
            photoCamera.EvidencePhotographed -= OnEvidencePhotographed;
    }
}
