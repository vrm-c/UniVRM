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

        static void FromVrm10(Vrm10Instance instance)
        {
            Undo.RegisterCompleteObjectUndo(instance, "RegisterCompleteObjectUndo");
            foreach (var spring in instance.SpringBone.Springs)
            {
                if (spring.Joints == null || spring.Joints[0] == null)
                {
                    continue;
                }

                var root_joint = spring.Joints[0].gameObject;
                if (root_joint == null)
                {
                    continue;
                }

                if (root_joint.GetComponent<Warp>() != null)
                {
                    continue;
                }

                var warp = Undo.AddComponent<Warp>(root_joint);
                var joints = spring.Joints.Where(x => x != null).ToArray();
                for (int i = 0; i < joints.Length; ++i)
                {
                    var joint = joints[i];
                    var settings = new Warp.ParticleSettings
                    {
                        DragForce = joint.m_dragForce,
                        GravityDir = joint.m_gravityDir,
                        GravityPower = joint.m_gravityPower,
                        StiffnessForce = joint.m_stiffnessForce,
                    };
                    if (i == 0)
                    {
                        settings.HitRadius = joints[0].m_jointRadius;
                        warp.BaseSettings = settings;
                    }
                    else
                    {
                        // breaking change from vrm-1.0
                        settings.HitRadius = joints[i - 1].m_jointRadius;
                        var useInheritSettings = warp.BaseSettings.Equals(settings);
                        warp.Particles.Add(new Warp.Particle
                        {
                            useInheritSettings = useInheritSettings,
                            OverrideSettings = settings,
                            Transform = joint.transform,
                        });
                    }
                    Undo.DestroyObjectImmediate(joint);
                }
                spring.Joints.Clear();
            }
            instance.SpringBone.Springs.Clear();

            Undo.RegisterFullObjectHierarchyUndo(instance.gameObject, "RegisterFullObjectHierarchyUndo");

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

                    FromVrm10(instance);

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

            base.OnInspectorGUI();
        }
    }
}