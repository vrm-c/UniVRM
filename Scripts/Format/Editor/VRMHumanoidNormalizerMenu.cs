using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    public static class VRMHumanoidNorimalizerMenu
    {
        const string MENU_KEY = VRMVersion.VRM_VERSION+"/freeze T-Pose";
        [MenuItem(MENU_KEY, true, 1)]
        private static bool ExportValidate()
        {
            var root=Selection.activeObject as GameObject;
            if (root == null)
            {
                return false;
            }

            var animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                return false;
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                return false;
            }

            if (!avatar.isValid) {
                return false;
            }

            if (!avatar.isHuman)
            {
                return false;
            }

            return true;
        }

        [MenuItem(MENU_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;
            Undo.RecordObjects(go.transform.Traverse().ToArray(), "before normalize");
            var map = new Dictionary<Transform, Transform>();
            var normalized = VRM.BoneNormalizer.Execute(go, map, true);
            CopyVRMComponents(go, normalized, map);
            Undo.PerformUndo();
        }

        //
        // トップレベルのMonoBehaviourを移植する
        //
        public static void CopyVRMComponents(GameObject go, GameObject root, 
            Dictionary<Transform, Transform> map)
        {
            {
                // blendshape
                var src = go.GetComponent<VRMBlendShapeProxy>();
                if (src != null)
                {
                    var dst = root.AddComponent<VRMBlendShapeProxy>();
                    dst.BlendShapeAvatar = src.BlendShapeAvatar;
                }
            }

            {
                var secondary = go.transform.Find("secondary");
                if (secondary == null)
                {
                    secondary = go.transform;
                }

                var dstSecondary = root.transform.Find("secondary");
                if (dstSecondary == null)
                {
                    dstSecondary = new GameObject("secondary").transform;
                    dstSecondary.SetParent(root.transform, false);
                }

                // 揺れモノ
                foreach (var src in go.transform.Traverse().Select(x => x.GetComponent<VRMSpringBoneColliderGroup>()).Where(x => x != null))
                {
                    var dst = map[src.transform];
                    var dstColliderGroup = dst.gameObject.AddComponent<VRMSpringBoneColliderGroup>();
                    dstColliderGroup.Colliders = src.Colliders.Select(y =>
                    {
                        var offset = dst.worldToLocalMatrix.MultiplyPoint(src.transform.localToWorldMatrix.MultiplyPoint(y.Offset));
                        return new VRMSpringBoneColliderGroup.SphereCollider
                        {
                            Offset = offset,
                            Radius = y.Radius
                        };
                    }).ToArray();
                }

                foreach (var src in go.transform.Traverse().SelectMany(x => x.GetComponents<VRMSpringBone>()))
                {
                    var dst = dstSecondary.gameObject.AddComponent<VRMSpringBone>();
                    dst.m_comment = src.m_comment;
                    dst.RootBones = src.RootBones.Select(x => map[x]).ToList();
                    if (src.ColliderGroups != null)
                    {
                        dst.ColliderGroups = src.ColliderGroups.Select(x => map[x.transform].GetComponent<VRMSpringBoneColliderGroup>()).ToArray();
                    }
                }
            }

            {
                // meta(obsolete)
                var src = go.GetComponent<VRMMetaInformation>();
                if (src != null)
                {
                    src.CopyTo(root);
                }
            }
            {
                // meta
                var src = go.GetComponent<VRMMeta>();
                if (src != null)
                {
                    var dst = root.AddComponent<VRMMeta>();
                    dst.Meta = src.Meta;
                }
            }

            {
                // firstPerson
                var src = go.GetComponent<VRMFirstPerson>();
                if (src != null)
                {
                    src.CopyTo(root, map);
                }
            }

            {
                // lookAt
                var src = go.GetComponent<VRMLookAt>();
                if (src != null)
                {
                    src.CopyTo(root, map);
                }
            }

            {
                // humanoid
                var dst = root.AddComponent<VRMHumanoidDescription>();
                var src = go.GetComponent<VRMHumanoidDescription>();
                if (src != null)
                {
                    dst.Avatar = src.Avatar;
                    dst.Description = src.Description;
                }
                else
                {
                    var animator = go.GetComponent<Animator>();
                    if (animator != null)
                    {
                        dst.Avatar = animator.avatar;
                    }
                }
            }
        }
    }
}
