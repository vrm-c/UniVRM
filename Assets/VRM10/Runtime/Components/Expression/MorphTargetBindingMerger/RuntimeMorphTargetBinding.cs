using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    internal readonly struct RuntimeMorphTargetBinding
    {
        public MorphTargetIdentifier TargetIdentifier { get; }
        public Action<float> WeightApplier { get; }

        public RuntimeMorphTargetBinding(MorphTargetIdentifier targetIdentifier, Action<float> weightApplier)
        {
            TargetIdentifier = targetIdentifier;
            WeightApplier = weightApplier;
        }
    }
}