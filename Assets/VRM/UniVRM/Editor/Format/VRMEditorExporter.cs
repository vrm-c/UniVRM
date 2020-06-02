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
        /// <summary>
        /// Editor向けのエクスポート処理
        /// </summary>
        /// <param name="path">出力先</param>
        /// <param name="settings">エクスポート設定</param>
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

        /// <summary>
        /// DeepCopy
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        static BlendShapeAvatar CopyBlendShapeAvatar(BlendShapeAvatar src, bool removeUnknown)
        {
            var avatar = GameObject.Instantiate(src);
            avatar.Clips = new List<BlendShapeClip>();
            foreach (var clip in src.Clips)
            {
                if (removeUnknown && clip.Preset == BlendShapePreset.Unknown)
                {
                    continue;
                }
                avatar.Clips.Add(GameObject.Instantiate(clip));
            }
            return avatar;
        }

        /// <summary>
        /// 使用されない BlendShape を間引いた Mesh を作成して置き換える
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        static void ReplaceMesh(GameObject target, SkinnedMeshRenderer smr, BlendShapeAvatar copyBlendShapeAvatar)
        {
            Mesh mesh = smr.sharedMesh;
            if (mesh == null) return;
            if (mesh.blendShapeCount == 0) return;

            // Mesh から BlendShapeClip からの参照がある blendShape の index を集める
            var usedBlendshapeIndexArray = copyBlendShapeAvatar.Clips
                .SelectMany(clip => clip.Values)
                .Where(val => target.transform.Find(val.RelativePath) == smr.transform)
                .Select(val => val.Index)
                .Distinct()
                .ToArray();

            var copyMesh = mesh.Copy(copyBlendShape: false);
            // 使われている BlendShape だけをコピーする
            foreach (var i in usedBlendshapeIndexArray)
            {
                var name = copyMesh.GetBlendShapeName(i);
                var vCount = copyMesh.vertexCount;
                var vertices = new Vector3[vCount];
                var normals = new Vector3[vCount];
                var tangents = new Vector3[vCount];
                mesh.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);
                copyMesh.AddBlendShapeFrame(name, 100f, vertices, normals, tangents);
            }

            // BlendShapeClip の BlendShapeIndex を更新する(前に詰める)
            var indexMapper = usedBlendshapeIndexArray
                .Select((x, i) => new { x, i })
                .ToDictionary(pair => pair.x, pair => pair.i);
            foreach (var clip in copyBlendShapeAvatar.Clips)
            {
                for (var i = 0; i < clip.Values.Length; ++i)
                {
                    var value = clip.Values[i];
                    if (target.transform.Find(value.RelativePath) != smr.transform) continue;
                    value.Index = indexMapper[value.Index];
                    clip.Values[i] = value;
                }
            }

            // mesh を置き換える
            smr.sharedMesh = copyMesh;
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

            // 正規化
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

            // 元のBlendShapeClipに変更を加えないように複製
            var proxy = target.GetComponent<VRMBlendShapeProxy>();
            var copyBlendShapeAvatar = CopyBlendShapeAvatar(proxy.BlendShapeAvatar, settings.ReduceBlendshapeClip);
            proxy.BlendShapeAvatar = copyBlendShapeAvatar;

            // BlendShape削減
            if (settings.ReduceBlendshape)
            {
                foreach (SkinnedMeshRenderer smr in target.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    // 未使用のBlendShapeを間引く
                    ReplaceMesh(target, smr, copyBlendShapeAvatar);
                }
            }

            // 出力
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var vrm = VRMExporter.Export(target, settings.ReduceBlendshape);
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
                // 出力ファイルのインポートを発動
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
            }
        }
    }
}
