using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    [Header("Tela de Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Animação de morte")]
    [SerializeField] private float deathAnimationFallbackDuration = 1f;
    [SerializeField] private float deathAnimationTimeout = 4f;
    [SerializeField, Min(0f)] private float endingRevealDelay = 0.75f;

    [Header("Cena do menu")]
    [SerializeField] private string menuSceneName = "Menu";

    private Animator playerAnimator;
    private PlayerHealth playerHealth;
    private PlayerController2D playerController;
    private PlayerInput playerInput;
    private Rigidbody2D playerRigidbody;
    private bool gameOverStarted;
    private bool endingUsesDeathAnimation;
    private string endingTitle = "GAME OVER";
    private string endingMessage = "A floresta tomou você.";
    private TMP_Text endingTitleText;
    private TMP_Text endingMessageText;

    public bool IsGameOverStarted => gameOverStarted;

    private void Awake()
    {
        Instance = this;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            FindEndingLabels();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        playerAnimator = player.GetComponent<Animator>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerController = player.GetComponent<PlayerController2D>();
        playerInput = player.GetComponent<PlayerInput>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();
    }

    public void TriggerGameOver()
    {
        BeginGameOver("GAME OVER", "A floresta tomou você.", true, 0f);
    }

    public void TriggerEnding(string title, string message, bool playDeathAnimation, float revealDelay)
    {
        BeginGameOver(title, message, playDeathAnimation, revealDelay);
    }

    private void BeginGameOver(string title, string message, bool playDeathAnimation, float revealDelay)
    {
        if (gameOverStarted)
            return;

        gameOverStarted = true;
        endingTitle = title;
        endingMessage = message;
        endingUsesDeathAnimation = playDeathAnimation;
        endingRevealDelay = Mathf.Max(0f, revealDelay);

        CorpseNotePanelController notePanel = CorpseNotePanelController.Instance;
        if (notePanel != null && notePanel.IsOpen)
            notePanel.Close();

        Time.timeScale = 1f;

        if (playDeathAnimation && playerHealth != null)
            playerHealth.PrepareForGameOver();

        if (playerRigidbody != null)
            playerRigidbody.linearVelocity = Vector2.zero;

        if (playerController != null)
            playerController.enabled = false;

        if (playerInput != null)
            playerInput.enabled = false;

        if (playDeathAnimation && playerAnimator != null)
            playerAnimator.SetBool("IsDead", true);

        StartCoroutine(ShowGameOverAfterDeathAnimation());
    }

    private IEnumerator ShowGameOverAfterDeathAnimation()
    {
        if (!endingUsesDeathAnimation)
        {
            yield return new WaitForSecondsRealtime(endingRevealDelay);
        }
        else if (playerAnimator == null)
        {
            yield return new WaitForSecondsRealtime(deathAnimationFallbackDuration);
        }
        else
        {
            float elapsed = 0f;
            bool deathAnimationFinished = false;

            while (elapsed < deathAnimationTimeout)
            {
                AnimatorStateInfo state = playerAnimator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Death") && state.normalizedTime >= 1f)
                {
                    deathAnimationFinished = true;
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!deathAnimationFinished && elapsed < deathAnimationFallbackDuration)
            {
                yield return new WaitForSecondsRealtime(
                    deathAnimationFallbackDuration - elapsed
                );
            }
        }

        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverController: atribua o Game Over Panel no Inspector.");
            yield break;
        }

        if (endingTitleText != null)
            endingTitleText.text = endingTitle;
        if (endingMessageText != null)
            endingMessageText.text = endingMessage;

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (firstSelectedButton != null && firstSelectedButton.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    private void FindEndingLabels()
    {
        TMP_Text[] labels = gameOverPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text label in labels)
        {
            if (label.name == "GameOverTitle")
                endingTitleText = label;
            else if (label.name == "GameOverMessage")
                endingMessageText = label;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("GameOverController: informe o nome da cena do menu.");
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
