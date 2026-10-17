using UnityEngine;

/// <summary>
/// Marks a scene object as a unique piece of evidence that can be photographed.
/// Assign the final artwork to galleryImage when it is provided by the art team.
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

    public Vector2 PhotoPosition => photoPoint != null
        ? (Vector2)photoPoint.position
        : (Vector2)transform.position;

    public bool IsSupportedCategory =>
        category == EvidenceCategory.MutantAnimal ||
        category == EvidenceCategory.Corpse ||
        category == EvidenceCategory.MutantTree;

    private void OnValidate()
    {
        evidenceIndex = Mathf.Max(1, evidenceIndex);
    }
}
