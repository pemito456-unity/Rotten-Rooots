using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>Mostra a introdução da história antes de iniciar a gameplay.</summary>
public class StoryIntroSequence : MonoBehaviour
{
    [Serializable]
    private class IntroParagraph
    {
        [TextArea(3, 10)] public string text;
        [Min(0f)] public float pauseAfter = 1.5f;
    }

    [Header("Texto")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, Min(1f)] private float charactersPerSecond = 28f;
    [SerializeField] private IntroParagraph[] paragraphs =
    {
        new IntroParagraph
        {
            text = "Você, um jornalista, descobre uma área de descarte de lixo tóxico da empresa Poison Tech, a expectativa da diretoria da corporação era simples: matar a flora local, pagar uma multa irrelevante e comprar a terra a preço de banana. Eles só não esperavam que a floresta resistisse. E se adaptasse.",
            pauseAfter = 1.5f
        },
        new IntroParagraph
        {
            text = "Você, Miguel Silva Cortez, se prepara e arruma a alça da câmera no pescoço e verifica o kit de descontaminação no bolso da jaqueta. Como jornalista independente, seu objetivo é direto: penetrar no perímetro, documentar as mutações, recuperar os relatórios confidenciais da Poison Tech e sair vivo.",
            pauseAfter = 0f
        }
    };

    [Header("Áudio de digitação")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField, Range(0f, 1f)] private float typingVolume = 0.65f;

    [Header("Depois da introdução")]
    [SerializeField, Min(0f)] private float silenceBeforeGameplay = 2f;
    [SerializeField] private string gameplaySceneName = "MainGame";

    private bool sequenceStarted;

    private void Awake()
    {
        if (typingAudioSource == null)
            return;

        typingAudioSource.playOnAwake = false;
        typingAudioSource.spatialBlend = 0f;
        typingAudioSource.volume = typingVolume;
        typingAudioSource.clip = typingSound;
        typingAudioSource.loop = true;
        if (sfxMixerGroup != null)
            typingAudioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (dialogueText == null)
        {
            Debug.LogError("StoryIntroSequence: atribua o texto da cena no Inspector.", this);
            return;
        }

        StartCoroutine(PlayIntroduction());
    }

    private IEnumerator PlayIntroduction()
    {
        if (sequenceStarted)
            yield break;

        sequenceStarted = true;
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;

        foreach (IntroParagraph paragraph in paragraphs)
        {
            if (paragraph == null)
                continue;

            yield return TypeParagraph(paragraph.text);

            if (paragraph.pauseAfter > 0f)
                yield return new WaitForSecondsRealtime(paragraph.pauseAfter);
        }

        StopTypingAudio();

        if (silenceBeforeGameplay > 0f)
            yield return new WaitForSecondsRealtime(silenceBeforeGameplay);

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("StoryIntroSequence: configure o nome da cena MainGame.", this);
            yield break;
        }

        SceneManager.LoadSceneAsync(gameplaySceneName);
    }

    private IEnumerator TypeParagraph(string paragraph)
    {
        dialogueText.fontStyle = FontStyles.Normal;
        dialogueText.text = paragraph;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int characterCount = dialogueText.textInfo.characterCount;
        if (characterCount > 0 && typingAudioSource != null && typingSound != null)
            typingAudioSource.Play();

        float secondsPerCharacter = 1f / Mathf.Max(1f, charactersPerSecond);
        for (int character = 0; character < characterCount; character++)
        {
            dialogueText.maxVisibleCharacters = character + 1;
            yield return new WaitForSecondsRealtime(secondsPerCharacter);
        }

        dialogueText.maxVisibleCharacters = characterCount;
        StopTypingAudio();
    }

    private void StopTypingAudio()
    {
        if (typingAudioSource != null)
            typingAudioSource.Stop();
    }
}
