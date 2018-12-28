using UnityEngine;


namespace UniHumanoid
{
    public class BoneGizmoDrawer : MonoBehaviour
    {
        const float size = 0.03f;
        readonly Vector3 SIZE = new Vector3(size, size, size);

        [SerializeField]
        public bool Draw = true;

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Draw && transform.parent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(transform.position, SIZE);
                Gizmos.DrawLine(transform.parent.position, transform.position);

                UnityEditor.Handles.Label(transform.position, name);
            }
#endif
        }
    }
}
