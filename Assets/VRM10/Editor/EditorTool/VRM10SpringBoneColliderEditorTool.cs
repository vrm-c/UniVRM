using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace UniVRM10
{
    [EditorTool("vrm-1.0/SpringBoneCollider", typeof(UniVRM10.VRM10SpringBoneCollider))]
    class VRM10SpringBoneColliderEditorTool : EditorTool
    {
        static GUIContent s_cachedIcon;
        public override GUIContent toolbarIcon
        {
            get
            {
                if (s_cachedIcon == null)
                {
                    s_cachedIcon = EditorGUIUtility.IconContent("d_editcollision_32", "|vrm-1.0 SpringBoneCollider");
                }
                return s_cachedIcon;
            }
        }

        void OnEnable()
        {
            ToolManager.activeToolChanged += ActiveToolDidChange;
        }

        void OnDisable()
        {
            ToolManager.activeToolChanged -= ActiveToolDidChange;
        }

        void ActiveToolDidChange()
        {
            if (!ToolManager.IsActiveTool(this))
            {
                return;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Selection.activeTransform == null)
            {
                return;
            }
            var collider = Selection.activeTransform.GetComponent<VRM10SpringBoneCollider>();
            if (collider == null)
            {
                return;
            }

            Handles.matrix = collider.transform.localToWorldMatrix;
            {
                Handles.Label(collider.Offset, "Head");
                EditorGUI.BeginChangeCheck();
                var offset = Handles.PositionHandle(collider.Offset, Quaternion.identity);
                var offsetChanged = EditorGUI.EndChangeCheck();
                if (offsetChanged)
                {
                    Undo.RecordObject(collider, "SpringBoneCollider");

                    var delta = offset - collider.Offset;
                    collider.Offset = offset;
                    if (collider.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
                    {
                        collider.Tail += delta;
                    }
                }

                if (collider.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
                {
                    Handles.Label(collider.Tail, "Tail");
                    EditorGUI.BeginChangeCheck();
                    var tail = Handles.PositionHandle(collider.Tail, Quaternion.identity);
                    var tailChanged = EditorGUI.EndChangeCheck();
                    if (tailChanged)
                    {
                        Undo.RecordObject(collider, "SpringBoneCollider");
                        collider.Tail = tail;
                    }
                }
            }
        }
    }
}