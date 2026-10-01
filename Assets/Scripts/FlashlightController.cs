using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Referência da Luz 2D")]
    public Light2D flashlightLight;

    [Header("Estado")]
    public bool isOn = true;

    private PlayerController2D playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController2D>();

        if (flashlightLight == null)
            flashlightLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (flashlightLight == null) return;

        flashlightLight.enabled = isOn;

        if (isOn)
        {
            UpdateDirection();
        }
    }

    private void UpdateDirection()
    {
        if (playerController == null) return;

        Vector2 dir = playerController.lastFacingDirection;
        if (dir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }

    public void ToggleFlashlight()
    {
        isOn = !isOn;
    }
}