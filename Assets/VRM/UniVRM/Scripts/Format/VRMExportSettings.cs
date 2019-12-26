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

        public string Version;

        public string Author;

        public string ContactInformation;

        public string Reference;

        public bool ForceTPose = true;

        public bool PoseFreeze = true;

        public bool UseExperimentalExporter = false;

        public bool ReduceBlendshapeSize = false;

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
            if (string.IsNullOrEmpty(Version))
            {
                yield return "Require Version. ";
            }
            if (string.IsNullOrEmpty(Author))
            {
                yield return "Require Author. ";
            }

            if(ReduceBlendshapeSize && Source.GetComponent<VRMBlendShapeProxy>() == null)
            {
                yield return "ReduceBlendshapeSize is need VRMBlendShapeProxy, you need to convert to VRM once.";
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
                Version = string.IsNullOrEmpty(meta.Meta.Version)? "0.0" : meta.Meta.Version;
                Author = meta.Meta.Author;
                ContactInformation = meta.Meta.ContactInformation;
                Reference = meta.Meta.Reference;
            }
            else
            {
                Title = go.name;
                Version = "0.0";
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
                        var offset =dst.worldToLocalMatrix.MultiplyPoint(src.transform.localToWorldMatrix.MultiplyPoint(y.Offset));
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
                        dst.ColliderGroups = src.ColliderGroups
                            .Select(x => map[x.transform].GetComponent<VRMSpringBoneColliderGroup>()).ToArray();
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
            return !go.scene.IsValid();
        }

#if UNITY_EDITOR
        public struct RecordDisposer : IDisposable
        {
            int _group;
            public RecordDisposer(UnityEngine.Object[] objects, string msg)
            {
                Undo.IncrementCurrentGroup();
                _group = Undo.GetCurrentGroup();
                Undo.RecordObjects(objects, msg);
            }

            public void Dispose()
            {
                Undo.RevertAllDownToGroup(_group);
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

             // remove unused blendShape
            if (ReduceBlendshapeSize)
            {
                var proxy = target.GetComponent<VRMBlendShapeProxy>();

                // 元のBlendShapeClipに変更を加えないように複製
                var copyBlendShapeAvatar = GameObject.Instantiate(proxy.BlendShapeAvatar);
                var copyBlendShapClips = new List<BlendShapeClip>();

                foreach (var clip in proxy.BlendShapeAvatar.Clips)
                {
                    copyBlendShapClips.Add(GameObject.Instantiate(clip));
                }

                var skinnedMeshRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();

                var names = new Dictionary<int, string>();
                var vs = new Dictionary<int, Vector3[]>();
                var ns = new Dictionary<int, Vector3[]>();
                var ts = new Dictionary<int, Vector3[]>();

                foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
                {
                    Mesh mesh = smr.sharedMesh;
                    if (mesh == null) continue;
                    if (mesh.blendShapeCount == 0) continue;

                    var copyMesh = mesh.Copy(true);
                    var vCount = copyMesh.vertexCount;
                    names.Clear();

                    vs.Clear();
                    ns.Clear();
                    ts.Clear();

                    var usedBlendshapeIndexArray = copyBlendShapClips
                        .SelectMany(clip => clip.Values)
                        .Where(val => target.transform.Find(val.RelativePath) == smr.transform)
                        .Select(val => val.Index)
                        .Distinct()
                        .ToArray();

                    foreach (var i in usedBlendshapeIndexArray)
                    {
                        var name = copyMesh.GetBlendShapeName(i);
                        var vertices = new Vector3[vCount];
                        var normals = new Vector3[vCount];
                        var tangents = new Vector3[vCount];
                        copyMesh.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);

                        names.Add(i, name);
                        vs.Add(i, vertices);
                        ns.Add(i, normals);
                        ts.Add(i, tangents);
                    }

                    copyMesh.ClearBlendShapes();

                    foreach (var i in usedBlendshapeIndexArray)
                    {
                        copyMesh.AddBlendShapeFrame(names[i], 100f, vs[i], ns[i], ts[i]);
                    }

                    var indexMapper = usedBlendshapeIndexArray
                        .Select((x, i) => new {x, i})
                        .ToDictionary(pair => pair.x, pair => pair.i);

                    foreach (var clip in copyBlendShapClips)
                    {
                        for (var i = 0; i < clip.Values.Length; ++i)
                        {
                            var value = clip.Values[i];
                            if (target.transform.Find(value.RelativePath) != smr.transform) continue;
                            value.Index = indexMapper[value.Index];
                            clip.Values[i] = value;
                        }
                    }

                    copyBlendShapeAvatar.Clips = copyBlendShapClips;

                    proxy.BlendShapeAvatar = copyBlendShapeAvatar;

                    smr.sharedMesh = copyMesh;
                }
            }

            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var vrm = VRMExporter.Export(target, ReduceBlendshapeSize);
                vrm.extensions.VRM.meta.title = Title;
                vrm.extensions.VRM.meta.version = Version;
                vrm.extensions.VRM.meta.author = Author;
                vrm.extensions.VRM.meta.contactInformation = ContactInformation;
                vrm.extensions.VRM.meta.reference = Reference;


                var bytes = vrm.ToGlbBytes(UseExperimentalExporter?SerializerTypes.Generated:SerializerTypes.UniJSON);
                File.WriteAllBytes(path, bytes);
                Debug.LogFormat("Export elapsed {0}", sw.Elapsed);
            }


            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
            }
        }
#endif
    }
}