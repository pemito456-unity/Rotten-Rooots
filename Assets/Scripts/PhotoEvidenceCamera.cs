using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Captures a unique evidence target in front of the player and plays the photo feedback.
/// The gallery images are supplied on EvidencePhotoTarget components by the art team.
/// </summary>
public class PhotoEvidenceCamera : MonoBehaviour
{
    [Header("Enquadramento")]
    [Min(0.1f)] public float maxPhotoDistance = 5f;
    [Range(1f, 120f)] public float fieldOfViewDegrees = 35f;
    [Min(0f)] public float photoCooldown = 0.5f;
    public LayerMask obstructionLayers = Physics2D.DefaultRaycastLayers;

    [Header("Feedback")]
    public AudioClip shutterSound;
    public AudioMixerGroup shutterMixerGroup;
    [Range(0f, 1f)] public float shutterVolume = 0.8f;
    [Min(0.05f)] public float edgeFlashDuration = 0.25f;
    [Range(0f, 1f)] public float edgeFlashAlpha = 0.9f;

    private PlayerController2D playerController;
    private AudioSource audioSource;
    private AudioClip generatedShutterSound;
    private Image edgeFlashImage;
    private Sprite edgeFlashSprite;
    private Texture2D edgeFlashTexture;
    private Coroutine edgeFlashCoroutine;
    private float nextPhotoTime;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = shutterMixerGroup;

        if (shutterSound == null)
        {
            generatedShutterSound = CreateShutterSound();
            shutterSound = generatedShutterSound;
        }

        CreateEdgeFlash();
    }

    public void TakePhoto()
    {
        if (Time.unscaledTime < nextPhotoTime)
            return;

        nextPhotoTime = Time.unscaledTime + photoCooldown;
        PlayShutterSound();
        TriggerEdgeFlash();

        EvidencePhotoTarget target = FindTargetInFrame();
        PhotoEvidenceGallery gallery = PhotoEvidenceGallery.Instance;

        if (target == null)
        {
            ShowNotice(
                (Vector2)transform.position + GetFacingDirection() * 1.5f,
                "Nenhuma evidência no enquadramento"
            );
            return;
        }

        if (gallery == null)
        {
            Debug.LogWarning("PhotoEvidenceGallery não foi inicializada na cena.");
            ShowNotice(target.PhotoPosition, "Evidência fotografada");
            return;
        }

        bool wasNewEvidence = gallery.TryUnlockEvidence(target);
        if (wasNewEvidence)
        {
            ShowNotice(
                target.PhotoPosition,
                $"{gallery.CollectedCount} de {gallery.EvidenceSlotCount} evidências coletadas"
            );
        }
        else
        {
            ShowNotice(target.PhotoPosition, "Evidência já coletada");
        }
    }

    private EvidencePhotoTarget FindTargetInFrame()
    {
        Vector2 origin = transform.position;
        Vector2 facing = GetFacingDirection();
        float halfFov = fieldOfViewDegrees * 0.5f;
        EvidencePhotoTarget closestTarget = null;
        float closestDistance = float.MaxValue;

        EvidencePhotoTarget[] targets = FindObjectsOfType<EvidencePhotoTarget>();
        foreach (EvidencePhotoTarget target in targets)
        {
            if (target == null || !target.gameObject.activeInHierarchy ||
                !target.IsSupportedCategory)
                continue;

            Vector2 targetPosition = target.PhotoPosition;
            Vector2 toTarget = targetPosition - origin;
            float distance = toTarget.magnitude;

            if (distance <= 0.01f || distance > maxPhotoDistance)
                continue;

            float angle = Vector2.Angle(facing, toTarget / distance);
            if (angle > halfFov)
                continue;

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                toTarget / distance,
                distance,
                obstructionLayers
            );

            if (hit.collider != null &&
                hit.distance < distance - 0.08f &&
                hit.collider.GetComponentInParent<EvidencePhotoTarget>() != target)
            {
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    private Vector2 GetFacingDirection()
    {
        if (playerController != null &&
            playerController.lastFacingDirection.sqrMagnitude > 0.01f)
        {
            return playerController.lastFacingDirection.normalized;
        }

        return Vector2.right;
    }

    private void ShowNotice(Vector2 worldPosition, string message)
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.ShowPhotoEvidenceNotice(worldPosition, message);
    }

    private void PlayShutterSound()
    {
        if (audioSource != null && shutterSound != null)
            audioSource.PlayOneShot(shutterSound, shutterVolume);
    }

    private void CreateEdgeFlash()
    {
        GameObject canvasObject = new GameObject(
            "PhotoCameraFlashCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10001;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject(
            "WhitePhotoFlashEdges",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        edgeFlashImage = imageObject.GetComponent<Image>();
        edgeFlashImage.sprite = CreateEdgeGradientSprite();
        edgeFlashImage.color = new Color(1f, 1f, 1f, 0f);
        edgeFlashImage.raycastTarget = false;
    }

    private Sprite CreateEdgeGradientSprite()
    {
        const int size = 128;
        const float fadeWidth = 0.22f;
        edgeFlashTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        edgeFlashTexture.name = "GeneratedPhotoFlashEdges";
        edgeFlashTexture.wrapMode = TextureWrapMode.Clamp;
        edgeFlashTexture.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            float verticalEdgeDistance = Mathf.Min(y, size - 1 - y) / (size * 0.5f);
            for (int x = 0; x < size; x++)
            {
                float horizontalEdgeDistance = Mathf.Min(x, size - 1 - x) / (size * 0.5f);
                float edgeDistance = Mathf.Min(horizontalEdgeDistance, verticalEdgeDistance);
                float fade = Mathf.Clamp01(edgeDistance / fadeWidth);
                fade = fade * fade * (3f - 2f * fade);
                byte alpha = (byte)Mathf.RoundToInt((1f - fade) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        edgeFlashTexture.SetPixels32(pixels);
        edgeFlashTexture.Apply(false, true);

        edgeFlashSprite = Sprite.Create(
            edgeFlashTexture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
        edgeFlashSprite.name = "GeneratedPhotoFlashEdgesSprite";
        return edgeFlashSprite;
    }

    private void TriggerEdgeFlash()
    {
        if (edgeFlashImage == null)
            return;

        if (edgeFlashCoroutine != null)
            StopCoroutine(edgeFlashCoroutine);

        edgeFlashCoroutine = StartCoroutine(EdgeFlashRoutine());
    }

    private IEnumerator EdgeFlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < edgeFlashDuration && edgeFlashImage != null)
        {
            float t = Mathf.Clamp01(elapsed / edgeFlashDuration);
            float alpha = Mathf.Lerp(edgeFlashAlpha, 0f, t);
            edgeFlashImage.color = new Color(1f, 1f, 1f, alpha);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (edgeFlashImage != null)
            edgeFlashImage.color = new Color(1f, 1f, 1f, 0f);

        edgeFlashCoroutine = null;
    }

    private AudioClip CreateShutterSound()
    {
        const int sampleRate = 44100;
        const float duration = 0.14f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        uint seed = 9137u;

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float envelope = Mathf.Exp(-time * 34f);
            seed = seed * 1664525u + 1013904223u;
            float noise = ((seed >> 8) / 16777215f) * 2f - 1f;
            float click = Mathf.Sin(2f * Mathf.PI * 1150f * time) * Mathf.Exp(-time * 75f);
            samples[i] = (noise * 0.17f + click * 0.42f) * envelope;
        }

        AudioClip clip = AudioClip.Create("GeneratedCameraShutter", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (edgeFlashSprite != null)
            Destroy(edgeFlashSprite);
        if (edgeFlashTexture != null)
            Destroy(edgeFlashTexture);
        if (generatedShutterSound != null)
            Destroy(generatedShutterSound);
    }
}
