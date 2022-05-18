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
        public static void FollowBlendshapeRendererChange(List<MeshIntegrationResult> results, GameObject root, string assetFolder)
        {
            var proxy = root.GetComponent<VRMBlendShapeProxy>();
            if (proxy == null || proxy.BlendShapeAvatar == null)
            {
                return;
            }
            var result = results.FirstOrDefault(x => x.SourceSkinnedMeshRenderers.Count > 0);
            if (result == null)
            {
                return;
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
            var clips = new List<BlendShapeClip>();
            foreach (var src in proxy.BlendShapeAvatar.Clips)
            {
                if (src == null) continue;

                // copy
                var clip = BlendShapeClip.Instantiate(src);

                // modify
                for (var i = 0; i < clip.Values.Length; ++i)
                {
                    var val = clip.Values[i];
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

                    clip.Values[i] = val;
                }

                // write
                var assetPath = $"{assetFolder}/{clip.name}.asset";
                sb.AppendLine($"write: {assetPath}");
                AssetDatabase.CreateAsset(clip, assetPath);

                clipAssetPathList.Add(assetPath);
                clips.Add(clip);
            }
            Debug.Log(sb.ToString());

            // reload
            // AssetDatabase.Refresh();
            // var clips = clipAssetPathList.Select(x => AssetDatabase.LoadAssetAtPath<BlendShapeClip>(x)).ToList();

            {
                // create blendshape avatar & replace
                var blendShapeAvatar = BlendShapeAvatar.CreateInstance<BlendShapeAvatar>();
                blendShapeAvatar.Clips.AddRange(clips);
                var assetPath = $"{assetFolder}/blendshape.asset";
                AssetDatabase.CreateAsset(blendShapeAvatar, assetPath);

                // reload
                // AssetDatabase.Refresh();
                // blendShapeAvatar = AssetDatabase.LoadAssetAtPath<BlendShapeAvatar>(assetPath);

                // assign
                proxy.BlendShapeAvatar = blendShapeAvatar;
            }
        }
    }
}