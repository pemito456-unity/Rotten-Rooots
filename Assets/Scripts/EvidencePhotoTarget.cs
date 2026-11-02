using UnityEngine;

/// <summary>
/// Marca um objeto como evidência que pode ser fotografada.
/// A imagem final da galeria é configurada no campo Gallery Image.
/// </summary>
public class EvidencePhotoTarget : MonoBehaviour
{
    public enum EvidenceCategory
    {
        MutantAnimal,
        Corpse,
        MutantTree
    }

    [Header("Identificação da evidência")]
    public EvidenceCategory category = EvidenceCategory.MutantAnimal;
    [Min(1)] public int evidenceIndex = 1;
    public string evidenceName = "Evidência";
    public Sprite galleryImage;

    [Header("Enquadramento")]
    [Tooltip("Ponto opcional para mirar a foto. Se vazio, usa a posição deste objeto.")]
    public Transform photoPoint;

    public Vector2 PhotoPosition
    {
        get
        {
            if (photoPoint != null)
                return (Vector2)photoPoint.position;

            return (Vector2)transform.position;
        }
    }

    public bool IsSupportedCategory
    {
        get
        {
            if (category == EvidenceCategory.MutantAnimal)
                return true;

            if (category == EvidenceCategory.Corpse)
                return true;

            return category == EvidenceCategory.MutantTree;
        }
    }

    private void OnValidate()
    {
        evidenceIndex = Mathf.Max(1, evidenceIndex);
    }
}
