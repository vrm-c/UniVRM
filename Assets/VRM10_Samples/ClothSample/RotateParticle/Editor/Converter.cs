using System.Linq;
using UnityEditor;
using UnityEngine;
using UniVRM10;


namespace RotateParticle.Components
{
    public static class Converter
    {
        const string FROM_VRM10_MENU = "GameObject/CreateRotateParticleFromVrm10";

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

        /// <summary>
        /// instance.SpringBone.Springs を rotate particle で置き換える。
        /// UNDO 可能。
        /// </summary>
        [MenuItem(FROM_VRM10_MENU, false)]
        public static void FromVrm10()
        {
            var go = Selection.activeGameObject;
            var instance = go.GetComponent<Vrm10Instance>();

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(FROM_VRM10_MENU);
            var undo = Undo.GetCurrentGroup();

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
                    warp.Particles.Add(new Warp.Particle
                    {
                        OverrideSettings = new Warp.ParticleSettings
                        {
                            DragForce = joint.m_dragForce,
                            GravityDir = joint.m_gravityDir,
                            GravityPower = joint.m_gravityPower,
                            StiffnessForce = joint.m_stiffnessForce,
                            // breaking change from vrm-1.0
                            HitRadius = i > 0 ? joints[i - 1].m_jointRadius : joints[i].m_jointRadius,
                        },
                    });
                    Undo.DestroyObjectImmediate(joint);
                }
                spring.Joints.Clear();
            }
            instance.SpringBone.Springs.Clear();

            Undo.RegisterFullObjectHierarchyUndo(go, "RegisterFullObjectHierarchyUndo");

            Undo.CollapseUndoOperations(undo);
        }
    }
}