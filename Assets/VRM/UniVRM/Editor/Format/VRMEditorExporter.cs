using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public static class VRMEditorExporter
    {
        public static void Export(string path, VRMExportSettings settings)
        {
            List<GameObject> destroy = new List<GameObject>();
            try
            {
                Export(path, settings, destroy);
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

        static bool IsPrefab(GameObject go)
        {
            return !go.scene.IsValid();
        }


        static void Export(string path, VRMExportSettings settings, List<GameObject> destroy)
        {
            var target = settings.Source;
            if (IsPrefab(target))
            {
                using (new RecordDisposer(settings.Source.transform.Traverse().ToArray(), "before normalize"))
                {
                    target = GameObject.Instantiate(target);
                    destroy.Add(target);
                }
            }

            if (settings.PoseFreeze)
            {
                using (new RecordDisposer(target.transform.Traverse().ToArray(), "before normalize"))
                {
                    var normalized = BoneNormalizer.Execute(target, settings.ForceTPose, false);
                    RecordDisposer.CopyVRMComponents(target, normalized.Root, normalized.BoneMap);
                    target = normalized.Root;
                    destroy.Add(target);
                }
            }

            // remove unused blendShape
            if (settings.ReduceBlendshapeSize)
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
                        .Select((x, i) => new { x, i })
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
                var vrm = VRMExporter.Export(target, settings.ReduceBlendshapeSize);
                vrm.extensions.VRM.meta.title = settings.Title;
                vrm.extensions.VRM.meta.version = settings.Version;
                vrm.extensions.VRM.meta.author = settings.Author;
                vrm.extensions.VRM.meta.contactInformation = settings.ContactInformation;
                vrm.extensions.VRM.meta.reference = settings.Reference;

                var bytes = vrm.ToGlbBytes(settings.UseExperimentalExporter ? SerializerTypes.Generated : SerializerTypes.UniJSON);
                File.WriteAllBytes(path, bytes);
                Debug.LogFormat("Export elapsed {0}", sw.Elapsed);
            }

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
            }
        }
    }
}
