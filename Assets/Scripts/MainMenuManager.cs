using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuração da Cena Inicial")]
    public string firstChunkSceneName = "J1_PrimeiraChunk";

    [Header("Painéis do Menu")]
    public GameObject controlsPanel;
    public GameObject defaultMenuSelection;
    public GameObject controlsPanelSelection;
    public GameObject settingsButton;
    public GameObject settingsPanel;
    public Button settingsBackButton;

    [Header("Sliders de volume")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Configurações de áudio")]
    public AudioMixer audioMixer;
    public string masterVolumeParameter = "MasterVolume";
    public string musicVolumeParameter = "MusicVolume";
    public string sfxVolumeParameter = "SFXVolume";

    private GameObject previousMenuSelection;

    private const string MasterPreferenceKey = "RottenRoots.MasterVolume";
    private const string MusicPreferenceKey = "RottenRoots.MusicVolume";
    private const string SfxPreferenceKey = "RottenRoots.SFXVolume";

    private void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        ConfigureSettingsPanel();

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

    public void OpenControls()
    {
        if (EventSystem.current != null)
            previousMenuSelection = EventSystem.current.currentSelectedGameObject;

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        SetSelectedMenuObject(controlsPanelSelection);
    }

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

    public void OpenSettings()
    {
        if (EventSystem.current != null)
            previousMenuSelection = EventSystem.current.currentSelectedGameObject;

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        SetSelectedMenuObject(settingsBackButton != null ? settingsBackButton.gameObject : null);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        PlayerPrefs.Save();

        GameObject selectionToRestore = previousMenuSelection != null &&
            previousMenuSelection.activeInHierarchy
            ? previousMenuSelection
            : settingsButton;

        SetSelectedMenuObject(selectionToRestore);
        previousMenuSelection = null;
    }

    private void ConfigureSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("MainMenuManager: atribua o painel SettingsPanel no Inspector.");
            return;
        }

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);

        ConfigureSlider(masterVolumeSlider, MasterPreferenceKey, masterVolumeParameter);
        ConfigureSlider(musicVolumeSlider, MusicPreferenceKey, musicVolumeParameter);
        ConfigureSlider(sfxVolumeSlider, SfxPreferenceKey, sfxVolumeParameter);
    }

    private void ConfigureSlider(Slider slider, string preferenceKey, string mixerParameter)
    {
        float savedValue = Mathf.Clamp01(PlayerPrefs.GetFloat(preferenceKey, 1f));

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = savedValue;
            slider.onValueChanged.AddListener(value => SetVolume(preferenceKey, mixerParameter, value));
        }

        SetVolume(preferenceKey, mixerParameter, savedValue);
    }

    private void SetVolume(string preferenceKey, string mixerParameter, float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(preferenceKey, value);

        if (audioMixer != null && !string.IsNullOrWhiteSpace(mixerParameter))
        {
            float decibels = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(mixerParameter, decibels);
        }
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
