using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartHUD : MonoBehaviour
{
    [Header("Referências")]
    public PlayerHealth playerHealth;
    public Image heartImage;              // placeholder: círculo vermelho
    public TextMeshProUGUI healthText;    // contador abaixo do coração

    [Header("Batimento")]
    public float minBpm = 55f;            // vida cheia
    public float maxBpm = 150f;           // vida crítica
    [Range(0f, 0.5f)] public float pulseAmplitude = 0.14f; // intensidade do pulso

    [Header("Placeholder")]
    public bool generateCircleIfMissing = true;

    private RectTransform heartRect;
    private float phase; // fase acumulada do batimento (radianos)
    private int lastShownHealth = -1;

    private void Awake()
    {
        if (heartImage != null) heartRect = heartImage.rectTransform;

        // Placeholder: círculo vermelho sem precisar de sprite
        if (heartImage != null && heartImage.sprite == null && generateCircleIfMissing)
        {
            heartImage.sprite = CreateCircleSprite(128);
            heartImage.color = new Color(0.75f, 0.1f, 0.1f); // vermelho sangue
        }
    }

    private void OnEnable()
    {
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (playerHealth == null || heartRect == null) return;

        // 1. BPM interpola com a fração de vida (55 -> 150)
        float fraction = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
        float bpm = Mathf.Lerp(maxBpm, minBpm, fraction); // menos vida = mais rápido

        // 2. Fase acumulada: permite o BPM mudar sem "pular" a animação
        phase += (bpm / 60f) * 2f * Mathf.PI * Time.deltaTime;

        // 3. Pulso: pico rápido e vale longo (parece batida de verdade, não senoide)
        float beat = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(phase)), 3f);
        float scale = 1f + pulseAmplitude * beat;
        heartRect.localScale = new Vector3(scale, scale, 1f);
    }

    private void OnHealthChanged(float current, float max)
    {
        // Contador: porcentagem inteira (100, 80, 60...)
        int percent = Mathf.CeilToInt((current / max) * 100f);
        if (healthText != null && percent != lastShownHealth)
        {
            healthText.text = percent.ToString();
            lastShownHealth = percent;
        }
    }

    private static Sprite CreateCircleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f - 2f;
        Vector2 c = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                float alpha = Mathf.Clamp01(r - d + 1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
