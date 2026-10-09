using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuração da Cena Inicial")]
    public string firstChunkSceneName = "J1_PrimeiraChunk";

    [Header("Painéis do Menu")]
    public GameObject controlsPanel; // Arraste o painel aqui no Inspector
    public GameObject defaultMenuSelection;
    public GameObject controlsPanelSelection;

    private GameObject previousMenuSelection;

    private void Start()
    {
        // Garante que o painel de controles comece escondido
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null &&
            defaultMenuSelection != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultMenuSelection);
        }
    }

    public void PlayGame()
    {
        Debug.Log("Iniciando o jogo...");
        SceneManager.LoadScene(firstChunkSceneName);
    }

    // Função para abrir a tela de controles
    public void OpenControls()
    {
        if (EventSystem.current != null)
            previousMenuSelection = EventSystem.current.currentSelectedGameObject;

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        SetSelectedMenuObject(controlsPanelSelection);
    }

    // Função para fechar a tela de controles
    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        GameObject selectionToRestore = previousMenuSelection != null &&
            previousMenuSelection.activeInHierarchy
            ? previousMenuSelection
            : defaultMenuSelection;

        SetSelectedMenuObject(selectionToRestore);
        previousMenuSelection = null;
    }

    private void SetSelectedMenuObject(GameObject selection)
    {
        if (EventSystem.current != null && selection != null && selection.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(selection);
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
