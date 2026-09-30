using UnityEngine;
using TMPro;

public class EnemyNametag : MonoBehaviour
{
    [Header("Configurações do Nome")]
    public string enemyName = "Inimigo";
    public TextMeshProUGUI nameText;

    [Header("Posicionamento")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);

    private Transform enemyTransform;

    private void Start()
    {
        // Encontra o transform do pai (o inimigo)
        if (transform.parent != null)
        {
            enemyTransform = transform.parent;
        }

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (nameText != null)
            nameText.text = enemyName;
    }

    private void LateUpdate()
    {
        if (enemyTransform == null) return;

        // Desvincula a posição do texto da rotação/scale do pai para não inverter ao dar flip
        transform.position = enemyTransform.position + offset;
        transform.rotation = Quaternion.identity; 
        transform.localScale = Vector3.one * 0.01f; // Mantém a escala World Space fixa
    }
}