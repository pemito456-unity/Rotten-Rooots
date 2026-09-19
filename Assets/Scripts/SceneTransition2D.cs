using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition2D : MonoBehaviour
{
    [Header("Configuração de Destino")]
    [Tooltip("Digite exatamente o nome da cena para onde o jogador deve ir")]
    public string targetSceneName; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"Carregando a cena: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("O nome da cena de destino não foi preenchido no Inspector!");
            }
        }
    }
}