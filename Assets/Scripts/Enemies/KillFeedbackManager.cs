using System.Collections;
using UnityEngine;

public class KillFeedbackManager : MonoBehaviour
{
    public static KillFeedbackManager Instance;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip firstKillRoarSound;

    [Header("Configurações de Screen Shake")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.2f;

    private static bool hasTriggeredFirstKill = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerFirstKillFeedback()
    {
        // Garante que o feedback ocorra apenas UMA vez por partida
        if (hasTriggeredFirstKill) return;

        hasTriggeredFirstKill = true;

        // Toca o áudio do rugido
        if (audioSource != null && firstKillRoarSound != null)
        {
            audioSource.PlayOneShot(firstKillRoarSound);
        }

        // Ativa o tremer de tela na Câmera Principal
        if (Camera.main != null)
        {
            StartCoroutine(ScreenShake(shakeDuration, shakeMagnitude));
        }
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }
}