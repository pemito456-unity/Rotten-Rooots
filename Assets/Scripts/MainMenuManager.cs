using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuração da Cena Inicial")]
    public string firstChunkSceneName = "J1_PrimeiraChunk";

    // Função chamada pelo botão PLAY
    public void PlayGame()
    {
        Debug.Log("Iniciando o jogo...");
        SceneManager.LoadScene(firstChunkSceneName);
    }

    // Função chamada pelo botão SAIR
    public void QuitGame()
    {
        Debug.Log("Fechando o jogo...");
        
        Application.Quit(); // Funciona no jogo compilado (.exe)

        // Permite fechar a execução também durante os testes na Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}