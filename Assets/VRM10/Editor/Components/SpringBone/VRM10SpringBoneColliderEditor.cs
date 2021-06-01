using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneCollider))]
    class VRM10SpringBoneColliderEditor : Editor
    {
        VRM10SpringBoneCollider _target;
        void OnEnable()
        {
            _target = target as VRM10SpringBoneCollider;
        }

        public override void OnInspectorGUI()
        {
            if (_target.GetInstanceID() == VRM10SpringBoneCollider.SelectedGuid)
            {
                GUI.backgroundColor = Color.red;
            }
            base.OnInspectorGUI();
        }
    }
}
