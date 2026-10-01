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
            healthBarFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }
}