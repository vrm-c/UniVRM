using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniGLTF;
using UniGLTF.MeshUtility;
using UniGLTF.Utils;
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
        public static byte[] Export(
            GameObject exportRoot,
            VRMMetaObject meta,
            VRMExportSettings settings,
            IMaterialExporter materialExporter = null
        )
        {
            List<GameObject> destroy = new List<GameObject>();
            try
            {
                return Export(exportRoot, meta, settings, materialExporter, destroy);
            }
            finally
            {
                foreach (var x in destroy)
                {
                    // エラーを引き起こす場合がある
                    // Debug.LogFormat("destroy: {0}", x.name);
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
                if (clip == null)
                {
                    continue;
                }

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
            var copyBlendShapeAvatarClips = copyBlendShapeAvatar.Clips.Where(x => x != null).ToArray();
            var usedBlendshapeIndexArray = copyBlendShapeAvatarClips
                .SelectMany(clip => clip.Values)
                .Where(val => target.transform.Find(val.RelativePath) == smr.transform)
                .Select(val => val.Index)
                .Distinct()
                .ToArray();

            var copyMesh = mesh.Copy(copyBlendShape: false);
            // 使われている BlendShape だけをコピーする
            foreach (var i in usedBlendshapeIndexArray)
            {
                var name = mesh.GetBlendShapeName(i);
                var vCount = mesh.vertexCount;
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
            foreach (var clip in copyBlendShapeAvatarClips)
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

        static void ForceUniqueName(Transform transform, Dictionary<string, int> nameCount)
        {
            for (int i = 2; i < 5000; ++i)
            {
                var sb = new StringBuilder();
                sb.Append(transform.name);
                sb.Append('_');
                sb.Append(i);
                var newName = sb.ToString();
                if (!nameCount.ContainsKey(newName))
                {
                    Debug.LogWarningFormat("force rename {0} => {1}", transform.name, newName);
                    transform.name = newName;
                    nameCount.Add(newName, 1);
                    return;
                }
            }
            throw new Exception("?");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="settings"></param>
        /// <param name="destroy">作業が終わったらDestoryするべき一時オブジェクト</param>
        private static byte[] Export(
            GameObject exportRoot,
            VRMMetaObject meta,
            VRMExportSettings settings,
            IMaterialExporter materialExporter,
            List<GameObject> destroy)
        {
            var target = exportRoot;

            // 常にコピーする。シーンを変化させない
            target = GameObject.Instantiate(target);
            destroy.Add(target);

            var metaBehaviour = target.GetComponentOrNull<VRMMeta>();
            if (metaBehaviour == null)
            {
                metaBehaviour = target.AddComponent<VRMMeta>();
                metaBehaviour.Meta = meta;
            }
            if (metaBehaviour.Meta == null)
            {
                // 来ないはず
                throw new Exception("meta required");
            }

            {
                // copy元
                var getBone = UniHumanoid.Humanoid.Get_GetBoneTransform(exportRoot);
                var beforeTransforms = exportRoot.GetComponentsInChildren<Transform>(true);
                // copy先
                var afterTransforms = target.GetComponentsInChildren<Transform>(true);
                // copy先のhumanoidBoneのリストを得る
                var humanTransforms = CachedEnum.GetValues<HumanBodyBones>()
                    .Where(x => x != HumanBodyBones.LastBone)
                    .Select(x => getBone(x))
                    .Where(x => x != null)
                    .Select(x => afterTransforms[Array.IndexOf(beforeTransforms, x)]) // copy 先を得る
                    .ToArray();

                var nameCount = target.GetComponentsInChildren<Transform>()
                    .GroupBy(x => x.name)
                    .ToDictionary(x => x.Key, x => x.Count());
                foreach (var t in target.GetComponentsInChildren<Transform>())
                {
                    if (humanTransforms.Contains(t))
                    {
                        // keep original name
                        continue;
                    }

                    if (nameCount[t.name] > 1)
                    {
                        // 重複するボーン名をリネームする
                        ForceUniqueName(t, nameCount);
                    }
                }
            }

            if (settings.PoseFreeze)
            {
                using (var backup = new VrmGeometryBackup(target))
                {
	                // 正規化
	                VRMBoneNormalizer.Execute(target, settings.ForceTPose, settings.FreezeMeshUseCurrentBlendShapeWeight);
	            }
            }

            // 元のBlendShapeClipに変更を加えないように複製
            if (target.TryGetComponent<VRMBlendShapeProxy>(out var proxy))
            {
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
            }

            // 出力
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var data = new UniGLTF.ExportingGltfData();
            var gltfExportSettings = settings.GltfExportSettings;
            using (var exporter = new VRMExporter(data, gltfExportSettings,
                animationExporter: settings.KeepAnimation ? new EditorAnimationExporter() : null,
                materialExporter: materialExporter,
                textureSerializer: new EditorTextureSerializer()))
            {
                exporter.Prepare(target);
                exporter.Export();
            }
            var bytes = data.ToGlbBytes();
            Debug.LogFormat("Export elapsed {0}", sw.Elapsed);
            return bytes;
        }
    }
}
