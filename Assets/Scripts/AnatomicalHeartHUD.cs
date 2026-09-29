using UnityEngine;
using UnityEngine.UI;

public class AnatomicalHeartHUD : MonoBehaviour
{
    public Image heartFillImage;

    private void Awake()
    {
        if (heartFillImage == null)
            heartFillImage = GetComponent<Image>();
    }

    // Atualiza a porcentagem de preenchimento do coração (0.0 a 1.0)
    public void UpdateHeartUI(float currentHealth, float maxHealth)
    {
        if (heartFillImage != null)
        {
            heartFillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }
}