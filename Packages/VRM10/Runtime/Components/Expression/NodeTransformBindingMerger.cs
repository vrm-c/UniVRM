using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;


namespace UniVRM10
{
    internal sealed class NodeTransformBindingMerger
    {
        IReadOnlyDictionary<ExpressionKey, VRM10Expression> _clipMap;
        Transform _root;
        IReadOnlyDictionary<Transform, TransformState> _initPose;
        Dictionary<ExpressionKey, float> _weightMap = new();

        public NodeTransformBindingMerger(
            IReadOnlyDictionary<ExpressionKey, VRM10Expression> clipMap,
            Transform root,
            IReadOnlyDictionary<Transform, TransformState> initPose)
        {
            _clipMap = clipMap;
            _root = root;
            _initPose = initPose;
        }

        public void AccumulateValue(ExpressionKey key, float value)
        {
            _weightMap[key] = value;
        }

        public void Apply()
        {
            foreach (var (k, weight) in _weightMap)
            {
                if (_clipMap.TryGetValue(k, out var clip))
                {
                    foreach (var b in clip.NodeTransformBindings)
                    {
                        var node = _root.GetFromPath(b.RelativePath);
                        if (node != null)
                        {
                            if (_initPose.TryGetValue(node, out var init))
                            {
                                b.Apply(node, init, weight);
                            }
                        }
                    }
                }
            }
        }
    }
}