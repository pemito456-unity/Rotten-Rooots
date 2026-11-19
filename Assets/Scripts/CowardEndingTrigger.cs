using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CowardEndingTrigger : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField, Min(0f)] private float fadeDuration = 1.2f;
    [SerializeField] private string endingSceneName = "CowardEnding";

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
            return;

        isLoading = true;
        StopPlayer(other);
        StartCoroutine(LoadEnding());
    }

    private static void StopPlayer(Collider2D playerCollider)
    {
        PlayerController2D controller = playerCollider.GetComponentInParent<PlayerController2D>();
        if (controller != null)
            controller.enabled = false;

        PlayerController2DAndroid androidController = playerCollider.GetComponentInParent<PlayerController2DAndroid>();
        if (androidController != null)
            androidController.enabled = false;

        Rigidbody2D body = playerCollider.attachedRigidbody;
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }
    }

    private IEnumerator LoadEnding()
    {
        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.alpha = 0f;
            fadeGroup.interactable = false;
            fadeGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            fadeGroup.alpha = 1f;
        }

        if (string.IsNullOrWhiteSpace(endingSceneName))
        {
            Debug.LogError("CowardEndingTrigger: configure o nome da cena de final.", this);
            yield break;
        }

        SceneManager.LoadSceneAsync(endingSceneName);
    }
}
