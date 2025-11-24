using System;
using TMPro;
using UnityEngine;

public class ControlUIText : MonoBehaviour
{
    public TMP_Text ControlText;

    public PlayerInput PlayerInput;

    private void Awake()
    {
        PlayerInput.OnInputModeChanged += UpdateControlText;
    }

    private void OnDestroy()
    {
        PlayerInput.OnInputModeChanged -= UpdateControlText;
    }

    private void UpdateControlText(PlayerInput.InputMode inputMode)
    {
        if (inputMode == PlayerInput.InputMode.Gamepad)
        {
            ControlText.text = "Controls: Gamepad";
        }
        else if (inputMode == PlayerInput.InputMode.Sliders)
        {
            ControlText.text = "Controls: Sliders";
        }
        else if (inputMode == PlayerInput.InputMode.Tilt)
        {
            ControlText.text = "Controls: Tilt";
        }
        else if (inputMode == PlayerInput.InputMode.Touch)
        {
            ControlText.text = "Controls: Touch";
        }
    }
}
