using Quantum;
using UnityEngine;

public unsafe class PinballDebugVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    public float axisLength = 0.5f;           // Length of XYZ axes
    public float angularVelocityScale = 0.02f; // How big the angular vel arrow is (tune!)
    public bool showAxes = true;
    public bool showAngularVelocity = true;

    [Header("Colors")]
    public Color xColor = Color.red;
    public Color yColor = Color.green;
    public Color zColor = Color.blue;
    public Color angVelColor = new Color(1f, 0.7f, 0f); // Orange

    public QuantumEntityView PinballEntityView;

    void Update()
    {
        if (!QuantumRunner.Default)
            return;

        var frame = QuantumRunner.Default.Game.Frames.Predicted;

        // Try to get angular velocity from Quantum view (preferred)
        Vector3 angularVelocity = Vector3.zero;

        if (PinballEntityView != null && PinballEntityView != null)
        {
            if (frame.Unsafe.TryGetPointer(PinballEntityView.EntityRef, out PhysicsBody3D* physicsBody))
            {
                angularVelocity = new Vector3(
                    physicsBody->AngularVelocity.X.AsFloat,
                    physicsBody->AngularVelocity.Y.AsFloat,
                    physicsBody->AngularVelocity.Z.AsFloat
                );
            }
        }

        DrawDebugLines();

        void DrawDebugLines()
        {
            // Draw local axes (world-space)
            if (showAxes)
            {
                Debug.DrawLine(transform.position + -transform.right * axisLength, transform.position + transform.right * axisLength, xColor); // X = Red
                Debug.DrawLine(transform.position + -transform.up * axisLength, transform.position + transform.up * axisLength, yColor); // Y = Green
                Debug.DrawLine(transform.position + -transform.forward * axisLength, transform.position + transform.forward * axisLength, zColor); // Z = Blue
            }

            // Draw angular velocity vector (scaled)
            if (showAngularVelocity && angularVelocity.sqrMagnitude > 0.01f)
            {
                Vector3 angVelWorld = transform.TransformVector(angularVelocity); // Local → World
                Vector3 end = transform.position + angVelWorld * angularVelocityScale;

                // Main arrow shaft
                Debug.DrawLine(transform.position, end, angVelColor);

                // Arrowhead
                Vector3 dir = (end - transform.position).normalized;
                Vector3 perp = Vector3.Cross(dir, Vector3.up);
                if (perp.sqrMagnitude < 0.1f) perp = Vector3.Cross(dir, Vector3.forward);

                perp = perp.normalized * 0.1f;
                Debug.DrawLine(end, end - dir * 0.15f + perp * 0.08f, angVelColor);
                Debug.DrawLine(end, end - dir * 0.15f - perp * 0.08f, angVelColor);
            }
        }
    }

    

}