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

    private InputAction leftFlipperAction;
    private InputAction rightFlipperAction;

    public Slider leftAxisSlider;
    public Slider rightAxisSlider;

    private InputMode inputMode = InputMode.Gamepad;

    private bool leftFlipper;
    private bool rightFlipper;

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
        leftFlipperAction = InputActionAsset.FindAction("LeftFlipper");
        rightFlipperAction = InputActionAsset.FindAction("RightFlipper");
        InputActionAsset.Enable();
    }

    private void OnEnable() =>
        QuantumCallback.Subscribe(listener: this, handler: (CallbackPollInput callback) => PollInput(callback));

    private void OnDisable() =>
        QuantumCallback.UnsubscribeListener(this);


    void Update()
    {
        leftFlipper = leftFlipper || leftFlipperAction.IsPressed();
        rightFlipper = rightFlipper || rightFlipperAction.IsPressed();
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

        leftAxis = leftAxisAction.ReadValue<Vector2>();
        rightAxis = rightAxisAction.ReadValue<Vector2>();


        var playerSlot = callback.PlayerSlot;
        var input = new Quantum.Input();
        input.LeftAxis = leftAxis.ToFPVector2();
        input.RightAxis = rightAxis.ToFPVector2();
        leftFlipper = leftFlipper || leftFlipperAction.IsPressed();
        rightFlipper = rightFlipper || rightFlipperAction.IsPressed();
        input.LeftFlipper = leftFlipper;
        input.RightFlipper = rightFlipper;

        // Reset latched buttons
        leftFlipper = false;
        rightFlipper = false;

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }


}