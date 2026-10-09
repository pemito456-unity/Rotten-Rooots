using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyNametag : MonoBehaviour
{
    [Header("Configuração do Nome")]
    public string enemyName = "Inimigo";
    public TextMeshProUGUI nameText;

    [Header("Barra de Vida")]
    public Image healthBarFill; // Arraste a Image 'HealthBarFill' aqui

    [Header("Ajuste de Posição")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);

    private Transform canvasTransform;
    private RectTransform healthBarFillRect;
    private Vector2 healthBarInitialAnchorMin;
    private Vector2 healthBarInitialAnchorMax;

    private void Awake()
    {
        if (healthBarFill != null)
        {
            healthBarFillRect = healthBarFill.rectTransform;
            healthBarInitialAnchorMin = healthBarFillRect.anchorMin;
            healthBarInitialAnchorMax = healthBarFillRect.anchorMax;
        }
    }

    private void Start()
    {
        if (nameText != null)
        {
            nameText.text = enemyName;
            canvasTransform = nameText.transform.parent;
        }
    }

    private void LateUpdate()
    {
        // Mantém a UI alinhada ao topo e previne distorção quando o sprite do inimigo espelha (Flip)
        if (canvasTransform != null)
        {
            canvasTransform.position = transform.position + offset;
            canvasTransform.rotation = Quaternion.identity;
        }
    }

    // Atualiza a proporção da barra (valor entre 0 e 1)
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null && maxHealth > 0)
        {
            float healthRatio = Mathf.Clamp01(currentHealth / maxHealth);

            // Reduz a largura pelos anchors, que também funciona quando a Image
            // está em Simple ou não tem Sprite (casos presentes nos prefabs atuais).
            if (healthBarFillRect != null)
            {
                Vector2 anchorMax = healthBarInitialAnchorMax;
                anchorMax.x = Mathf.Lerp(healthBarInitialAnchorMin.x,
                    healthBarInitialAnchorMax.x, healthRatio);
                healthBarFillRect.anchorMax = anchorMax;
            }

            // Mantém Filled Images compatíveis sem aplicar a redução duas vezes.
            healthBarFill.fillAmount = 1f;
        }
    }
}
