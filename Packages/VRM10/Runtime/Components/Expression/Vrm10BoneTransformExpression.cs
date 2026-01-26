using System;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    [DisallowMultipleComponent]
    public class Vrm10BoneTransformExpression : MonoBehaviour
    {
        [Serializable]
        public class BoneTransformExpression
        {
            public ExpressionPreset Preset = ExpressionPreset.custom;
            public string Name = "custom";
            public ExpressionKey ExpressionKey => new(Preset, Name);
            public Quaternion Rotation = Quaternion.identity;
            public Vector3 Translation = Vector3.zero;
        }

        public BoneTransformExpression Expression;
    }
}