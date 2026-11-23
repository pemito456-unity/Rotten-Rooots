using System;
using UnityEngine.InputSystem;

/// <summary>
/// Guarda qual tipo de dispositivo foi usado por último para atualizar os avisos de interação.
/// </summary>
public static class InteractionPromptInput
{
    public static event Action<bool> DeviceChanged;
    public static bool LastInputWasGamepad { get; private set; }

    public static void Register(InputAction.CallbackContext context)
    {
        if (context.control == null)
            return;

        SetGamepad(context.control.device is Gamepad);
    }

    private static void SetGamepad(bool isGamepad)
    {
        if (LastInputWasGamepad == isGamepad)
            return;

        LastInputWasGamepad = isGamepad;
        DeviceChanged?.Invoke(isGamepad);
    }
}
