using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VRM
{
    [Serializable]
    public class VRMExportSettings
    {
        public GameObject Source;

        public string Title;

        public string Author;

        public bool ForceTPose = true;

        public bool PoseFreeze = true;

        public bool UseExperimentalExporter = true;

        public bool RemoveUnusedBlendShapes = false;

    public IEnumerable<string> CanExport()
        {
            if (Source == null)
            {
                yield return "Require source";
                yield break;
            }

            var animator = Source.GetComponent<Animator>();
            if (animator == null)
            {
                yield return "Require animator. ";
            }
            else if (animator.avatar == null)
            {
                yield return "Require animator.avatar. ";
            }
            else if (!animator.avatar.isValid)
            {
                yield return "Animator.avatar is not valid. ";
            }
            else if (!animator.avatar.isHuman)
            {
                yield return "Animator.avatar is not humanoid. Please change model's AnimationType to humanoid. ";
            }

            if (string.IsNullOrEmpty(Title))
            {
                yield return "Require Title. ";
            }

            if (string.IsNullOrEmpty(Author))
            {
                yield return "Require Author. ";
            }
        }

        public void InitializeFrom(GameObject go)
        {
            if (Source == go) return;
            Source = go;

            var desc = Source == null ? null : go.GetComponent<VRMHumanoidDescription>();
            if (desc == null)
            {
                ForceTPose = true;
                PoseFreeze = true;
            }
            else
            {
                ForceTPose = false;
                PoseFreeze = false;
            }

            var meta = Source == null ? null : go.GetComponent<VRMMeta>();
            if (meta != null && meta.Meta != null)
            {
                Title = meta.Meta.Title;
                Author = meta.Meta.Author;
            }
            else
            {
                Title = go.name;
                //Author = "";
            }
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
                    // Copy VRMSpringBone
                    var dst = dstSecondary.gameObject.AddComponent<VRMSpringBone>();
                    dst.m_comment = src.m_comment;
                    dst.m_stiffnessForce = src.m_stiffnessForce;
                    dst.m_gravityPower = src.m_gravityPower;
                    dst.m_gravityDir = src.m_gravityDir;
                    dst.m_dragForce = src.m_dragForce;
                    if (src.m_center != null)
                    {
                        dst.m_center = map[src.m_center];
                    }
                    dst.RootBones = src.RootBones.Select(x => map[x]).ToList();
                    dst.m_hitRadius = src.m_hitRadius;
                    if (src.ColliderGroups != null)
                    {
                        dst.ColliderGroups = src.ColliderGroups.Select(x => map[x.transform].GetComponent<VRMSpringBoneColliderGroup>()).ToArray();
                    }
                }
            }

#pragma warning disable 0618
            {
                // meta(obsolete)
                var src = go.GetComponent<VRMMetaInformation>();
                if (src != null)
                {
                    src.CopyTo(root);
                }
            }
#pragma warning restore 0618

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

        public static bool IsPrefab(GameObject go)
        {
            return go.scene.name == null;
        }

#if UNITY_EDITOR
        public struct RecordDisposer : IDisposable
        {
            public RecordDisposer(UnityEngine.Object[] objects, string msg)
            {
                Undo.RecordObjects(objects, msg);
            }

            public void Dispose()
            {
                Undo.PerformUndo();
            }
        }

        public void Export(string path)
        {
            List<GameObject> destroy = new List<GameObject>();
            try
            {
                Export(path, destroy);
            }
            finally
            {
                foreach (var x in destroy)
                {
                    Debug.LogFormat("destroy: {0}", x.name);
                    GameObject.DestroyImmediate(x);
                }
            }
        }

        void Export(string path, List<GameObject> destroy)
        {
            var target = Source;
            if (IsPrefab(target))
            {
                using (new RecordDisposer(Source.transform.Traverse().ToArray(), "before normalize"))
                {
                    target = GameObject.Instantiate(target);
                    destroy.Add(target);
                }
            }
            if (PoseFreeze)
            {
                using (new RecordDisposer(target.transform.Traverse().ToArray(), "before normalize"))
                {
                    var normalized = BoneNormalizer.Execute(target, ForceTPose, false);
                    CopyVRMComponents(target, normalized.Root, normalized.BoneMap);
                    target = normalized.Root;
                    destroy.Add(target);
                }
            }

            // [SPORADIC-E] このあたりでどうにかしたい
            if(RemoveUnusedBlendShapes)
            {
                // 使っているblendshapeをすべて取得する
                // VRMで使うanimation clipをすべて取得して
                var proxy = target.GetComponent<VRMBlendShapeProxy>();
                int blendShapeClipSize = proxy.BlendShapeAvatar.Clips.Count();
                
                
                SkinnedMeshRenderer bodyobject = null;
                foreach(Transform child in target.transform)
                {
                    Debug.Log(child.name);
                    if(child.name=="Body") {bodyobject = child.GetComponent<SkinnedMeshRenderer>();}
                }

                Mesh body = bodyobject.sharedMesh;
                if(body == null)
                {
                    Debug.LogWarning("No object \"Body\" found in VRM game object !");
                    return;
                }

                bool[] isUsedInBlendShape = new bool[body.blendShapeCount];
                for (int i = 0; i < isUsedInBlendShape.Length; i++)
                {
                    isUsedInBlendShape[i] = false;
                }

                // それぞれのanimation clipの内容をすべて取得して利用しているものはリストに記録
                for (int i = 0; i < blendShapeClipSize; i++)
                {
                    var clip = proxy.BlendShapeAvatar.Clips[i];
                    for(int j = 0; j < clip.Values.Length; j++) {
                        if(clip.Values[j].RelativePath == "Body") isUsedInBlendShape[clip.Values[j].Index] = true;
                    }
                }
                
                #region showBlendShapeinfo
                {
                    //int tsize = 0;
                    //for (int i = 0; i < isUsedInBlendShape.Length; i++)
                    //{
                    //    Debug.LogFormat("{0} is in use:{1}", body.GetBlendShapeName(i), isUsedInBlendShape[i]);
                    //    if(isUsedInBlendShape[i])
                    //    {
                    //        tsize++;
                    //    }
                    //}
                    //Debug.LogFormat("{0} blendshapes in use from all {1} blendshapes", tsize, isUsedInBlendShape.Length);
                }
                #endregion

                // 複製したmeshに対して得られたリストを元に削除処理
                var copiedmesh = body.CopyWithSelectedBlendShape(isUsedInBlendShape);

                // ターゲットを挿げ替え
                bodyobject.sharedMesh = copiedmesh;
                

            }

            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var vrm = VRMExporter.Export(target);
                vrm.extensions.VRM.meta.title = Title;
                vrm.extensions.VRM.meta.author = Author;

                var bytes = vrm.ToGlbBytes(UseExperimentalExporter);
                File.WriteAllBytes(path, bytes);
                Debug.LogFormat("Export elapsed {0}", sw.Elapsed);
            }

            PrefabUtility.RevertPrefabInstance(target);

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
            }
        }
#endif
    }
}
