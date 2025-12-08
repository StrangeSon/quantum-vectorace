namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class FlipperSystem : SystemMainThreadFilter<FlipperSystem.Filter>, ISignalOnComponentAdded<Flipper>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Flipper* Flipper;
            public Transform3D* Transform;
            public PhysicsBody3D* PhysicsBody;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var flipper = filter.Flipper;
            var transform = filter.Transform;
            var physicsBody = filter.PhysicsBody;
            if (!flipper->Owner.IsValid)
                return;

            var input = frame.GetPlayerInput(flipper->Owner);

            bool isFlipping = flipper->IsLeft ? input->LeftFlipper.IsDown : input->RightFlipper.IsDown;
            var oldProgress = flipper->FlipProgress;

            if (isFlipping)
                flipper->FlipProgress += flipper->FlipSpeed * frame.DeltaTime;
            else
                flipper->FlipProgress -= flipper->ReturnSpeed * frame.DeltaTime;

            flipper->FlipProgress = FPMath.Clamp01(flipper->FlipProgress);

            // Compute delta for angular velocity (in degrees initially)
            var deltaProgress = flipper->FlipProgress - oldProgress;
            var deltaAngleDeg = deltaProgress * flipper->MaxAngle;

            // Convert to rad/s for physics
            var angularVelMag = (deltaAngleDeg * FP.Deg2Rad) / frame.DeltaTime;

            // Set angular velocity along the axis (handles direction via sign)
            physicsBody->AngularVelocity = angularVelMag * flipper->RotationAxis;

            // Proceed with rotation update
            var angle = flipper->FlipProgress * flipper->MaxAngle;
            var deltaQuat = FPQuaternion.AngleAxis(angle, flipper->RotationAxis);
            transform->Rotation = deltaQuat * flipper->RestRotation;
        }

        public void OnAdded(Frame frame, EntityRef entity, Flipper* flipper)
        {
            if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
                flipper->RestRotation = transform->Rotation;

            flipper->FlipProgress = 0;
            flipper->MaxAngle = 52;
            flipper->FlipSpeed = 50;
            flipper->ReturnSpeed = 20;
            if (flipper->IsLeft)
                flipper->RotationAxis = transform->Down.Normalized;
            else
                flipper->RotationAxis = transform->Up.Normalized;
        }
    }
}