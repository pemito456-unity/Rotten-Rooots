using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Input do jogador")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayActionMapName = "Player";
    [SerializeField] private string pauseActionName = "Pause";

    [Header("Cena do menu principal")]
    [SerializeField] private string menuSceneName = "Menu";

    private readonly List<InputAction> disabledGameplayActions =
        new List<InputAction>();

    private bool isPaused;
    private float timeScaleBeforePause = 1f;
    private GameObject selectionBeforePause;
    private PhotoEvidenceGallery photoEvidenceGallery;

    private void Awake()
    {
        // Evita carregar a cena já pausada.
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInput>();

        photoEvidenceGallery = GetComponent<PhotoEvidenceGallery>();
        if (photoEvidenceGallery == null)
            photoEvidenceGallery = gameObject.AddComponent<PhotoEvidenceGallery>();

        photoEvidenceGallery.Initialize(pausePanel);
    }

    // Ligue este método ao evento da ação Pause no componente Player Input.
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (isPaused && photoEvidenceGallery != null && photoEvidenceGallery.IsOpen)
        {
            photoEvidenceGallery.CloseGallery();
            return;
        }

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        if (pausePanel == null)
        {
            Debug.LogError("PauseMenuController: atribua o Pause Panel no Inspector.");
            return;
        }

        isPaused = true;
        timeScaleBeforePause = Time.timeScale;

        if (EventSystem.current != null)
            selectionBeforePause = EventSystem.current.currentSelectedGameObject;

        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        DisableGameplayActions();
        SetSelectedButton(firstSelectedButton);
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        if (photoEvidenceGallery != null && photoEvidenceGallery.IsOpen)
            photoEvidenceGallery.CloseGalleryWithoutChangingSelection();

        pausePanel.SetActive(false);
        Time.timeScale = timeScaleBeforePause;

        RestoreGameplayActions();
        isPaused = false;

        GameObject selectionToRestore =
            selectionBeforePause != null && selectionBeforePause.activeInHierarchy
                ? selectionBeforePause
                : null;

        SetSelectedButton(selectionToRestore);
        selectionBeforePause = null;
    }

    public void ReturnToMenu()
    {
        // Time.timeScale persiste entre cenas; redefina antes de carregar o menu.
        if (photoEvidenceGallery != null && photoEvidenceGallery.IsOpen)
            photoEvidenceGallery.CloseGalleryWithoutChangingSelection();

        PhotoEvidenceGallery.ResetProgress();

        Time.timeScale = 1f;
        isPaused = false;

        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("PauseMenuController: informe o nome da cena do menu.");
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

    private void DisableGameplayActions()
    {
        disabledGameplayActions.Clear();

        if (playerInput == null)
        {
            Debug.LogWarning(
                "PauseMenuController: Player Input não foi encontrado. " +
                "A pausa funcionará, mas as ações de gameplay não serão bloqueadas."
            );
            return;
        }

        InputActionMap gameplayMap =
            playerInput.actions.FindActionMap(gameplayActionMapName, false);

        if (gameplayMap == null)
        {
            Debug.LogError(
                $"PauseMenuController: mapa '{gameplayActionMapName}' não encontrado."
            );
            return;
        }

        foreach (InputAction action in gameplayMap.actions)
        {
            // Mantém Pause ativa para Esc/Start também poderem continuar o jogo.
            if (action.name == pauseActionName || !action.enabled)
                continue;

            action.Disable();
            disabledGameplayActions.Add(action);
        }
    }

    private void RestoreGameplayActions()
    {
        foreach (InputAction action in disabledGameplayActions)
        {
            if (action != null)
                action.Enable();
        }

        disabledGameplayActions.Clear();
    }

    private void SetSelectedButton(GameObject button)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);

        if (button != null && button.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(button);
    }

    private void OnDestroy()
    {
        // Recuperação de segurança se a cena for descarregada enquanto pausada.
        if (isPaused)
        {
            Time.timeScale = 1f;
            RestoreGameplayActions();
        }
    }
}
