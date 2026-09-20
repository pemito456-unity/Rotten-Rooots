using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuração da Cena Inicial")]
    public string firstChunkSceneName = "J1_PrimeiraChunk";

    [Header("Painéis do Menu")]
    public GameObject controlsPanel; // Arraste o painel aqui no Inspector

    private void Start()
    {
        // Garante que o painel de controles comece escondido
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        Debug.Log("Iniciando o jogo...");
        SceneManager.LoadScene(firstChunkSceneName);
    }

    // Função para abrir a tela de controles
    public void OpenControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    // Função para fechar a tela de controles
    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Fechando o jogo...");
        
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}