using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniGLTF;
using UniGLTF.MeshUtility;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// Meshを統合し、統合後のMeshのBlendShapeの変化をVRMのBlendShapeClipに反映する
    /// </summary>
    public static class VRMMeshIntegratorUtility
    {
        public static List<BlendShapeClip> FollowBlendshapeRendererChange(List<MeshIntegrationResult> results, GameObject root, string assetFolder)
        {
            var clips = new List<BlendShapeClip>();
            var proxy = root.GetComponent<VRMBlendShapeProxy>();
            if (proxy == null || proxy.BlendShapeAvatar == null)
            {
                return clips;
            }
            var result = results.FirstOrDefault(x => x.IntegratedRenderer.sharedMesh.blendShapeCount > 0);
            if (result == null)
            {
                return clips;
            }

            var rendererDict = new Dictionary<string, SkinnedMeshRenderer>();
            foreach (var x in result.SourceSkinnedMeshRenderers)
            {
                rendererDict.Add(x.transform.RelativePathFrom(root.transform), x);
            }
            var dstPath = result.IntegratedRenderer.transform.RelativePathFrom(root.transform);

            // copy modify and write
            var clipAssetPathList = new List<string>();
            var sb = new StringBuilder();
            foreach (var src in proxy.BlendShapeAvatar.Clips)
            {
                if (src == null) continue;

                // copy
                var copy = ScriptableObject.CreateInstance<BlendShapeClip>();
                copy.CopyFrom(src);
                copy.Prefab = null;

                // modify
                for (var i = 0; i < copy.Values.Length; ++i)
                {
                    var val = copy.Values[i];
                    if (rendererDict.ContainsKey(val.RelativePath))
                    {
                        var srcRenderer = rendererDict[val.RelativePath];
                        var name = srcRenderer.sharedMesh.GetBlendShapeName(val.Index);
                        var newIndex = result.IntegratedRenderer.sharedMesh.GetBlendShapeIndex(name);
                        if (newIndex == -1)
                        {
                            throw new KeyNotFoundException($"blendshape:{name} not found");
                        }

                        val.RelativePath = dstPath;
                        val.Index = newIndex;
                    }
                    copy.Values[i] = val;
                }

                // write
                var assetPath = $"{assetFolder}/{copy.name}.asset";
                sb.AppendLine($"write: {assetPath}");
                AssetDatabase.CreateAsset(copy, assetPath);

                clipAssetPathList.Add(assetPath);
                clips.Add(copy);
            }
            Debug.Log(sb.ToString());

            {
                // create blendshape avatar & replace
                var copy = ScriptableObject.CreateInstance<BlendShapeAvatar>();
                copy.Clips.AddRange(clips);
                var assetPath = $"{assetFolder}/blendshape.asset";
                AssetDatabase.CreateAsset(copy, assetPath);

                // assign
                proxy.BlendShapeAvatar = copy;
            }

            return clips;
        }
    }
}