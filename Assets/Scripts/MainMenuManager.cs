using System.Collections.Generic;
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

    [Header("Trilha sonora do menu")]
    public AudioClip[] menuMusicTracks;
    public AudioSource menuMusicSource;
    public AudioMixerGroup musicMixerGroup;

    private GameObject previousMenuSelection;
    private readonly List<AudioClip> menuPlaylist = new List<AudioClip>();
    private int nextMenuTrackIndex;
    private AudioClip lastMenuTrack;

    private const string MasterPreferenceKey = "RottenRoots.MasterVolume";
    private const string MusicPreferenceKey = "RottenRoots.MusicVolume";
    private const string SfxPreferenceKey = "RottenRoots.SFXVolume";
    private const string VolumeDefaultsVersionKey = "RottenRoots.VolumeDefaults50Applied";

    private void Start()
    {
        ApplyInitialVolumeDefaults();

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        ConfigureSettingsPanel();
        SetupMenuMusic();

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null &&
            defaultMenuSelection != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultMenuSelection);
        }
    }

    private void Update()
    {
        ContinueMenuMusic();
    }

    private void SetupMenuMusic()
    {
        if (menuMusicSource == null)
        {
            Debug.LogWarning("MainMenuManager: atribua o AudioSource da música do menu.");
            return;
        }

        menuMusicSource.playOnAwake = false;
        menuMusicSource.loop = false;
        menuMusicSource.spatialBlend = 0f;
        menuMusicSource.volume = 1f;
        menuMusicSource.outputAudioMixerGroup = musicMixerGroup;

        menuPlaylist.Clear();
        if (menuMusicTracks != null)
        {
            foreach (AudioClip track in menuMusicTracks)
            {
                if (track != null)
                    menuPlaylist.Add(track);
            }
        }

        ShuffleMenuPlaylist();
        PlayNextMenuTrack();
    }

    private void ContinueMenuMusic()
    {
        if (menuMusicSource == null || menuPlaylist.Count == 0 || menuMusicSource.isPlaying)
            return;

        if (nextMenuTrackIndex >= menuPlaylist.Count)
        {
            ShuffleMenuPlaylist();
            nextMenuTrackIndex = 0;
        }

        PlayNextMenuTrack();
    }

    private void ShuffleMenuPlaylist()
    {
        for (int i = menuPlaylist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temporary = menuPlaylist[i];
            menuPlaylist[i] = menuPlaylist[randomIndex];
            menuPlaylist[randomIndex] = temporary;
        }

        if (menuPlaylist.Count > 1 && menuPlaylist[0] == lastMenuTrack)
        {
            AudioClip temporary = menuPlaylist[0];
            menuPlaylist[0] = menuPlaylist[1];
            menuPlaylist[1] = temporary;
        }
    }

    private void PlayNextMenuTrack()
    {
        if (menuPlaylist.Count == 0)
        {
            Debug.LogWarning("MainMenuManager: nenhuma faixa de menu foi configurada.");
            return;
        }

        if (nextMenuTrackIndex >= menuPlaylist.Count)
            nextMenuTrackIndex = 0;

        lastMenuTrack = menuPlaylist[nextMenuTrackIndex++];
        menuMusicSource.clip = lastMenuTrack;
        menuMusicSource.Play();
    }

    private void ApplyInitialVolumeDefaults()
    {
        // Aplica 50% uma vez, inclusive em instalações que já tinham valores antigos salvos.
        if (PlayerPrefs.GetInt(VolumeDefaultsVersionKey, 0) != 0)
            return;

        PlayerPrefs.SetFloat(MasterPreferenceKey, 0.5f);
        PlayerPrefs.SetFloat(MusicPreferenceKey, 0.5f);
        PlayerPrefs.SetFloat(SfxPreferenceKey, 0.5f);
        PlayerPrefs.SetInt(VolumeDefaultsVersionKey, 1);
        PlayerPrefs.Save();
    }

    public void PlayGame()
    {
        Debug.Log("Iniciando o jogo...");
        ForestWrathManager.ResetForNewRun();
        PhotoEvidenceGallery.ResetProgress();
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

        GameObject selectionToRestore = defaultMenuSelection;
        if (previousMenuSelection != null && previousMenuSelection.activeInHierarchy)
            selectionToRestore = previousMenuSelection;

        SetSelectedMenuObject(selectionToRestore);
        previousMenuSelection = null;
    }

    public void OpenSettings()
    {
        if (EventSystem.current != null)
            previousMenuSelection = EventSystem.current.currentSelectedGameObject;

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        GameObject firstSettingsSelection = null;
        if (settingsBackButton != null)
            firstSettingsSelection = settingsBackButton.gameObject;

        SetSelectedMenuObject(firstSettingsSelection);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        PlayerPrefs.Save();

        GameObject selectionToRestore = settingsButton;
        if (previousMenuSelection != null && previousMenuSelection.activeInHierarchy)
            selectionToRestore = previousMenuSelection;

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
        // Começa em 50% na primeira execução; valores salvos pelo jogador continuam valendo.
        float savedValue = Mathf.Clamp01(PlayerPrefs.GetFloat(preferenceKey, 0.5f));

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
            float decibels = -80f;
            if (value > 0.0001f)
                decibels = Mathf.Log10(value) * 20f;

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
