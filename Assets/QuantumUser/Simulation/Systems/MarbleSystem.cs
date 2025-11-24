namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;
    using Quantum.Physics2D;
    using Quantum.Collections;

    [Preserve]
    public unsafe class MarbleSystem : SystemMainThreadFilter<MarbleSystem.Filter>, ISignalOnComponentAdded<Marble>
    {

        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public Marble* Marble;
            public Transform3D* Transform3D;
        }

        public override void Update(Frame frame, ref Filter filter)
        {

        }

        public unsafe void OnAdded(Frame frame, EntityRef entity, Marble* marble)
        {
            frame.Events.MarbleAdded(entity);
            if (!frame.Unsafe.TryGetPointer(entity, out Transform3D* transform3D))
                return;

            ResetMarblePosition(frame, entity, marble);
        }

        public static void ResetMarblePosition(Frame frame, EntityRef entity, Marble* marble)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out Transform3D* transform3D))
                return;

            transform3D->Position = new FPVector3(0, 50, 0);
        }
    }
}