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
    public float rotationSpeed = 420f;

    private PlayerController2D playerController;
    private float batteryAtRechargeStart;
    private bool isRecharging;
    private bool depletionFeedbackPlayed;
    private bool depletionFlickerActive;
    private float depletionFlickerTimer;
    private int depletionFlickerStep;

    private const int PressesToRecharge = 4;

    public float BatteryNormalized
    {
        get
        {
            if (batteryDurationSeconds <= 0f)
                return 0f;

            return Mathf.Clamp01(batteryRemainingSeconds / batteryDurationSeconds);
        }
    }

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController2D>();

        // A lanterna fica equipada durante toda a partida.
        isOn = true;
        batteryDurationSeconds = Mathf.Max(1f, batteryDurationSeconds);
        batteryRemainingSeconds = batteryDurationSeconds;

        if (flashlightLight == null)
            flashlightLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (flashlightLight == null)
            return;

        if (isOn && !isRecharging && batteryRemainingSeconds > 0f)
        {
            batteryRemainingSeconds = Mathf.Max(
                0f,
                batteryRemainingSeconds - Time.deltaTime
            );

            // Depois da recarga completa, libera um novo ciclo só quando
            // a bateria já tiver começado a ser usada de verdade.
            if (rechargePresses >= PressesToRecharge && BatteryNormalized < 0.99f)
                rechargePresses = 0;
        }

        if (batteryRemainingSeconds <= 0f)
        {
            if (!depletionFeedbackPlayed)
                BeginDepletionFlicker();

            UpdateDepletionFlicker();
            return;
        }

        depletionFeedbackPlayed = false;
        depletionFlickerActive = false;
        flashlightLight.enabled = isOn && !isRecharging;

        if (isOn && !isRecharging)
            UpdateDirection();
    }

    public bool RegisterRechargePress()
    {
        // A quarta pressão completa a carga. Toques extras não iniciam
        // outra recarga nem desligam a lanterna.
        if (rechargePresses >= PressesToRecharge)
        {
            if (BatteryNormalized >= 0.99f)
                return false;

            rechargePresses = 0;
        }

        if (rechargePresses == 0)
        {
            if (BatteryNormalized >= 1f)
            {
                Debug.Log("A bateria da lanterna já está cheia.");
                return false;
            }

            batteryAtRechargeStart = batteryRemainingSeconds;
            isRecharging = true;
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
            rechargePresses = PressesToRecharge;
            batteryRemainingSeconds = batteryDurationSeconds;
            isRecharging = false;
            depletionFeedbackPlayed = false;
            depletionFlickerActive = false;
            if (flashlightLight != null)
                flashlightLight.enabled = isOn;
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

}
