using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// XR_EXT_hand_tracking の joint を VRM-1.0 の TPose に当てはめたときの方向を定義します。
    /// 
    /// * https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#_conventions_of_hand_joints
    /// * https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_vrm-1.0/tpose.ja.md
    /// 
    /// OpenXR は右手系なのに対して Unityは左手系です。
    /// この Rig が期待するボーンの値は、XR_EXT_hand_tracking の Joint の値を Z軸反転で座標変換したものです。
    /// </summary>
    public static class XR_EXT_hand_tracking
    {
        /// <summary>
        /// up vector と forward vector の外積により空間を算出して、回転を得ます。
        /// </summary>
        /// <param name="up"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        public static Quaternion GetRotation(Vector3 up, Vector3 forward)
        {
            var xAxis = Vector3.Cross(up, forward).normalized;
            var m = new Matrix4x4(xAxis, up, forward, new Vector4(0, 0, 0, 1));
            return m.rotation;
        }

        public static Quaternion LeftHand = GetRotation(Vector3.up, Vector3.left);
        public static Quaternion RightHand = GetRotation(Vector3.up, Vector3.right);

        /// <summary>
        /// 親指は XZ 平面45度です。
        /// </summary>
        public static Quaternion LeftThumb = GetRotation((Vector3.forward + Vector3.right).normalized, (Vector3.left + Vector3.forward).normalized);

        /// <summary>
        /// 親指は XZ 平面45度です。
        /// </summary>
        public static Quaternion RightThumb = GetRotation((Vector3.forward + Vector3.left).normalized, (Vector3.right + Vector3.forward).normalized);

        /// <summary>
        /// VRM-1.0 の T-Pose の定義から各指はX軸と並行です。親指はXZ平面に45度です。
        /// </summary>
        public static IReadOnlyDictionary<HumanBodyBones, Quaternion> InitialRotations => new Dictionary<HumanBodyBones, Quaternion>()
        {
            // Left
            {HumanBodyBones.LeftHand, LeftHand},
            {HumanBodyBones.LeftThumbProximal, LeftThumb},
            {HumanBodyBones.LeftThumbIntermediate, LeftThumb},
            {HumanBodyBones.LeftThumbDistal, LeftThumb},
            {HumanBodyBones.LeftIndexProximal, LeftHand},
            {HumanBodyBones.LeftIndexIntermediate, LeftHand},
            {HumanBodyBones.LeftIndexDistal, LeftHand},
            {HumanBodyBones.LeftMiddleProximal, LeftHand},
            {HumanBodyBones.LeftMiddleIntermediate, LeftHand},
            {HumanBodyBones.LeftMiddleDistal, LeftHand},
            {HumanBodyBones.LeftRingProximal, LeftHand},
            {HumanBodyBones.LeftRingIntermediate, LeftHand},
            {HumanBodyBones.LeftRingDistal, LeftHand},
            {HumanBodyBones.LeftLittleProximal, LeftHand},
            {HumanBodyBones.LeftLittleIntermediate, LeftHand},
            {HumanBodyBones.LeftLittleDistal, LeftHand},
            // Right
            {HumanBodyBones.RightHand, RightHand},
            {HumanBodyBones.RightThumbProximal, RightThumb},
            {HumanBodyBones.RightThumbIntermediate, RightThumb},
            {HumanBodyBones.RightThumbDistal, RightThumb},
            {HumanBodyBones.RightIndexProximal, RightHand},
            {HumanBodyBones.RightIndexIntermediate, RightHand},
            {HumanBodyBones.RightIndexDistal, RightHand},
            {HumanBodyBones.RightMiddleProximal, RightHand},
            {HumanBodyBones.RightMiddleIntermediate, RightHand},
            {HumanBodyBones.RightMiddleDistal, RightHand},
            {HumanBodyBones.RightRingProximal, RightHand},
            {HumanBodyBones.RightRingIntermediate, RightHand},
            {HumanBodyBones.RightRingDistal, RightHand},
            {HumanBodyBones.RightLittleProximal, RightHand},
            {HumanBodyBones.RightLittleIntermediate, RightHand},
            {HumanBodyBones.RightLittleDistal, RightHand},
       };
    }
}