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
        public static bool IntegrateRuntime(GameObject vrmRootObject)
        {
            if (vrmRootObject == null) return false;
            var proxy = vrmRootObject.GetComponent<VRMBlendShapeProxy>();
            if (proxy == null) return false;
            var avatar = proxy.BlendShapeAvatar;
            if (avatar == null) return false;
            var clips = avatar.Clips;

            var results = Integrate(vrmRootObject, clips, null);
            if (results.Any(x => x.IntegratedRenderer == null)) return false;

            foreach (var result in results)
            {
                foreach (var renderer in result.SourceSkinnedMeshRenderers)
                {
                    Object.Destroy(renderer);
                }

                foreach (var renderer in result.SourceMeshRenderers)
                {
                    Object.Destroy(renderer);
                }
            }

            return true;
        }

        public static List<UniGLTF.MeshUtility.MeshIntegrationResult> Integrate(GameObject root, List<BlendShapeClip> blendshapeClips, IEnumerable<Mesh> excludes)
        {
            var result = new List<UniGLTF.MeshUtility.MeshIntegrationResult>();

            var withoutBlendShape = MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: false, excludes: excludes);
            if (withoutBlendShape.IntegratedRenderer != null)
            {
                result.Add(withoutBlendShape);
            }

            var onlyBlendShape = MeshIntegratorUtility.Integrate(root, onlyBlendShapeRenderers: true, excludes: excludes);
            if (onlyBlendShape.IntegratedRenderer != null)
            {
                result.Add(onlyBlendShape);
                FollowBlendshapeRendererChange(blendshapeClips, onlyBlendShape, root);
            }

            return result;
        }

        private static void FollowBlendshapeRendererChange(List<BlendShapeClip> clips, MeshIntegrationResult result, GameObject root)
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