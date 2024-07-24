using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    /// <summary>
    /// Meshを統合し、統合後のMeshのBlendShapeの変化をVRMのBlendShapeClipに反映する
    /// </summary>
    public class VrmBlendShapeUpdater
    {
        // BlendShapeBinding.RelativePath からの逆引き
        Dictionary<string, List<MeshIntegrationResult>> _rendererPathMap = new();
        GameObject _root;

        VrmBlendShapeUpdater(GameObject root, List<MeshIntegrationResult> results)
        {
            _root = root;
            foreach (var result in results)
            {
                foreach (var x in result.SourceSkinnedMeshRenderers)
                {
                    var srcPath = x.transform.RelativePathFrom(root.transform);
                    if (_rendererPathMap.TryGetValue(srcPath, out var value))
                    {
                        value.Add(result);
                    }
                    else
                    {
                        value = new List<MeshIntegrationResult>();
                        value.Add(result);
                        _rendererPathMap.Add(srcPath, value);
                    }
                }
            }
        }

        // 分割されて増える => 増えない BlendShape のある方にいく
        // 統合されて減る => 名前が同じものが統合される
        private IEnumerable<BlendShapeBinding> ReplaceBlendShapeBinding(IEnumerable<BlendShapeBinding> values)
        {
            var used = new HashSet<BlendShapeBinding>();
            foreach (var val in values)
            {
                if (_rendererPathMap.TryGetValue(val.RelativePath, out var results))
                {
                    foreach (var result in results)
                    {
                        if (result.Integrated == null)
                        {
                            continue;
                        }
                        var name = result.Integrated.Mesh.GetBlendShapeName(val.Index);
                        var newIndex = result.Integrated.Mesh.GetBlendShapeIndex(name);
                        if (newIndex == -1)
                        {
                            throw new KeyNotFoundException($"blendshape:{name} not found");
                        }

                        var dstPath = result.Integrated.IntegratedRenderer.transform.RelativePathFrom(_root.transform);
                        var binding = new BlendShapeBinding
                        {
                            RelativePath = dstPath,
                            Index = newIndex,
                            Weight = val.Weight,
                        };
                        if (used.Contains(binding))
                        {
                            Debug.LogWarning($"duplicated: {binding}");
                        }
                        else
                        {
#if VRM_DEVELOP                            
                            Debug.Log($"{val} >> {binding}");
#endif
                            used.Add(binding);
                            yield return binding;
                        }
                    }
                }
                else
                {
                    // skip
                    Debug.LogWarning($"SkinnedMeshRenderer not found: {val.RelativePath}");
                }
            }
        }

        public static List<BlendShapeClip> FollowBlendshapeRendererChange(string assetFolder,
            GameObject root,
            List<MeshIntegrationResult> results)
        {
            var clips = new List<BlendShapeClip>();
            if (root.TryGetComponent<VRMBlendShapeProxy>(out var proxy))
            {
                if (proxy.BlendShapeAvatar == null)
                {
                    return clips;
                }

                var util = new VrmBlendShapeUpdater(root, results);

                // create modified BlendShapeClip
                var clipAssetPathList = new List<string>();
                foreach (var src in proxy.BlendShapeAvatar.Clips.Where(x => x != null))
                {
                    var copy = util.RecreateBlendShapeClip(src, assetFolder);
                    var assetPath = $"{assetFolder}/{copy.name}.asset";
                    AssetDatabase.CreateAsset(copy, assetPath);
                    clipAssetPathList.Add(assetPath);
                    clips.Add(copy);
                }

                // create BlendShapeAvatar
                proxy.BlendShapeAvatar = RecreateBlendShapeAvatar(clips, assetFolder);

                return clips;
            }
            else
            {
                return clips;
            }
        }

        BlendShapeClip RecreateBlendShapeClip(BlendShapeClip src, string assetFolder)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }

            // copy
            var copy = ScriptableObject.CreateInstance<BlendShapeClip>();
            copy.CopyFrom(src);
            copy.Prefab = null;
            copy.Values = ReplaceBlendShapeBinding(copy.Values).ToArray();
            return copy;
        }

        static BlendShapeAvatar RecreateBlendShapeAvatar(IReadOnlyCollection<BlendShapeClip> clips, string assetFolder)
        {
            var copy = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            copy.Clips.AddRange(clips);
            var assetPath = $"{assetFolder}/blendshape.asset";
            AssetDatabase.CreateAsset(copy, assetPath);
            return copy;
        }
    }
}