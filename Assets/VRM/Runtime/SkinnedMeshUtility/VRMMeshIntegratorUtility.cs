using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// Meshを統合し、統合後のMeshのBlendShapeの変化をVRMのBlendShapeClipに反映する
    /// </summary>
    public static class VRMMeshIntegratorUtility
    {
        public static void FollowBlendshapeRendererChange(List<BlendShapeClip> clips, MeshIntegrationResult result, GameObject root)
        {
            if (clips == null || result == null || result.IntegratedRenderer == null || root == null) return;

            var rendererDict = result.SourceSkinnedMeshRenderers
                .ToDictionary(x => x.transform.RelativePathFrom(root.transform), x => x);

            var dstPath = result.IntegratedRenderer.transform.RelativePathFrom(root.transform);

            foreach (var clip in clips)
            {
                if (clip == null) continue;

                for (var i = 0; i < clip.Values.Length; ++i)
                {
                    var val = clip.Values[i];
                    if (rendererDict.ContainsKey(val.RelativePath))
                    {
                        var srcRenderer = rendererDict[val.RelativePath];
                        var name = srcRenderer.sharedMesh.GetBlendShapeName(val.Index);
                        var newIndex = result.IntegratedRenderer.sharedMesh.GetBlendShapeIndex(name);

                        val.RelativePath = dstPath;
                        val.Index = newIndex;
                    }

                    clip.Values[i] = val;
                }
            }
        }
    }
}