using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public static class Coords
    {
        public static void Draw(Matrix4x4 m, float size)
        {
            Handles.matrix = m * Matrix4x4.Scale(new Vector3(size, size, size));

            // Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, size, EventType.Repaint);
            Handles.color = Color.red;
            Handles.DrawLine(Vector3.zero, Vector3.right);
            Handles.color = Color.green;
            Handles.DrawLine(Vector3.zero, Vector3.up);
            Handles.color = Color.blue;
            Handles.DrawLine(Vector3.zero, Vector3.forward);

            Handles.color = Color.white;
            // xy
            Handles.DrawLine(Vector3.right + Vector3.up, Vector3.right);
            Handles.DrawLine(Vector3.right + Vector3.up, Vector3.up);
            // yz
            Handles.DrawLine(Vector3.up + Vector3.forward, Vector3.forward);
            Handles.DrawLine(Vector3.up + Vector3.forward, Vector3.up);
            // zx
            Handles.DrawLine(Vector3.forward + Vector3.right, Vector3.forward);
            Handles.DrawLine(Vector3.forward + Vector3.right, Vector3.right);
        }
    }
}
