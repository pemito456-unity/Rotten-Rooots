using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Referência da Luz 2D")]
    public Light2D flashlightLight;

    [Header("Estado")]
    public bool isOn = true;

    [Header("Transição de direção")]
    public float rotationSpeed = 360f;

    private PlayerController2D playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController2D>();

        if (flashlightLight == null)
            flashlightLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (flashlightLight == null)
            return;

        flashlightLight.enabled = isOn;

        if (isOn)
            UpdateDirection();
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