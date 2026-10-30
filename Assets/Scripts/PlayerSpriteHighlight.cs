using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSpriteHighlight : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private SpriteRenderer sourceRenderer;
    [SerializeField] private SpriteRenderer highlightRenderer;
    [SerializeField] private Material highlightMaterial;

    [Header("Aparência")]
    [SerializeField] private Color highlightColor = new Color(0.68f, 1f, 0.68f, 1f);
    [SerializeField, Min(1f)] private float outlineScale = 1.07f;
    [SerializeField] private int sortingOrderOffset = -1;

    private void Awake()
    {
        ResolveReferences();
        SyncHighlight();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        SyncHighlight();
    }

    private void ResolveReferences()
    {
        if (sourceRenderer == null)
            sourceRenderer = GetComponent<SpriteRenderer>();

        if (highlightRenderer == null)
        {
            Transform child = transform.Find("PlayerSpriteHighlight");
            if (child != null)
                highlightRenderer = child.GetComponent<SpriteRenderer>();
        }
    }

    private void SyncHighlight()
    {
        if (sourceRenderer == null || highlightRenderer == null)
            return;

        bool shouldShow = sourceRenderer.enabled && sourceRenderer.sprite != null;
        highlightRenderer.enabled = shouldShow;
        if (!shouldShow)
            return;

        highlightRenderer.sprite = sourceRenderer.sprite;
        highlightRenderer.color = highlightColor;
        highlightRenderer.flipX = sourceRenderer.flipX;
        highlightRenderer.flipY = sourceRenderer.flipY;
        highlightRenderer.drawMode = sourceRenderer.drawMode;
        highlightRenderer.size = sourceRenderer.size;
        highlightRenderer.maskInteraction = sourceRenderer.maskInteraction;
        highlightRenderer.spriteSortPoint = sourceRenderer.spriteSortPoint;
        highlightRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        highlightRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
        if (highlightMaterial != null)
            highlightRenderer.sharedMaterial = highlightMaterial;
        highlightRenderer.transform.localScale = Vector3.one * outlineScale;
    }
}
