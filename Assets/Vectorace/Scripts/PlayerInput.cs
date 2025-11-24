using UnityEngine;
using Photon.Deterministic;
using Quantum;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public unsafe class PlayerInput : MonoBehaviour
{
    public InputActionAsset InputActionAsset;

    private InputAction leftAxisAction;
    private InputAction rightAxisAction;

    private InputAction tiltAction;

    private InputAction touchAction;


    public Slider leftAxisSlider;
    public Slider rightAxisSlider;

    private InputMode inputMode = InputMode.Gamepad;

    public static event Action<InputMode> OnInputModeChanged;

    public enum InputMode
    {
        Gamepad,
        Sliders,
        Tilt,
        Touch
    }

    private void Start()
    {
        leftAxisAction = InputActionAsset.FindAction("LeftAxis");
        rightAxisAction = InputActionAsset.FindAction("RightAxis");
        tiltAction = InputActionAsset.FindAction("Tilt");
        touchAction = InputActionAsset.FindAction("Touch");
        InputActionAsset.Enable();
    }

    private void OnEnable() =>
        QuantumCallback.Subscribe(listener: this, handler: (CallbackPollInput callback) => PollInput(callback));

    private void OnDisable() =>
        QuantumCallback.UnsubscribeListener(this);


    void Update()
    {
        // would latch buttons here, if we used any
    }

    public void ToggleInputMode()
    {
        if (inputMode == InputMode.Gamepad)
            inputMode = InputMode.Sliders;
        else if (inputMode == InputMode.Sliders)
            inputMode = InputMode.Tilt;
        else if (inputMode == InputMode.Tilt)
            inputMode = InputMode.Touch;
        else
            inputMode = InputMode.Gamepad;


        OnInputModeChanged?.Invoke(inputMode);
    }

    /// <summary>
    /// Quantum callback to collect networked input.
    /// Networked input affects simulation. Non-networked input does not.
    /// This is called per player.
    /// </summary>
    public void PollInput(CallbackPollInput callback)
    {
        var localPlayers = QuantumRunner.Default.Game.GetLocalPlayers();
        if (localPlayers.Count == 0)
            return;

        Vector2 leftAxis = default;
        Vector2 rightAxis = default;

        if (inputMode == InputMode.Gamepad)
        {
            leftAxis = leftAxisAction.ReadValue<Vector2>();
            rightAxis = rightAxisAction.ReadValue<Vector2>();
        }
        else if (inputMode == InputMode.Sliders)
        {
            leftAxis.x = leftAxisSlider.value;
            rightAxis.y = rightAxisSlider.value;
        }
        else if (inputMode == InputMode.Tilt)
        {
            leftAxis = tiltAction.ReadValue<Vector2>();
        } else if (inputMode == InputMode.Touch)
        {
            var pointer = touchAction.ReadValue<Vector2>();
            leftAxis = pointer;
        }


        var playerSlot = callback.PlayerSlot;
        var input = new Quantum.Input();
        input.LeftAxis = leftAxis.ToFPVector2();
        input.RightAxis = rightAxis.ToFPVector2();

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }


}