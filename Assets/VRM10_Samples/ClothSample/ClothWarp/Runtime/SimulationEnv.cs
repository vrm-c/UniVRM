using UnityEngine;

namespace UniVRM10.ClothWarp
{
    [System.Serializable]
    public class SimulationEnv
    {
        public Transform Center;

        [Range(0, 1)]
        public float Stiffness = 0.1f;

        [Range(0, 1)]
        public float DragForce = 0.4f;

        public Vector3 External = new Vector3(0, -0.001f, 0);
    }
}
