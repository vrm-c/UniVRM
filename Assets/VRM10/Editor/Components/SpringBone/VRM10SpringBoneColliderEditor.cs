using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneCollider))]
    class VRM10SpringBoneColliderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (VRM10Window.Active == target)
            {
                GUI.backgroundColor = Color.cyan;
                Repaint();
            }
            base.OnInspectorGUI();
        }
    }
}
