using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.MeshUtility;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    public class Vrm10ExpressionUpdater
    {
        // BlendShapeBinding.RelativePath からの逆引き
        Dictionary<string, List<MeshIntegrationResult>> _rendererPathMap = new();
        GameObject _root;

        Vrm10ExpressionUpdater(GameObject root, List<MeshIntegrationResult> results)
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
        IEnumerable<MorphTargetBinding> ReplaceBlendShapeBinding(IEnumerable<MorphTargetBinding> values)
        {
            var used = new HashSet<MorphTargetBinding>();
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
                        var binding = new MorphTargetBinding
                        {
                            RelativePath = dstPath,
                            Index = newIndex,
                            Weight = val.Weight,
                        };
                        if (used.Contains(binding))
                        {
                            UniGLTFLogger.Warning($"duplicated: {binding}");
                        }
                        else
                        {
                            if (Symbols.VRM_DEVELOP)
                            {
                                UniGLTFLogger.Log($"{val} >> {binding}");
                            }
                            used.Add(binding);
                            yield return binding;
                        }
                    }
                }
                else
                {
                    // skip
                    UniGLTFLogger.Warning($"SkinnedMeshRenderer not found: {val.RelativePath}");
                }
            }
        }

        public static Dictionary<VRM10Expression, VRM10Expression> Update(string assetFolder, GameObject instance,
            List<MeshIntegrationResult> results)
        {
            var vrm = instance.GetComponentOrThrow<Vrm10Instance>();
            var util = new Vrm10ExpressionUpdater(instance, results);

            // write Vrm10Expressions
            var copyMap = new Dictionary<VRM10Expression, VRM10Expression>();
            foreach (var (preset, clip) in vrm.Vrm.Expression.Clips)
            {
                var copy = ScriptableObject.Instantiate(clip);
                copy.MorphTargetBindings = util.ReplaceBlendShapeBinding(clip.MorphTargetBindings).ToArray();
                var assetPath = $"{assetFolder}/{copy.name}.asset";
                AssetDatabase.CreateAsset(copy, assetPath);
                copyMap.Add(clip, copy);
            }

            // write Vrm10Object
            {
                var copy = ScriptableObject.Instantiate<VRM10Object>(vrm.Vrm);
                var assetPath = $"{assetFolder}/{copy.name}.asset";
                copy.Expression.Replace(copyMap);
                AssetDatabase.CreateAsset(copy, assetPath);
                vrm.Vrm = copy;
            }

            return copyMap;
        }
    }
}