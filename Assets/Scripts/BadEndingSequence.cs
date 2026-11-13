using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Exibe o final ruim como falas digitadas sobre uma tela preta.</summary>
public class BadEndingSequence : MonoBehaviour
{
    [Serializable]
    private class EndingBeat
    {
        [TextArea(3, 8)] public string text;
        public bool narration;
        public bool showSmokeDuringTyping;
        [Min(0f)] public float pauseAfter = 1.5f;
        public bool playFireCueAfter;
    }

    [Header("Texto")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, Min(1f)] private float charactersPerSecond = 28f;
    [SerializeField] private EndingBeat[] endingBeats =
    {
        new EndingBeat
        {
            text = "Jornalista: Não... Não, não, não! O setor C-1... era logo ali! Por que o caminho está bloqueado? Essa vegetação... ela cresceu de novo, mais rápido do que deveria! Parece que está... me bloqueando de propósito.",
            pauseAfter = 1.6f
        },
        new EndingBeat
        {
            text = "Ele olha para o céu, onde a fumaça incandescente do napalm reflete nas copas das árvores como um sol artificial de fogo. O calor começa a ficar insuportável.",
            narration = true,
            showSmokeDuringTyping = true,
            pauseAfter = 1.2f
        },
        new EndingBeat
        {
            text = "Jornalista: Eu consegui... Os relatórios estão aqui. As fotos da mutação, os crimes da Poison Tech... está tudo gravado. O mundo finalmente ia saber o que eles fizeram...",
            pauseAfter = 1.6f
        },
        new EndingBeat
        {
            playFireCueAfter = true
        },
        new EndingBeat
        {
            text = "Jornalista: Então é isso. Nenhuma saída. O plano deles funcionou perfeitamente: a floresta vai queimar, as provas vão virar cinzas... e eu também. Vão dizer que fui apenas mais um aventureiro imprudente, uma perda colateral.",
            pauseAfter = 0f
        }
    };

    [Header("Áudios")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioSource impactAudioSource;
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup ambienceMixerGroup;
    [SerializeField, Range(0f, 1f)] private float typingVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float impactVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float fireVolume = 0.8f;
    [Tooltip("O fogo toca sozinho por este tempo antes da última fala começar; depois continua ao fundo.")]
    [SerializeField, Min(0f)] private float fireSoundLeadInSeconds = 5f;

    [Header("Depois do final")]
    [SerializeField, Min(0f)] private float silenceBeforeMenu = 2f;
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Fumaça durante a narração")]
    [SerializeField] private Image[] smokePuffs;
    [SerializeField] private Color smokeColor = new Color(0.84f, 0.85f, 0.86f, 1f);
    [SerializeField, Range(0f, 0.7f)] private float smokeOpacity = 0.36f;
    [SerializeField, Range(0f, 1f)] private float smokeDispersalStartsAt = 0.72f;

    private bool sequenceStarted;
    private Vector2[] smokeStartPositions;
    private Vector3[] smokeStartScales;
    private Sprite smokeSprite;
    private Texture2D smokeTexture;

    private void Awake()
    {
        ConfigureAudio();
        ConfigureSmoke();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (dialogueText == null)
        {
            Debug.LogError("BadEndingSequence: atribua o texto da cena no Inspector.", this);
            return;
        }

        StartCoroutine(PlayEnding());
    }

    private void ConfigureAudio()
    {
        ConfigureSource(typingAudioSource, typingVolume, sfxMixerGroup);
        ConfigureSource(impactAudioSource, impactVolume, sfxMixerGroup);
        ConfigureSource(fireAudioSource, fireVolume, ambienceMixerGroup);

        if (typingAudioSource != null)
        {
            typingAudioSource.clip = typingSound;
            typingAudioSource.loop = true;
        }

        if (impactAudioSource != null)
            impactAudioSource.clip = impactSound;

        if (fireAudioSource != null)
        {
            fireAudioSource.clip = fireSound;
            fireAudioSource.loop = false;
        }
    }

    private void ConfigureSmoke()
    {
        if (smokePuffs == null || smokePuffs.Length == 0)
            return;

        const int textureSize = 64;
        smokeTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "BadEndingSoftSmoke",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[textureSize * textureSize];
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.5f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float fade = Mathf.Clamp01(1f - distance);
                float alpha = fade * fade * (3f - 2f * fade);
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        smokeTexture.SetPixels(pixels);
        smokeTexture.Apply(false, true);
        smokeSprite = Sprite.Create(smokeTexture,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f), 100f);

        smokeStartPositions = new Vector2[smokePuffs.Length];
        smokeStartScales = new Vector3[smokePuffs.Length];
        for (int i = 0; i < smokePuffs.Length; i++)
        {
            Image puff = smokePuffs[i];
            if (puff == null)
                continue;

            RectTransform puffTransform = puff.rectTransform;
            smokeStartPositions[i] = puffTransform.anchoredPosition;
            smokeStartScales[i] = puffTransform.localScale;
            puff.sprite = smokeSprite;
            puff.preserveAspect = true;
            puff.raycastTarget = false;
            puff.color = new Color(smokeColor.r, smokeColor.g, smokeColor.b, 0f);
        }
    }

    private void UpdateSmoke(float progress)
    {
        if (smokePuffs == null || smokeStartPositions == null)
            return;

        float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.18f));
        float dispersalStart = Mathf.Clamp(smokeDispersalStartsAt, 0f, 0.999f);
        float dispersal = Mathf.SmoothStep(dispersalStart, 1f, progress);
        RectTransform smokeRoot = smokePuffs.Length > 0 && smokePuffs[0] != null
            ? smokePuffs[0].rectTransform.parent as RectTransform
            : null;
        float canvasHalfWidth = smokeRoot != null ? smokeRoot.rect.width * 0.5f : Screen.width * 0.5f;

        for (int i = 0; i < smokePuffs.Length; i++)
        {
            Image puff = smokePuffs[i];
            if (puff == null)
                continue;

            Vector2 start = smokeStartPositions[i];
            float side = Mathf.Abs(start.x) > 1f ? Mathf.Sign(start.x) : (i % 2 == 0 ? -1f : 1f);
            float puffHalfWidth = puff.rectTransform.rect.width * puff.rectTransform.localScale.x * 0.5f;
            float edgeX = side * (canvasHalfWidth + puffHalfWidth);
            Vector2 edgePosition = new Vector2(edgeX, start.y + Mathf.Sin(i * 1.7f) * 130f);
            puff.rectTransform.anchoredPosition = Vector2.Lerp(start, edgePosition, dispersal);
            puff.rectTransform.localScale = smokeStartScales[i] * Mathf.Lerp(1f, 1.35f, dispersal);

            float alpha = smokeOpacity * fadeIn * (1f - dispersal);
            puff.color = new Color(smokeColor.r, smokeColor.g, smokeColor.b, alpha);
        }
    }

    private static void ConfigureSource(AudioSource source, float volume, AudioMixerGroup mixerGroup)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        if (mixerGroup != null)
            source.outputAudioMixerGroup = mixerGroup;
    }

    private IEnumerator PlayEnding()
    {
        if (sequenceStarted)
            yield break;

        sequenceStarted = true;
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;

        foreach (EndingBeat beat in endingBeats)
        {
            if (beat == null)
                continue;

            yield return TypeBeat(beat);

            if (beat.playFireCueAfter)
                yield return PlayFireCue();

            if (beat.pauseAfter > 0f)
                yield return new WaitForSecondsRealtime(beat.pauseAfter);
        }

        StopAllEndingAudio();

        if (silenceBeforeMenu > 0f)
            yield return new WaitForSecondsRealtime(silenceBeforeMenu);

        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("BadEndingSequence: configure o nome da cena do menu.", this);
            yield break;
        }

        SceneManager.LoadSceneAsync(menuSceneName);
    }

    private IEnumerator TypeBeat(EndingBeat beat)
    {
        dialogueText.fontStyle = beat.narration ? FontStyles.Italic : FontStyles.Normal;
        dialogueText.text = beat.text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int characterCount = dialogueText.textInfo.characterCount;
        if (beat.showSmokeDuringTyping)
            UpdateSmoke(0f);

        if (characterCount > 0 && typingAudioSource != null && typingSound != null)
            typingAudioSource.Play();

        float secondsPerCharacter = 1f / Mathf.Max(1f, charactersPerSecond);
        for (int character = 0; character < characterCount; character++)
        {
            dialogueText.maxVisibleCharacters = character + 1;
            if (beat.showSmokeDuringTyping)
                UpdateSmoke((character + 1f) / characterCount);
            yield return new WaitForSecondsRealtime(secondsPerCharacter);
        }

        dialogueText.maxVisibleCharacters = characterCount;
        if (beat.showSmokeDuringTyping)
            UpdateSmoke(1f);
        if (typingAudioSource != null)
            typingAudioSource.Stop();
    }

    private IEnumerator PlayFireCue()
    {
        if (impactAudioSource != null && impactSound != null)
        {
            impactAudioSource.PlayOneShot(impactSound);
            yield return new WaitForSecondsRealtime(impactSound.length);
        }

        if (fireAudioSource != null && fireSound != null)
        {
            fireAudioSource.Play();
            if (fireSoundLeadInSeconds > 0f)
                yield return new WaitForSecondsRealtime(fireSoundLeadInSeconds);
        }
    }

    private void StopAllEndingAudio()
    {
        if (typingAudioSource != null)
            typingAudioSource.Stop();
        if (impactAudioSource != null)
            impactAudioSource.Stop();
        if (fireAudioSource != null)
            fireAudioSource.Stop();
    }

    private void OnDestroy()
    {
        if (smokeSprite != null)
            Destroy(smokeSprite);
        if (smokeTexture != null)
            Destroy(smokeTexture);
    }
}
