using System.Collections;
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

    [Header("Cena do menu")]
    [SerializeField] private string menuSceneName = "Menu";

    private Animator playerAnimator;
    private PlayerHealth playerHealth;
    private PlayerController2D playerController;
    private PlayerInput playerInput;
    private Rigidbody2D playerRigidbody;
    private bool gameOverStarted;

    private void Awake()
    {
        Instance = this;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

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
        if (gameOverStarted)
            return;

        gameOverStarted = true;
        Time.timeScale = 1f;

        if (playerHealth != null)
            playerHealth.PrepareForGameOver();

        if (playerRigidbody != null)
            playerRigidbody.linearVelocity = Vector2.zero;

        if (playerController != null)
            playerController.enabled = false;

        if (playerInput != null)
            playerInput.enabled = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsDead", true);

        StartCoroutine(ShowGameOverAfterDeathAnimation());
    }

    private IEnumerator ShowGameOverAfterDeathAnimation()
    {
        if (playerAnimator == null)
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

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (firstSelectedButton != null && firstSelectedButton.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
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
