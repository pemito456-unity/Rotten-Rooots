using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>Exibe o final bom como falas digitadas sobre uma tela preta.</summary>
public class GoodEndingSequence : MonoBehaviour
{
    [Serializable]
    private class EndingLine
    {
        [TextArea(3, 8)] public string text;
        [Min(0f)] public float pauseAfter = 1.5f;
    }

    [Header("Texto")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, Min(1f)] private float charactersPerSecond = 28f;
    [SerializeField] private EndingLine[] endingLines =
    {
        new EndingLine
        {
            text = "Jornalista: Eu consegui… Argh! Eu saí! Eu saí daquele inferno!",
            pauseAfter = 1.6f
        },
        new EndingLine
        {
            text = "Jornalista: C-1 ao norte… Aquele coitado não mentiu no relatório. Se não fosse por ele, eu teria virado cinzas junto com o resto das vítimas da Poison Tech.",
            pauseAfter = 1.8f
        },
        new EndingLine
        {
            text = "Jornalista: Podem jogar todo o napalm do mundo, seus desgraçados… Não vai adiantar nada! Podem queimar a floresta, podem encobrir os corpos, mas não vão apagar isso aqui!",
            pauseAfter = 1.8f
        },
        new EndingLine
        {
            text = "Jornalista: Eles achavam que tudo viraria “perda colateral”. Acharam que podiam matar a flora, matar a própria equipe de segurança e fingir que nada aconteceu. Que os monstros eram as criaturas lá dentro… Quando os verdadeiros monstros vestem terno e gravata.",
            pauseAfter = 2f
        },
        new EndingLine
        {
            text = "Jornalista: Está tudo documentado. As fotos das mutações, os corpos, os relatórios confidenciais… O mundo inteiro vai saber o que a Poison Tech fez! Não sou mais um corpo naquela terra. Não virei parte da flora maldita deles. Eu sobrevivi.",
            pauseAfter = 0f
        }
    };

    [Header("Áudio de digitação")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField, Range(0f, 1f)] private float typingVolume = 0.65f;

    [Header("Depois do final")]
    [SerializeField, Min(0f)] private float silenceBeforeMenu = 2f;
    [SerializeField] private string menuSceneName = "Menu";

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
            Debug.LogError("GoodEndingSequence: atribua o texto da cena no Inspector.", this);
            return;
        }

        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        if (sequenceStarted)
            yield break;

        sequenceStarted = true;
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;

        foreach (EndingLine line in endingLines)
        {
            if (line == null)
                continue;

            yield return TypeLine(line.text);

            if (line.pauseAfter > 0f)
                yield return new WaitForSecondsRealtime(line.pauseAfter);
        }

        StopTypingAudio();

        if (silenceBeforeMenu > 0f)
            yield return new WaitForSecondsRealtime(silenceBeforeMenu);

        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("GoodEndingSequence: configure o nome da cena do menu.", this);
            yield break;
        }

        SceneManager.LoadSceneAsync(menuSceneName);
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.fontStyle = FontStyles.Normal;
        dialogueText.text = line;
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
