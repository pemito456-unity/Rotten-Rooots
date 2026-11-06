using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Controla o incêndio final.
/// O fogo começa na coluna A e na fileira 4, depois avança pela coluna B em D2.
/// Neste protótipo, as chamas são apenas visuais.
/// </summary>
public class FinalFireSequence : MonoBehaviour
{
    public static FinalFireSequence Instance { get; private set; }

    [Header("Rota e tempo")]
    [SerializeField, Min(1f)] private float escapeTimeLimit = 120f;
    [SerializeField] private string secondFireStageChunkID = "D2";
    [SerializeField] private string escapeChunkID = "C1";

    [Header("Música da gameplay e do incêndio")]
    [SerializeField] private AudioClip[] gameplayTracks;
    [SerializeField] private AudioClip[] fireTracks;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField, Range(0f, 1f)] private float gameplayMusicVolume = 0.25f;

    private AudioSource musicAudioSource;
    private readonly List<AudioClip> musicPlaylist = new List<AudioClip>();
    private int nextTrackIndex;
    private bool fireMusicStarted;
    private bool musicPausedForGame;
    private AudioClip lastPlayedTrack;

    [Header("Chunks atingidas pelo incêndio")]
    [SerializeField] private string[] initialBurningChunkIDs =
        { "A1", "A2", "A3", "A4", "B4", "C4", "D4" };
    [SerializeField] private string[] secondStageBurningChunkIDs =
        { "B1", "B2", "B3", "B4" };

    [Header("Posições locais das chamas")]
    [Tooltip("Pontos relativos ao objeto raiz de cada chunk. O fogo não causa dano neste protótipo.")]
    [SerializeField] private Vector2[] flameLocalPositions =
    {
        new Vector2(-3f, -1.35f),
        new Vector2(0f, 1.15f),
        new Vector2(3f, -1.15f)
    };
    [SerializeField, Min(0.1f)] private float flameScale = 1.15f;

    [Header("Alerta nas bordas da tela")]
    [SerializeField] private Color warningColor = new Color(1f, 0.32f, 0.025f, 1f);
    [SerializeField, Range(0f, 1f)] private float startingWarningIntensity = 0.22f;
    [SerializeField, Range(0f, 1f)] private float maximumWarningIntensity = 0.68f;
    [SerializeField, Min(0f)] private float startingPulseSpeed = 1.2f;
    [SerializeField, Min(0f)] private float maximumPulseSpeed = 3.8f;
    [SerializeField, Min(1f)] private float urgencyWindowSeconds = 40f;

    [Header("Telas finais")]
    [SerializeField, Min(0f)] private float goodEndingRevealDelay = 0.75f;
    [SerializeField] private string goodEndingTitle = "VOCÊ ESCAPOU";
    [TextArea(2, 4)]
    [SerializeField] private string goodEndingMessage =
        "Você alcançou a rota de fuga em C1 antes que o incêndio tomasse a reserva.";
    [SerializeField] private string badEndingTitle = "FIM DE JOGO";
    [TextArea(2, 4)]
    [SerializeField] private string badEndingMessage =
        "O incêndio consumiu a reserva antes que você alcançasse C1.";

    private readonly HashSet<string> burningChunks = new HashSet<string>();
    private readonly List<FlameVisual> flameVisuals = new List<FlameVisual>();
    private ChunkManager chunkManager;
    private PlayerHealth playerHealth;
    private float elapsedTime;
    private bool sequenceStarted;
    private bool secondStageStarted;
    private bool outcomeTriggered;

    private static Sprite sharedFlameSprite;
    private static Texture2D sharedFlameTexture;

    public bool IsSequenceActive
    {
        get
        {
            return sequenceStarted && !outcomeTriggered;
        }
    }

    public float RemainingTime
    {
        get
        {
            return Mathf.Max(0f, escapeTimeLimit - elapsedTime);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        chunkManager = ChunkManager.Instance;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        SetupMusic();
        StartMusicPlaylist(gameplayTracks);
    }

    /// <summary>Começa quando o jogador abre as anotações do último cadáver.</summary>
    public void BeginFinalSequence()
    {
        if (sequenceStarted || outcomeTriggered)
            return;

        chunkManager = ChunkManager.Instance;
        if (chunkManager == null)
        {
            Debug.LogError("FinalFireSequence: ChunkManager não foi encontrado.", this);
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        sequenceStarted = true;
        elapsedTime = 0f;
        StartFireMusic();

        foreach (string chunkID in initialBurningChunkIDs)
            SetChunkBurning(chunkID);

        UpdateScreenWarning();
        Debug.Log("Incêndio final iniciado. O jogador tem 2 minutos para chegar à chunk C1.", this);
    }

    private void Update()
    {
        if (outcomeTriggered)
            return;

        if (GameOverController.Instance != null && GameOverController.Instance.IsGameOverStarted)
        {
            StopScreenWarning();
            StopMusic();
            outcomeTriggered = true;
            return;
        }

        ContinueMusicPlaylist();

        if (!sequenceStarted)
            return;

        // Usa o tempo de jogo: abrir menus ou as anotações pausa a contagem.
        elapsedTime += Time.deltaTime;

        string currentChunk = null;
        if (chunkManager != null)
            currentChunk = chunkManager.CurrentChunkID;
        if (!secondStageStarted && currentChunk == secondFireStageChunkID)
        {
            secondStageStarted = true;
            foreach (string chunkID in secondStageBurningChunkIDs)
                SetChunkBurning(chunkID);

            Debug.Log("O incêndio avançou para a coluna B.", this);
        }

        UpdateScreenWarning();

        // A condição de derrota vence se o prazo terminar antes da chegada a C1.
        if (elapsedTime >= escapeTimeLimit)
        {
            TriggerEnding(false);
            return;
        }

        if (currentChunk == escapeChunkID)
            TriggerEnding(true);
    }

    private void SetupMusic()
    {
        musicAudioSource = GetComponent<AudioSource>();
        if (musicAudioSource == null)
            musicAudioSource = gameObject.AddComponent<AudioSource>();

        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = false;
        musicAudioSource.spatialBlend = 0f;
        // Mantém a trilha de fundo abaixo dos batimentos cardíacos do PlayerHealth.
        musicAudioSource.volume = gameplayMusicVolume;
        musicAudioSource.outputAudioMixerGroup = musicMixerGroup;
    }

    private void StartFireMusic()
    {
        if (fireMusicStarted)
            return;

        fireMusicStarted = true;
        StartMusicPlaylist(fireTracks);
    }

    private void StartMusicPlaylist(AudioClip[] tracks)
    {
        if (musicAudioSource == null)
            SetupMusic();

        musicAudioSource.Stop();
        musicPlaylist.Clear();
        nextTrackIndex = 0;

        if (tracks != null)
        {
            foreach (AudioClip track in tracks)
            {
                if (track != null)
                    musicPlaylist.Add(track);
            }
        }

        ShufflePlaylist();
        PlayNextTrack();
    }

    private void ContinueMusicPlaylist()
    {
        if (musicAudioSource == null)
            return;

        CorpseNotePanelController notePanel = CorpseNotePanelController.Instance;
        bool readingCorpseNotes = notePanel != null && notePanel.IsOpen;

        if (Time.timeScale <= 0f && !readingCorpseNotes)
        {
            if (!musicPausedForGame)
                musicAudioSource.Pause();

            musicPausedForGame = true;
            return;
        }

        if (musicPausedForGame)
        {
            musicAudioSource.UnPause();
            musicPausedForGame = false;
        }

        if (musicPlaylist.Count == 0 || musicAudioSource.isPlaying)
            return;

        if (nextTrackIndex >= musicPlaylist.Count)
        {
            ShufflePlaylist();
            nextTrackIndex = 0;
        }

        PlayNextTrack();
    }

    private void ShufflePlaylist()
    {
        for (int i = musicPlaylist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temporary = musicPlaylist[i];
            musicPlaylist[i] = musicPlaylist[randomIndex];
            musicPlaylist[randomIndex] = temporary;
        }

        // Evita que a mesma faixa toque duas vezes seguidas ao reiniciar o ciclo.
        if (musicPlaylist.Count > 1 && musicPlaylist[0] == lastPlayedTrack)
        {
            AudioClip temporary = musicPlaylist[0];
            musicPlaylist[0] = musicPlaylist[1];
            musicPlaylist[1] = temporary;
        }
    }

    private void PlayNextTrack()
    {
        if (musicPlaylist.Count == 0)
        {
            Debug.LogWarning("FinalFireSequence: nenhuma faixa de música foi configurada.", this);
            return;
        }

        if (nextTrackIndex >= musicPlaylist.Count)
            nextTrackIndex = 0;

        lastPlayedTrack = musicPlaylist[nextTrackIndex++];
        musicAudioSource.clip = lastPlayedTrack;
        musicAudioSource.Play();
    }

    private void StopMusic()
    {
        if (musicAudioSource != null)
            musicAudioSource.Stop();
    }

    private void SetChunkBurning(string chunkID)
    {
        if (string.IsNullOrWhiteSpace(chunkID) || !burningChunks.Add(chunkID))
            return;

        if (chunkManager == null)
            chunkManager = ChunkManager.Instance;

        ChunkManager.ChunkData chunk = null;
        if (chunkManager != null)
        {
            foreach (ChunkManager.ChunkData data in chunkManager.chunks)
            {
                if (data != null && data.chunkID == chunkID)
                {
                    chunk = data;
                    break;
                }
            }
        }

        if (chunk == null || chunk.chunkObject == null)
        {
            Debug.LogWarning($"FinalFireSequence: não encontrei a chunk '{chunkID}' para criar o fogo.", this);
            return;
        }

        EnsureFlameSprite();
        Vector2[] positions = flameLocalPositions;
        if (positions == null || positions.Length == 0)
            positions = new[] { Vector2.zero };

        for (int i = 0; i < positions.Length; i++)
            CreateFlame(chunk.chunkObject.transform, chunkID, positions[i], i);
    }

    private void CreateFlame(Transform chunkRoot, string chunkID, Vector2 localPosition, int index)
    {
        GameObject flameObject = new GameObject($"Fire_{chunkID}_{index + 1}");
        flameObject.transform.SetParent(chunkRoot, false);
        flameObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -0.1f);
        flameObject.transform.localScale = Vector3.one * flameScale;

        SpriteRenderer renderer = flameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sharedFlameSprite;
        renderer.sortingOrder = 3;
        renderer.color = Color.white;

        flameVisuals.Add(new FlameVisual
        {
            transform = flameObject.transform,
            renderer = renderer,
            baseScale = flameObject.transform.localScale,
            phase = chunkID[0] * 0.13f + chunkID[1] * 0.19f + index * 1.71f
        });
    }

    private void LateUpdate()
    {
        if (flameVisuals.Count == 0)
            return;

        float time = Time.time;
        for (int i = flameVisuals.Count - 1; i >= 0; i--)
        {
            FlameVisual flame = flameVisuals[i];
            if (flame.transform == null || flame.renderer == null)
            {
                flameVisuals.RemoveAt(i);
                continue;
            }

            float wave = Mathf.Sin(time * 5.5f + flame.phase);
            flame.transform.localScale = new Vector3(
                flame.baseScale.x * (1f + wave * 0.09f),
                flame.baseScale.y * (1f + Mathf.Sin(time * 7f + flame.phase) * 0.12f),
                flame.baseScale.z);
            flame.renderer.color = Color.Lerp(
                new Color(1f, 0.48f, 0.08f, 0.9f),
                new Color(1f, 0.9f, 0.28f, 1f),
                (wave + 1f) * 0.5f);
        }
    }

    private void UpdateScreenWarning()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        float urgency = 1f - Mathf.Clamp01(RemainingTime / urgencyWindowSeconds);
        float intensity = Mathf.Lerp(startingWarningIntensity, maximumWarningIntensity, urgency);
        float pulseSpeed = Mathf.Lerp(startingPulseSpeed, maximumPulseSpeed, urgency);
        playerHealth.SetFireWarning(true, warningColor, intensity, pulseSpeed);
    }

    private void StopScreenWarning()
    {
        if (playerHealth != null)
            playerHealth.SetFireWarning(false, warningColor, 0f, 0f);
    }

    private void TriggerEnding(bool escaped)
    {
        if (outcomeTriggered)
            return;

        outcomeTriggered = true;
        StopScreenWarning();
        StopMusic();

        if (GameOverController.Instance == null)
        {
            Debug.LogError("FinalFireSequence: GameOverController não foi encontrado.", this);
            return;
        }

        if (escaped)
        {
            GameOverController.Instance.TriggerEnding(
                goodEndingTitle, goodEndingMessage, false, goodEndingRevealDelay);
        }
        else
        {
            GameOverController.Instance.TriggerEnding(
                badEndingTitle, badEndingMessage, true, 0f);
        }
    }

    private static void EnsureFlameSprite()
    {
        if (sharedFlameSprite != null)
            return;

        const int width = 48;
        const int height = 64;
        sharedFlameTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "GeneratedFinalFireTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            float t = y / (float)(height - 1);
            float center = 0.5f + Mathf.Sin(t * 8f) * 0.08f;
            float halfWidth = Mathf.Lerp(0.045f, 0.43f, Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.78f)));

            for (int x = 0; x < width; x++)
            {
                float distance = Mathf.Abs(x / (float)(width - 1) - center) / Mathf.Max(halfWidth, 0.001f);
                if (distance >= 1f)
                    continue;

                float edge = Mathf.SmoothStep(1f, 0f, distance);
                float baseHeat = Mathf.Clamp01(1f - t * 0.78f);
                Color color = Color.Lerp(new Color(0.95f, 0.08f, 0.005f),
                    new Color(1f, 0.92f, 0.2f), Mathf.Clamp01(baseHeat + (1f - distance) * 0.35f));
                float alpha = edge * Mathf.Clamp01((1f - t) * 5f + 0.08f);
                pixels[y * width + x] = new Color32(
                    (byte)Mathf.RoundToInt(color.r * 255f),
                    (byte)Mathf.RoundToInt(color.g * 255f),
                    (byte)Mathf.RoundToInt(color.b * 255f),
                    (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        sharedFlameTexture.SetPixels32(pixels);
        sharedFlameTexture.Apply(false, true);
        sharedFlameSprite = Sprite.Create(sharedFlameTexture,
            new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 48f);
        sharedFlameSprite.name = "GeneratedFinalFireSprite";
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;

        if (sharedFlameSprite != null)
            Destroy(sharedFlameSprite);
        if (sharedFlameTexture != null)
            Destroy(sharedFlameTexture);

        sharedFlameSprite = null;
        sharedFlameTexture = null;
    }

    private class FlameVisual
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public Vector3 baseScale;
        public float phase;
    }
}
