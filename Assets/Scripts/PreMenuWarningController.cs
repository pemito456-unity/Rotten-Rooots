using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Exibe o aviso de fones e faz a transição para o menu principal.
/// A interface é montada na cena; este script controla apenas o fade e o tempo.
/// </summary>
public class PreMenuWarningController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField, Min(0f)] private float fadeInDuration = 0.8f;
    [SerializeField, Min(0f)] private float displayDuration = 2.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.8f;

    private IEnumerator Start()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("PreMenuWarningController: atribua o CanvasGroup no Inspector.", this);
            yield break;
        }

        canvasGroup.alpha = 0f;
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(displayDuration);
        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("PreMenuWarningController: informe o nome da cena do menu.", this);
            yield break;
        }

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
        yield return Fade(1f, 0f, fadeOutDuration);
        yield return SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, easedProgress);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
