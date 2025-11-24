using UnityEngine;
using Photon.Deterministic;
using Quantum;
using UnityEngine.InputSystem;

public unsafe class PlayerInput : MonoBehaviour
{
    public InputActionAsset InputActionAsset;

    private InputAction leftAxisAction;
    private InputAction rightAxisAction;


    private void Start()
    {
        leftAxisAction = InputActionAsset.FindAction("LeftAxis");
        rightAxisAction = InputActionAsset.FindAction("RightAxis");
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

        var leftAxis = leftAxisAction.ReadValue<Vector2>();
        var rightAxis = rightAxisAction.ReadValue<Vector2>();


        var playerSlot = callback.PlayerSlot;
        var input = new Quantum.Input();
        input.LeftAxis = leftAxis.ToFPVector2();
        input.RightAxis = rightAxis.ToFPVector2();

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }


}