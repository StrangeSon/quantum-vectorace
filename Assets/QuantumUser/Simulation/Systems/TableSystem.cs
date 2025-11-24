namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;
    using Quantum.Physics3D;

    [Preserve]
    public unsafe class TableSystem : SystemMainThreadFilter<TableSystem.Filter>, ISignalOnComponentAdded<Table>, ISignalOnTriggerEnter3D
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var playerLink = filter.PlayerLink;
            var input = frame.GetPlayerInput(playerLink->PlayerRef);
            var leftAxis = input->LeftAxis;
            var rightAxis = input->RightAxis;
            FPVector2 combinedAxis = default;

            var combinedX = FPMath.Clamp(leftAxis.X + rightAxis.X, -1, 1);
            var combinedY = FPMath.Clamp(leftAxis.Y + rightAxis.Y, -1, 1);
            combinedAxis = new FPVector2(combinedX, combinedY);

            var maxTilt = FP.FromString("15");

            // Compute angles: pitch (forward/back) from Y-axis, roll (left/right) from X-axis
            var pitch = -combinedAxis.Y * maxTilt;  // Negative for intuitive forward tilt
            var roll = combinedAxis.X * maxTilt;

            // Default gravity magnitude and direction
            var gravityMagnitude = FP.FromString("9810");
            var defaultDirection = FPVector3.Down;  // Unit vector for direction

            // Create quaternions for rotations (order: pitch then roll for natural feel)
            var quatPitch = FPQuaternion.AngleAxis(pitch, FPVector3.Right);  // X-axis
            var quatRoll = FPQuaternion.AngleAxis(roll, FPVector3.Forward);  // Z-axis

            // Combine and normalize to prevent FP precision drift
            var combinedQuat = FPQuaternion.Normalize(quatPitch * quatRoll);

            // Rotate the direction, then scale by magnitude (ensures exact length)
            var rotatedDirection = combinedQuat * defaultDirection;
            rotatedDirection = FPVector3.Normalize(rotatedDirection);  // Extra safety for vector
            var rotatedGravity = rotatedDirection * gravityMagnitude;

            // Apply to global physics
            frame.PhysicsSceneSettings->Gravity = rotatedGravity;
        }

        public void OnAdded(Frame frame, EntityRef entity, Table* component)
        {
            frame.Events.TableAdded(entity);
        }

        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D info)
        {
            if (!frame.Unsafe.TryGetPointer(info.Entity, out Marble* marble))
                return;

            if (!frame.Unsafe.TryGetPointer(info.Entity, out PhysicsBody3D* physicsBody))
                return;

            physicsBody->Velocity = FPVector3.Zero;
            physicsBody->AngularVelocity = FPVector3.Zero;

            MarbleSystem.ResetMarblePosition(frame, info.Entity, marble);
        }
    }
}