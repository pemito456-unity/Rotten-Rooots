using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Controla o aviso de fones e a transição para o menu.</summary>
public class PreMenuWarningController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup noticeGroup;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField, Min(0f)] private float fadeInDuration = 0.8f;
    [SerializeField, Min(0f)] private float displayDuration = 2.5f;
    [SerializeField, Min(0f)] private float contentFadeOutDuration = 0.6f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.8f;

    private IEnumerator Start()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("PreMenuWarningController: atribua o CanvasGroup no Inspector.", this);
            yield break;
        }

        canvasGroup.alpha = 1f;
        if (noticeGroup != null)
            noticeGroup.alpha = 0f;
        yield return Fade(noticeGroup, 0f, 1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(displayDuration);
        yield return Fade(noticeGroup, 1f, 0f, contentFadeOutDuration);

        AsyncOperation loadMenu = SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
        if (loadMenu == null)
        {
            Debug.LogError($"PreMenuWarningController: não foi possível carregar a cena {menuSceneName}.", this);
            yield break;
        }

        yield return loadMenu;

        Scene menuScene = SceneManager.GetSceneByName(menuSceneName);
        if (!menuScene.IsValid() || !menuScene.isLoaded)
        {
            Debug.LogError($"PreMenuWarningController: a cena {menuSceneName} não foi carregada.", this);
            yield break;
        }

        SceneManager.SetActiveScene(menuScene);
        yield return Fade(canvasGroup, 1f, 0f, fadeOutDuration);
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    private IEnumerator Fade(CanvasGroup target, float startAlpha, float endAlpha, float duration)
    {
        if (target == null)
            yield break;

        if (duration <= 0f)
        {
            target.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            target.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.SmoothStep(0f, 1f, progress));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        target.alpha = endAlpha;
    }
}
