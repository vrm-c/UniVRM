using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// XR_FB_body_tracking の joint を VRM-1.0 の TPose に当てはめたときの方向を定義します。
    /// 
    /// * https://developer.oculus.com/documentation/native/android/move-ref-body-joints/
    /// 
    /// OpenXR は右手系なのに対して Unityは左手系です。
    /// この Rig が期待するボーンの値は、XR_FB_body_tracking の Joint の値を Z軸反転で座標変換したものです。
    /// </summary>
    public static class XR_FB_body_tracking
    {
        /// <summary>
        /// up vector と forward vector の外積により空間を算出して、回転を得ます。
        /// </summary>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        /// <returns></returns>
        public static Quaternion GetRotation(Vector3 yAxis, Vector3 zAxis)
        {
            var xAxis = Vector3.Cross(yAxis, zAxis).normalized;
            var m = new Matrix4x4(xAxis, yAxis, zAxis, new Vector4(0, 0, 0, 1));
            return m.rotation;
        }

        public static Quaternion Spine = GetRotation(Vector3.forward, Vector3.left);
        public static Quaternion Left = GetRotation(Vector3.forward, Vector3.down);
        public static Quaternion LeftThumb = GetRotation((Vector3.left + Vector3.forward).normalized, Vector3.up);
        public static Quaternion Right = GetRotation(Vector3.back, Vector3.up);
        public static Quaternion RightThumb = GetRotation((Vector3.left + Vector3.back).normalized, Vector3.down);

        public static IReadOnlyDictionary<HumanBodyBones, Quaternion> InitialRotations => new Dictionary<HumanBodyBones, Quaternion>()
        {
            {HumanBodyBones.Hips, Spine},
            {HumanBodyBones.Spine, Spine},
            {HumanBodyBones.Chest, Spine},
            {HumanBodyBones.UpperChest, Spine},
            {HumanBodyBones.Neck, Spine},
            {HumanBodyBones.Head, Spine},
            {HumanBodyBones.LeftShoulder, Left},
            {HumanBodyBones.LeftUpperArm, Left},
            {HumanBodyBones.LeftLowerArm, Left},
            {HumanBodyBones.LeftHand, Left},
            {HumanBodyBones.RightShoulder, Right},
            {HumanBodyBones.RightUpperArm, Right},
            {HumanBodyBones.RightLowerArm, Right},
            {HumanBodyBones.RightHand, Right},
            // left
            {HumanBodyBones.LeftThumbProximal, LeftThumb},
            {HumanBodyBones.LeftThumbIntermediate, LeftThumb},
            {HumanBodyBones.LeftThumbDistal, LeftThumb},
            {HumanBodyBones.LeftIndexProximal, Left},
            {HumanBodyBones.LeftIndexIntermediate, Left},
            {HumanBodyBones.LeftIndexDistal, Left},
            {HumanBodyBones.LeftMiddleProximal, Left},
            {HumanBodyBones.LeftMiddleIntermediate, Left},
            {HumanBodyBones.LeftMiddleDistal, Left},
            {HumanBodyBones.LeftRingProximal, Left},
            {HumanBodyBones.LeftRingIntermediate, Left},
            {HumanBodyBones.LeftRingDistal, Left},
            {HumanBodyBones.LeftLittleProximal, Left},
            {HumanBodyBones.LeftLittleIntermediate, Left},
            {HumanBodyBones.LeftLittleDistal, Left},
            // right
            {HumanBodyBones.RightThumbProximal, RightThumb},
            {HumanBodyBones.RightThumbIntermediate, RightThumb},
            {HumanBodyBones.RightThumbDistal, RightThumb},
            {HumanBodyBones.RightIndexProximal, Right},
            {HumanBodyBones.RightIndexIntermediate, Right},
            {HumanBodyBones.RightIndexDistal, Right},
            {HumanBodyBones.RightMiddleProximal, Right},
            {HumanBodyBones.RightMiddleIntermediate, Right},
            {HumanBodyBones.RightMiddleDistal, Right},
            {HumanBodyBones.RightRingProximal, Right},
            {HumanBodyBones.RightRingIntermediate, Right},
            {HumanBodyBones.RightRingDistal, Right},
            {HumanBodyBones.RightLittleProximal, Right},
            {HumanBodyBones.RightLittleIntermediate, Right},
            {HumanBodyBones.RightLittleDistal, Right},
       };
    }
}