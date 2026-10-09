using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Referência da Luz 2D")]
    public Light2D flashlightLight;

    [Header("Estado")]
    public bool isOn = true;

    [Header("Bateria")]
    [Min(1f)] public float batteryDurationSeconds = 30f;
    [SerializeField, Min(0f)] private float batteryRemainingSeconds = 30f;
    [SerializeField, Range(0, 4)] private int rechargePresses;

    [Header("Transição de direção")]
    public float rotationSpeed = 360f;

    private PlayerController2D playerController;
    private float batteryAtRechargeStart;
    private bool depletionFeedbackPlayed;
    private bool depletionFlickerActive;
    private float depletionFlickerTimer;
    private int depletionFlickerStep;

    private const int PressesToRecharge = 4;

    public float BatteryNormalized => batteryDurationSeconds <= 0f
        ? 0f
        : Mathf.Clamp01(batteryRemainingSeconds / batteryDurationSeconds);

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController2D>();

        batteryDurationSeconds = Mathf.Max(1f, batteryDurationSeconds);
        batteryRemainingSeconds = batteryDurationSeconds;

        if (flashlightLight == null)
            flashlightLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (flashlightLight == null)
            return;

        if (isOn && rechargePresses == 0 && batteryRemainingSeconds > 0f)
        {
            batteryRemainingSeconds = Mathf.Max(
                0f,
                batteryRemainingSeconds - Time.deltaTime
            );
        }

        if (batteryRemainingSeconds <= 0f)
        {
            if (!depletionFeedbackPlayed)
                BeginDepletionFlicker();

            UpdateDepletionFlicker();
            return;
        }

        // Com carga disponível, o estado da luz depende apenas da seleção e da recarga.
        // Isso evita que um pisca-pisca antigo deixe a luz apagada após recarregar.
        depletionFeedbackPlayed = false;
        depletionFlickerActive = false;
        flashlightLight.enabled = isOn && rechargePresses == 0;

        if (isOn && rechargePresses == 0)
            UpdateDirection();
    }

    public bool RegisterRechargePress()
    {
        if (!isOn)
        {
            Debug.Log("Selecione a lanterna para recarregá-la.");
            return false;
        }

        if (rechargePresses == 0)
        {
            if (BatteryNormalized >= 1f)
            {
                Debug.Log("A bateria da lanterna já está cheia.");
                return false;
            }

            batteryAtRechargeStart = batteryRemainingSeconds;
            depletionFlickerActive = false;
            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }

        rechargePresses++;
        batteryRemainingSeconds = Mathf.Lerp(
            batteryAtRechargeStart,
            batteryDurationSeconds,
            rechargePresses / (float)PressesToRecharge
        );

        Debug.Log($"Recarga da lanterna: {rechargePresses}/{PressesToRecharge}.");

        if (rechargePresses >= PressesToRecharge)
        {
            rechargePresses = 0;
            batteryRemainingSeconds = batteryDurationSeconds;
            Debug.Log("Lanterna recarregada.");
        }

        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.UpdateHotbarUI();

        return true;
    }

    private void BeginDepletionFlicker()
    {
        depletionFeedbackPlayed = true;
        depletionFlickerActive = true;
        depletionFlickerTimer = 0.12f;
        depletionFlickerStep = 0;
        flashlightLight.enabled = isOn;
    }

    private void UpdateDepletionFlicker()
    {
        if (!depletionFlickerActive)
        {
            flashlightLight.enabled = false;
            return;
        }

        depletionFlickerTimer -= Time.deltaTime;
        if (depletionFlickerTimer > 0f)
            return;

        depletionFlickerTimer = 0.12f;
        depletionFlickerStep++;

        if (depletionFlickerStep >= 6)
        {
            depletionFlickerActive = false;
            flashlightLight.enabled = false;
            return;
        }

        flashlightLight.enabled = isOn && depletionFlickerStep % 2 == 0;
    }

    private void UpdateDirection()
    {
        if (playerController == null)
            return;

        Vector2 direction = playerController.lastFacingDirection;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        float targetAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void ToggleFlashlight()
    {
        isOn = !isOn;
    }
}
