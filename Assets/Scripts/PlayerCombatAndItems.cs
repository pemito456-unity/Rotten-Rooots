using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class PlayerCombatAndItems : MonoBehaviour
{
    [Header("Referências principais")]
    public PlayerInventory inventory;

    [Header("Configurações da pistola")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float wrathPerShot = 10f;

    [Header("Som do disparo")]
    public AudioClip shotSound;
    public AudioMixerGroup shotMixerGroup;
    [Range(0f, 1f)] public float shotVolume = 0.8f;

    [Header("Ataque corpo a corpo com faca")]
    public float meleeDamage = 20f;
    public float meleeReach = 0.8f;
    public float meleeRadius = 0.75f;
    public float meleeCooldown = 0.45f;
    public float wrathPerMeleeHit = 2f;
    public AudioClip meleeSwingSound;
    public AudioClip meleeHitSound;
    public AudioMixerGroup meleeMixerGroup;
    [Range(0f, 1f)] public float meleeVolume = 0.8f;
    public Color meleeSlashColor = new Color(0.8f, 1f, 0.75f, 1f);

    [Header("Configurações do kit de descontaminação")]
    public float wrathReductionPerKit = 25f;
    public float decontamKitUseRange = 2.25f;

    private PlayerController2D playerController;
    private AudioSource shotAudioSource;
    private AudioSource meleeAudioSource;
    private AudioClip generatedMeleeSwingSound;
    private LineRenderer meleeSlashRenderer;
    private Material meleeSlashMaterial;
    private Coroutine meleeSlashCoroutine;
    private float lastMeleeTime = -Mathf.Infinity;
    private PhotoEvidenceCamera photoCamera;

    private void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        playerController = GetComponent<PlayerController2D>();

        photoCamera = GetComponent<PhotoEvidenceCamera>();
        if (photoCamera == null)
            photoCamera = gameObject.AddComponent<PhotoEvidenceCamera>();

        shotAudioSource = gameObject.AddComponent<AudioSource>();
        shotAudioSource.playOnAwake = false;
        shotAudioSource.spatialBlend = 0f;
        shotAudioSource.outputAudioMixerGroup = shotMixerGroup;

        meleeAudioSource = gameObject.AddComponent<AudioSource>();
        meleeAudioSource.playOnAwake = false;
        meleeAudioSource.spatialBlend = 0f;
        meleeAudioSource.outputAudioMixerGroup = meleeMixerGroup != null
            ? meleeMixerGroup
            : shotMixerGroup;

        if (meleeSwingSound == null)
            generatedMeleeSwingSound = CreateMeleeSwingSound();

        CreateMeleeSlashVisual();
    }

    // Pistola selecionada: dispara.
    // Kit selecionado: usa o kit.
    public void OnPrimary(InputAction.CallbackContext context)
    {
        if (!context.performed || inventory == null)
            return;

        CorpseNotePanelController notePanel = CorpseNotePanelController.Instance;
        if (notePanel != null && notePanel.IsOpen)
            return;

        InventorySlotData selectedSlot = inventory.SelectedSlot;

        if (selectedSlot == null || selectedSlot.isFlashlight)
            return;

        if (selectedSlot.isCamera)
        {
            if (photoCamera != null)
                photoCamera.TakePhoto();
            return;
        }

        switch (selectedSlot.type)
        {
            case ItemPickup2D.ItemType.Gun:
                TryShoot(selectedSlot);
                break;

            case ItemPickup2D.ItemType.DecontamKit:
                TryUseDecontamKit(selectedSlot);
                break;

            case ItemPickup2D.ItemType.Knife:
                TryMeleeAttack();
                break;

            default:
                Debug.Log("O item selecionado não tem ação no botão principal.");
                break;
        }
    }

    // A pistola não recarrega.
    // Este callback fica disponível para a recarga futura da lanterna.
    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed || inventory == null)
            return;

        InventorySlotData selectedSlot = inventory.SelectedSlot;

        if (selectedSlot != null && selectedSlot.isFlashlight)
        {
            FlashlightController flashlight = inventory.FlashlightController;
            if (flashlight != null)
                flashlight.RegisterRechargePress();
            else
                Debug.LogWarning("FlashlightController não foi encontrado no Player.");
            return;
        }

        Debug.Log("A pistola não é recarregável.");
    }

    private void TryShoot(InventorySlotData pistolSlot)
    {
        if (!inventory.hasGun)
        {
            Debug.Log("Você ainda não possui uma pistola.");
            return;
        }

        if (pistolSlot.ammoCount <= 0)
        {
            Debug.Log("A pistola está sem munição.");
            return;
        }

        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogWarning(
                "Configure Fire Point e Bullet Prefab no PlayerCombatAndItems."
            );
            return;
        }

        Vector2 direction = playerController != null
            ? playerController.lastFacingDirection
            : Vector2.right;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (shotSound != null && shotAudioSource != null)
            shotAudioSource.PlayOneShot(shotSound, shotVolume);

        pistolSlot.ammoCount--;
        inventory.RefreshInventoryUI();

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.AddWrath(wrathPerShot);

        Debug.Log($"Tiro disparado. Munição restante: {pistolSlot.ammoCount}.");
    }

    private void TryUseDecontamKit(InventorySlotData kitSlot)
    {
        if (kitSlot == null || kitSlot.amount <= 0)
            return;

        ArvoreSinistra nearbyTree = FindNearbyTreeForKit();
        if (nearbyTree != null)
            nearbyTree.DispelByKit();

        kitSlot.amount--;

        if (kitSlot.amount <= 0)
            inventory.slots.Remove(kitSlot);

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.ReduceWrath(wrathReductionPerKit);

        inventory.RefreshInventoryUI();

        Debug.Log(nearbyTree != null
            ? "Kit de descontaminação usado. A árvore sinistra foi removida e a ira diminuiu."
            : "Kit usado. A ira da floresta diminuiu.");
    }

    private ArvoreSinistra FindNearbyTreeForKit()
    {
        ArvoreSinistra nearestTree = null;
        float nearestDistance = decontamKitUseRange;

        foreach (ArvoreSinistra tree in FindObjectsOfType<ArvoreSinistra>())
        {
            if (tree == null)
                continue;

            float distance = Vector2.Distance(transform.position, tree.transform.position);
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestTree = tree;
            }
        }

        return nearestTree;
    }

    private void TryMeleeAttack()
    {
        if (Time.time < lastMeleeTime + meleeCooldown)
            return;

        lastMeleeTime = Time.time;
        Vector2 direction = playerController != null
            ? playerController.lastFacingDirection
            : Vector2.right;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;
        direction.Normalize();

        ShowMeleeSlash(direction);
        AudioClip swingClip = meleeSwingSound != null
            ? meleeSwingSound
            : generatedMeleeSwingSound;
        if (swingClip != null && meleeAudioSource != null)
            meleeAudioSource.PlayOneShot(swingClip, meleeVolume);

        Vector2 attackCenter = (Vector2)transform.position + direction * meleeReach;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackCenter, meleeRadius);
        HashSet<int> hitTargets = new HashSet<int>();
        bool hitSomething = false;

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider == null || hitCollider.transform.root == transform.root)
                continue;

            IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
            if (target == null)
                continue;

            Component targetComponent = target as Component;
            if (targetComponent == null || !hitTargets.Add(targetComponent.GetInstanceID()))
                continue;

            ArvoreSinistra tree = target as ArvoreSinistra;
            if (tree != null && tree.OnlyRemovableByKit)
                continue;

            target.TakeDamage(meleeDamage);
            hitSomething = true;
        }

        if (hitSomething)
        {
            if (meleeHitSound != null && meleeAudioSource != null)
                meleeAudioSource.PlayOneShot(meleeHitSound, meleeVolume);

            if (ForestWrathManager.Instance != null && wrathPerMeleeHit > 0f)
                ForestWrathManager.Instance.AddWrath(wrathPerMeleeHit);
        }
    }

    private void CreateMeleeSlashVisual()
    {
        GameObject slashObject = new GameObject("MeleeSlashFeedback");
        slashObject.transform.SetParent(transform, false);
        meleeSlashRenderer = slashObject.AddComponent<LineRenderer>();
        meleeSlashRenderer.useWorldSpace = false;
        meleeSlashRenderer.positionCount = 2;
        meleeSlashRenderer.SetPosition(0, new Vector3(-0.35f, -0.32f, 0f));
        meleeSlashRenderer.SetPosition(1, new Vector3(0.35f, 0.32f, 0f));
        meleeSlashRenderer.startWidth = 0.12f;
        meleeSlashRenderer.endWidth = 0.04f;
        meleeSlashRenderer.startColor = meleeSlashColor;
        meleeSlashRenderer.endColor = new Color(meleeSlashColor.r, meleeSlashColor.g,
            meleeSlashColor.b, 0.15f);
        meleeSlashRenderer.sortingOrder = 100;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            meleeSlashMaterial = new Material(shader);
            meleeSlashRenderer.material = meleeSlashMaterial;
        }

        meleeSlashRenderer.enabled = false;
    }

    private void ShowMeleeSlash(Vector2 direction)
    {
        if (meleeSlashRenderer == null)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meleeSlashRenderer.transform.localPosition = (Vector3)(direction * 0.6f);
        meleeSlashRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        meleeSlashRenderer.startColor = meleeSlashColor;
        meleeSlashRenderer.enabled = true;

        if (meleeSlashCoroutine != null)
            StopCoroutine(meleeSlashCoroutine);
        meleeSlashCoroutine = StartCoroutine(HideMeleeSlash());
    }

    private IEnumerator HideMeleeSlash()
    {
        yield return new WaitForSeconds(0.14f);
        if (meleeSlashRenderer != null)
            meleeSlashRenderer.enabled = false;
        meleeSlashCoroutine = null;
    }

    private AudioClip CreateMeleeSwingSound()
    {
        const int sampleRate = 22050;
        const int sampleCount = 3087; // 0,14 s
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float envelope = Mathf.Sin(Mathf.PI * t);
            float frequency = Mathf.Lerp(1050f, 240f, t);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            float texturedTone = Mathf.Sin(phase) * 0.55f + Mathf.Sin(phase * 2.17f) * 0.18f;
            float softNoise = Mathf.Sin(i * 78.233f) * 0.18f;
            samples[i] = (texturedTone + softNoise) * envelope * 0.32f;
        }

        AudioClip clip = AudioClip.Create("GeneratedMeleeWhoosh", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (generatedMeleeSwingSound != null)
            Destroy(generatedMeleeSwingSound);
        if (meleeSlashMaterial != null)
            Destroy(meleeSlashMaterial);
    }
}
