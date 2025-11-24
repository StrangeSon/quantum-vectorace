using UnityEngine;
using Quantum;
using System;

public unsafe class CameraController : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private QuantumEntityViewUpdater entityViewUpdater;

    [Header("Positioning")]
    [SerializeField] private float initialDistance = 80f;  // Starting distance (units); adjust for zoom out
    [SerializeField] private float minDistance = 80f;
    [SerializeField] private float maxDistance = 1000f;
    [SerializeField] private Vector3 tableCenter = Vector3.zero;

    [Header("Rotation")]
    [SerializeField][Range(0f, 1f)] private float rotationDamping = 0.1f;
    [SerializeField] private float yawSpeed = 150f;  // Mouse sensitivity for horizontal rotate
    [SerializeField] private float pitchSpeed = 150f;  // Mouse sensitivity for vertical tilt
    [SerializeField] private float zoomSpeed = 100f;  // Mouse wheel zoom speed
    [SerializeField] private float minPitch = 5f;  // Min downward angle (degrees)
    [SerializeField] private float maxPitch = 80f;  // Max downward angle

    [Header("Mode")]
    [SerializeField] private CameraMode mode = CameraMode.CenterOnTable;

    [Header("Forward Locking")]
    [SerializeField] private ForwardLockMode forwardLock = ForwardLockMode.LockedToWorldZ;
    [SerializeField] private Vector3 referenceForward = Vector3.forward;  // Base for locked mode

    private EntityRef followingEntityRef;
    private Quaternion targetRotation;
    [SerializeField] private float currentYaw = 0f;
    [SerializeField] private float currentPitch = 30f;  // Initial downward tilt for realistic angle
    [SerializeField] private float currentDistance;

    public enum CameraMode { FollowMarble, CenterOnTable }
    public enum ForwardLockMode { Free, LockedToWorldZ }

    void Awake()
    {
        QuantumEvent.Subscribe<EventPlayerLinked>(this, OnPlayerLinked);
        currentDistance = initialDistance;
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void OnPlayerLinked(EventPlayerLinked eventData)
    {
        var localPlayers = QuantumRunner.Default.Game.GetLocalPlayers();
        if (!localPlayers.Contains(eventData.PlayerRef)) return;

        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        if (!frame.Unsafe.TryGetPointer(eventData.PlayerEntity, out Marble* marble)) return;

        followingEntityRef = eventData.PlayerEntity;
    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        if (!followingEntityRef.IsValid) return;

        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        var gravity = frame.PhysicsSceneSettings->Gravity.ToUnityVector3();
        var gravityDir = gravity.normalized;
        var upDir = -gravityDir;

        // Get target position
        Vector3 targetPos = (mode == CameraMode.FollowMarble)
            ? frame.Unsafe.GetPointer<Transform3D>(followingEntityRef)->Position.ToUnityVector3()
            : tableCenter;

        // Base forward based on lock mode
        Vector3 baseForward = (forwardLock == ForwardLockMode.LockedToWorldZ)
            ? referenceForward
            : camera.transform.forward;

        // Project base forward onto plane perp to upDir (horizontal relative to gravity)
        Vector3 horizontalForward = Vector3.ProjectOnPlane(baseForward, upDir).normalized;
        if (horizontalForward == Vector3.zero) horizontalForward = Vector3.forward;

        // Apply yaw rotation around upDir (orbit horizontally)
        horizontalForward = Quaternion.AngleAxis(currentYaw, upDir) * horizontalForward;

        // Compute local right for pitch axis
        Vector3 rightDir = Vector3.Cross(upDir, horizontalForward).normalized;

        // Apply pitch tilt (downward angle)
        Vector3 forwardDir = Quaternion.AngleAxis(currentPitch, rightDir) * horizontalForward;

        // Target rotation: Look along forwardDir with gravity up
        targetRotation = Quaternion.LookRotation(forwardDir, upDir);

        // Smooth rotation
        camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, targetRotation, rotationDamping > 0 ? Time.deltaTime / rotationDamping : 1f);

        // Position: Back along view direction (orbit distance)
        camera.transform.position = targetPos - camera.transform.forward * currentDistance;
    }
}