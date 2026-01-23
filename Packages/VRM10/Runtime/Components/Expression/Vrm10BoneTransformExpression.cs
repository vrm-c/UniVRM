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
            public ExpressionPreset Preset;
            public EuclideanTransform Transformation = new EuclideanTransform(Quaternion.identity, Vector3.zero);
        }

        public BoneTransformExpression Expression;
    }
}