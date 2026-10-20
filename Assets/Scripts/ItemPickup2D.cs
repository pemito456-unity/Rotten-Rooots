using UnityEngine;
using TMPro;

public class ItemPickup2D : MonoBehaviour
{
    public enum ItemType
    {
        Ammo,
        DecontamKit,
        Gun,
        PhotoCamera,
        Knife
    }

    [Header("Configurações do item")]
    public ItemType itemType;
    public int amount = 1;
    public string itemName = "Munição";
    public Sprite itemIcon;

    [Header("Dados carregados pelo item")]
    public int carriedAmmo;

    [Header("UI do item")]
    public GameObject promptCanvas;
    public TextMeshProUGUI promptText;

    private bool playerInRange;
    private static Sprite generatedKnifeSprite;
    private static Texture2D generatedKnifeTexture;

    private void Start()
    {
        if (itemType == ItemType.Knife && itemIcon == null)
            itemIcon = GetKnifePlaceholderSprite();

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (itemType == ItemType.Knife && spriteRenderer != null && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = itemIcon;
            spriteRenderer.sortingOrder = 5;
        }

        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        if (promptText != null)
            promptText.text = $"{itemName}\n[E / Y] Coletar";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (promptCanvas != null)
            promptCanvas.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void CollectItem(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError($"PlayerInventory ausente ao coletar {itemName}.");
            return;
        }

        Vector3 pickupWorldPosition = transform.position;

        bool success = inventory.AddItem(
            itemName,
            itemIcon,
            itemType,
            amount,
            carriedAmmo
        );

        if (!success)
        {
            Debug.LogWarning("Inventário cheio.");
            return;
        }

        if (InventoryUIManager.Instance != null)
        {
            if (itemType == ItemType.Ammo)
                InventoryUIManager.Instance.ShowAmmoPickupNotice(pickupWorldPosition, amount);
            else
                InventoryUIManager.Instance.ShowCollectionNotice(itemName, amount);

            InventoryUIManager.Instance.UpdateHotbarUI();
        }

        Destroy(gameObject);
    }

    public static void TryCollectNearby(
        Vector2 playerPosition,
        PlayerInventory inventory)
    {
        if (inventory == null)
            return;

        ItemPickup2D[] pickups = FindObjectsOfType<ItemPickup2D>();
        ItemPickup2D nearestPickup = null;
        float nearestDistance = float.MaxValue;

        foreach (ItemPickup2D pickup in pickups)
        {
            if (!pickup.playerInRange)
                continue;

            float distance = Vector2.Distance(
                playerPosition,
                pickup.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPickup = pickup;
            }
        }

        if (nearestPickup != null)
            nearestPickup.CollectItem(inventory);
    }

    private static Sprite GetKnifePlaceholderSprite()
    {
        if (generatedKnifeSprite != null)
            return generatedKnifeSprite;

        const int size = 32;
        generatedKnifeTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        generatedKnifeTexture.name = "KnifePlaceholderTexture";
        generatedKnifeTexture.filterMode = FilterMode.Point;
        generatedKnifeTexture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color outline = new Color(0.08f, 0.07f, 0.06f, 1f);
        Color blade = new Color(0.78f, 0.82f, 0.78f, 1f);
        Color bladeShade = new Color(0.48f, 0.55f, 0.52f, 1f);
        Color handle = new Color(0.34f, 0.17f, 0.09f, 1f);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        // Silhueta de uma faca simples: lâmina apontada para cima e para a direita.
        Vector2[] bladeShape =
        {
            new Vector2(7, 22), new Vector2(13, 18), new Vector2(27, 4),
            new Vector2(22, 20), new Vector2(14, 27)
        };
        Vector2[] handleShape =
        {
            new Vector2(3, 25), new Vector2(7, 21), new Vector2(15, 29),
            new Vector2(10, 32)
        };
        Vector2[] guardShape =
        {
            new Vector2(8, 19), new Vector2(11, 16), new Vector2(18, 23),
            new Vector2(15, 26)
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                if (InsidePolygon(point, bladeShape)) pixels[y * size + x] = blade;
                if (InsidePolygon(point, handleShape)) pixels[y * size + x] = handle;
                if (InsidePolygon(point, guardShape)) pixels[y * size + x] = outline;
            }
        }

        Color[] outlinedPixels = (Color[])pixels.Clone();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (pixels[y * size + x].a > 0f)
                    continue;

                bool touchesShape = false;
                for (int dy = -1; dy <= 1 && !touchesShape; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size &&
                            pixels[ny * size + nx].a > 0f)
                        {
                            touchesShape = true;
                            break;
                        }
                    }
                }

                if (touchesShape)
                    outlinedPixels[y * size + x] = outline;
            }
        }

        // Um fio de brilho diferencia a lâmina do cabo no ícone provisório.
        for (int i = 0; i < 9; i++)
        {
            int x = 14 + i;
            int y = 17 - i;
            if (x < size && y >= 0 && y < size)
                outlinedPixels[y * size + x] = bladeShade;
        }

        generatedKnifeTexture.SetPixels(outlinedPixels);
        generatedKnifeTexture.Apply(false, true);
        generatedKnifeSprite = Sprite.Create(
            generatedKnifeTexture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            32f
        );
        generatedKnifeSprite.name = "KnifePlaceholderSprite";
        return generatedKnifeSprite;
    }

    private static bool InsidePolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            bool crosses = (polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) *
                (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x;
            if (crosses)
                inside = !inside;
        }
        return inside;
    }
}
