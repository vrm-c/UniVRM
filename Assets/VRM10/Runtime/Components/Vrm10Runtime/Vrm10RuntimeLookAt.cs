using System;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;

namespace UniVRM10
{
    public sealed class Vrm10RuntimeLookAt : ILookAtEyeDirectionProvider
    {
        private readonly VRM10ObjectLookAt _lookAt;
        private readonly Transform _head;
        private readonly Vector3 _eyeTransformLocalPosition;
        private readonly Quaternion _eyeTransformLocalRotation;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        internal ILookAtEyeDirectionApplicable EyeDirectionApplicable { get; }

        public LookAtEyeDirection EyeDirection { get; private set; }

        public Transform EyeTransform { get; }

        internal Vrm10RuntimeLookAt(VRM10ObjectLookAt lookAt, UniHumanoid.Humanoid humanoid, Transform head)
        {
            _lookAt = lookAt;
            _head = head;
            EyeTransform = InitializeEyePositionTransform(_head, _lookAt.OffsetFromHead);
            _eyeTransformLocalPosition = EyeTransform.localPosition;
            _eyeTransformLocalRotation = EyeTransform.localRotation;

            var leftEyeBone = humanoid.GetBoneTransform(HumanBodyBones.LeftEye);
            var rightEyeBone = humanoid.GetBoneTransform(HumanBodyBones.RightEye);
            if (_lookAt.LookAtType == LookAtType.bone && leftEyeBone != null && rightEyeBone != null)
            {
                EyeDirectionApplicable = new LookAtEyeDirectionApplicableToBone(leftEyeBone, rightEyeBone, _lookAt.HorizontalOuter, _lookAt.HorizontalInner, _lookAt.VerticalDown, _lookAt.VerticalUp);
            }
            else
            {
                EyeDirectionApplicable = new LookAtEyeDirectionApplicableToExpression(_lookAt.HorizontalOuter, _lookAt.HorizontalInner, _lookAt.VerticalDown, _lookAt.VerticalUp);
            }
        }

        internal void Process(VRM10ObjectLookAt.LookAtTargetTypes lookAtTargetType, Transform lookAtTarget)
        {
            EyeTransform.localPosition = _eyeTransformLocalPosition;
            EyeTransform.localRotation = _eyeTransformLocalRotation;

            switch (lookAtTargetType)
            {
                case VRM10ObjectLookAt.LookAtTargetTypes.SpecifiedTransform:
                    // NOTE: 指定された Transform の位置を向くように Yaw/Pitch を計算して適用する
                    if (lookAtTarget != null)
                    {
                        var value = CalculateYawPitchFromLookAtPosition(lookAtTarget.position);
                        SetYawPitchManually(value.Yaw, value.Pitch);
                    }
                    break;
                case VRM10ObjectLookAt.LookAtTargetTypes.YawPitchValue:
                    // NOTE: 直接 Set された Yaw/Pitch を使って計算する
                    break;
            }

            EyeDirection = new LookAtEyeDirection(Yaw, Pitch, 0, 0);
        }

        /// <summary>
        /// Yaw/Pitch 値を直接設定します。
        /// LookAtTargetTypes が SpecifiedTransform の場合、ここで設定しても値は上書きされます。
        /// </summary>
        /// <param name="yaw">Headボーンのforwardに対するyaw角(度)</param>
        /// <param name="pitch">Headボーンのforwardに対するpitch角(度)</param>
        public void SetYawPitchManually(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }

        public (float Yaw, float Pitch) CalculateYawPitchFromLookAtPosition(Vector3 lookAtWorldPosition)
        {
            var localPosition = EyeTransform.worldToLocalMatrix.MultiplyPoint(lookAtWorldPosition);
            Matrix4x4.identity.CalcYawPitch(localPosition, out var yaw, out var pitch);
            return (yaw, pitch);
        }

        private static Transform InitializeEyePositionTransform(Transform head, Vector3 eyeOffsetValue)
        {
            if (!Application.isPlaying) return null;

            // NOTE: このメソッドを実行するとき、モデル全体は初期姿勢（T-Pose）でなければならない。
            var eyePositionTransform = new GameObject("_eye_transform_").transform;
            eyePositionTransform.SetParent(head);
            eyePositionTransform.localPosition = eyeOffsetValue;
            eyePositionTransform.rotation = Quaternion.identity;

            return eyePositionTransform;
        }

#region Obsolete
        [Obsolete]
        public Transform GetLookAtOrigin(Transform head)
        {
            return EyeTransform;
        }

        [Obsolete]
        public void SetLookAtYawPitch(float yaw, float pitch)
        {
            SetYawPitchManually(yaw, pitch);
        }

        [Obsolete]
        public (float, float) GetLookAtYawPitch(
            Transform head,
            VRM10ObjectLookAt.LookAtTargetTypes lookAtTargetType,
            Transform gaze)
        {
            switch (lookAtTargetType)
            {
                case VRM10ObjectLookAt.LookAtTargetTypes.SpecifiedTransform:
                    return CalculateYawPitchFromLookAtPosition(gaze.position);
                case VRM10ObjectLookAt.LookAtTargetTypes.YawPitchValue:
                    return (Yaw, Pitch);
                default:
                    throw new ArgumentOutOfRangeException(nameof(lookAtTargetType), lookAtTargetType, null);
            }
        }
#endregion
    }
}
