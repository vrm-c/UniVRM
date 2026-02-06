using System;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public struct NodeTransformBinding
    {
        public string RelativePath;

        /// <summary>
        /// t = init_t + offset_t * weight
        /// disable if offset_t = (0, 0, 0)
        /// </summary>
        public Vector3 OffsetTranslation;

        /// <summary>
        /// r = slerp(init_t, init_t * offset r, weight)
        /// disable if rotation_t = (0, 0, 0, 1)
        /// </summary>
        public Quaternion OffsetRotation;

        /// <summary>
        /// s = lerp(init_s, blend_s, weight)
        /// disalbe if blend_s = init_s. maybe(1, 1, 1)
        /// </summary>
        public Vector3 TargetScale;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="index"></param>
        /// <param name="weight">0 to 1.0</param>
        public NodeTransformBinding(string path, in Vector3 t, in Quaternion r, in Vector3 s)
        {
            RelativePath = path;
            OffsetTranslation = t;
            OffsetRotation = r;
            TargetScale = s;
        }

        public void Apply(Transform node, in TransformState init, float weight)
        {
            node.SetLocalPositionAndRotation(
                init.LocalPosition + this.OffsetTranslation * weight,
                Quaternion.Slerp(init.LocalRotation, init.LocalRotation * this.OffsetRotation, weight)
            );
            node.localScale = Vector3.Lerp(init.LocalScale, this.TargetScale, weight);
        }
    }
}