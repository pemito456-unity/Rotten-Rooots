using UnityEngine;

/// <summary>
/// Marks a scene object as a unique piece of evidence that can be photographed.
/// Assign the final artwork to galleryImage when it is provided by the art team.
/// </summary>
public class EvidencePhotoTarget : MonoBehaviour
{
    [Header("Identificação da evidência")]
    [Min(1)] public int evidenceIndex = 1;
    public string evidenceName = "Evidência";
    public Sprite galleryImage;

    [Header("Enquadramento")]
    [Tooltip("Ponto opcional para mirar a foto. Se vazio, usa a posição deste objeto.")]
    public Transform photoPoint;

    public Vector2 PhotoPosition => photoPoint != null
        ? (Vector2)photoPoint.position
        : (Vector2)transform.position;

    private void OnValidate()
    {
        evidenceIndex = Mathf.Max(1, evidenceIndex);
    }
}
