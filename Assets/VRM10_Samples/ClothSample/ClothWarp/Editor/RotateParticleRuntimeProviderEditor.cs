using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;


namespace UniVRM10.ClothWarp.Components
{
    [CustomEditor(typeof(ClothWarpRuntimeProvider))]
    public class RotateParticleRuntimeProviderEditor : Editor
    {
        const string FROM_VRM10_MENU = "Replace VRM10 Springs to ClothWarp Warps";

        ClothWarpRuntimeProvider _target;
        Vrm10Instance _vrm;

        void OnEnable()
        {
            _target = (ClothWarpRuntimeProvider)target;
            if (_target != null)
            {
                _vrm = _target.GetComponent<Vrm10Instance>();
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Bind(serializedObject);
            {
                var s = new PropertyField { bindingPath = "m_Script" };
                s.SetEnabled(false);
                root.Add(s);
            }
            root.Add(new PropertyField { bindingPath = nameof(_target.Warps) });
            root.Add(new PropertyField { bindingPath = nameof(_target.Cloths) });

            {
                var setup = new Foldout { text = "Setup" };

                var from_vrm10 = new Button { text = "Replace VRM10 Springs to ClothWarp Warps" };
                setup.Add(from_vrm10);
                from_vrm10.RegisterCallback<ClickEvent>(e =>
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName(FROM_VRM10_MENU);
                    var undo = Undo.GetCurrentGroup();

                    Undo.RegisterCompleteObjectUndo(_vrm, "RegisterCompleteObjectUndo");
                    ClothWarpRuntimeProvider.FromVrm10(_vrm, Undo.AddComponent<ClothWarpRoot>, Undo.DestroyObjectImmediate);
                    Undo.RegisterFullObjectHierarchyUndo(_vrm.gameObject, "RegisterFullObjectHierarchyUndo");

                    Undo.RegisterCompleteObjectUndo(_target, "RegisterCompleteObjectUndo");
                    _target.Reset();

                    Undo.CollapseUndoOperations(undo);
                });

                var reload = new Button { text = "Reload" };
                setup.Add(reload);
                reload.RegisterCallback<ClickEvent>(e =>
                {
                    _target.Reset();
                });

                root.Add(setup);
            }

            {
                // runtime: reset button
                var runtime = new Foldout { text = "Runtime" };
                root.Add(runtime);

                var button = new Button
                {
                    text = "RestoreInitialTransform",
                };
                runtime.Add(button);
                button.RegisterCallback<ClickEvent>((e) =>
                {
                    _vrm.Runtime.SpringBone.RestoreInitialTransform();
                });
            }

            return root;
        }
    }
}