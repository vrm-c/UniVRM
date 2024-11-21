using System.Linq;
using UnityEditor;
using UnityEngine;
using UniVRM10;


namespace RotateParticle.Components
{
    [CustomEditor(typeof(RotateParticleRuntimeProvider))]
    public class RotateParticleRuntimeProviderEditor : Editor
    {
        const string FROM_VRM10_MENU = "Replace VRM10 Springs to RotateParticle Warps";

        [MenuItem(FROM_VRM10_MENU, true)]
        public static bool IsFromVrm10()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                return false;
            }
            return go.GetComponent<Vrm10Instance>() != null;
        }

        public override void OnInspectorGUI()
        {
            var provider = target as RotateParticleRuntimeProvider;
            if (provider == null)
            {
                return;
            }
            var instance = provider.GetComponent<Vrm10Instance>();
            using (new EditorGUI.DisabledScope(instance == null))
            {
                if (GUILayout.Button("Replace VRM10 Springs to RotateParticle Warps"))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName(FROM_VRM10_MENU);
                    var undo = Undo.GetCurrentGroup();

                    Undo.RegisterCompleteObjectUndo(instance, "RegisterCompleteObjectUndo");
                    RotateParticleRuntimeProvider.FromVrm10(instance, Undo.AddComponent<WarpRoot>, Undo.DestroyObjectImmediate);
                    Undo.RegisterFullObjectHierarchyUndo(instance.gameObject, "RegisterFullObjectHierarchyUndo");

                    Undo.RegisterCompleteObjectUndo(provider, "RegisterCompleteObjectUndo");
                    provider.Reset();

                    Undo.CollapseUndoOperations(undo);
                }
            }

            using (new EditorGUI.DisabledScope(instance == null || !Application.isPlaying))
            {
                if (GUILayout.Button("RestoreInitialTransform"))
                {
                    instance.Runtime.SpringBone.RestoreInitialTransform();
                }
            }

            if (GUILayout.Button("Reset"))
            {
                provider.Reset();
            }

            base.OnInspectorGUI();
        }
    }
}