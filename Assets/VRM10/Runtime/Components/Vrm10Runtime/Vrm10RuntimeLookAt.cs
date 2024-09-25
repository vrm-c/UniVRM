using System;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;

namespace UniVRM10
{
    public sealed class Vrm10RuntimeLookAt : ILookAtEyeDirectionProvider
    {
        private readonly VRM10ObjectLookAt _lookAt;
        private readonly Transform _head;
        private readonly Vector3 _lookAtOriginTransformLocalPosition;
        private readonly Quaternion _lookAtOriginTransformLocalRotation;

        internal ILookAtEyeDirectionApplicable EyeDirectionApplicable { get; }

        /// <summary>
        /// 入力値。適宜更新可。
        /// </summary>
        public LookAtInput LookAtInput { get; set; }

        /// <summary>
        /// 出力値。Process() のみが更新する
        /// </summary>
        public LookAtEyeDirection EyeDirection { get; private set; }
        public float Yaw => EyeDirection.Yaw;
        public float Pitch => EyeDirection.Pitch;

        /// <summary>
        /// Transform that indicates the position center of eyes.
        /// This only represents the position of center of eyes, not the viewing direction.
        /// Local +Z axis represents forward vector of the head.
        /// Local +Y axis represents up vector of the head.
        ///
        /// 目の位置を示す Transform。
        /// 視線方向は反映されない。
        /// ローカル +Z 軸が頭の正面方向を表す。
        /// ローカル +Y 軸が頭の上方向を表す。
        /// </summary>
        public Transform LookAtOriginTransform { get; }

        internal Vrm10RuntimeLookAt(Vrm10Instance instance, UniHumanoid.Humanoid humanoid, Vrm10RuntimeControlRig controlRig)
        {
            _lookAt = instance.Vrm.LookAt;

            LookAtOriginTransform = InitializeLookAtOriginTransform(
                humanoid,
                controlRig,
                _lookAt.OffsetFromHead,
                 instance.transform.rotation);

            _lookAtOriginTransformLocalPosition = LookAtOriginTransform.localPosition;
            _lookAtOriginTransformLocalRotation = LookAtOriginTransform.localRotation;

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

        internal LookAtEyeDirection Process()
        {
            LookAtOriginTransform.localPosition = _lookAtOriginTransformLocalPosition;
            LookAtOriginTransform.localRotation = _lookAtOriginTransformLocalRotation;

            if (LookAtInput.YawPitch is LookAtEyeDirection dir)
            {
                EyeDirection = dir;
            }
            else if (LookAtInput.WorldPosition is Vector3 worldPosition)
            {
                // NOTE: 指定された Transform の位置を向くように Yaw/Pitch を計算して適用する
                var (yaw, pitch) = CalculateYawPitchFromLookAtPosition(worldPosition);
                EyeDirection = new LookAtEyeDirection(yaw, pitch);
            }
            return EyeDirection;
        }

        /// <summary>
        /// Yaw/Pitch 値を直接設定します。
        /// Vrm10Instance.LookAtTargetTypes が SpecifiedTransform の場合、ここで設定しても値は上書きされます。
        /// </summary>
        /// <param name="yaw">Headボーンのforwardに対するyaw角(度)</param>
        /// <param name="pitch">Headボーンのforwardに対するpitch角(度)</param>
        public void SetYawPitchManually(float yaw, float pitch)
        {
            LookAtInput = new LookAtInput { YawPitch = new LookAtEyeDirection(yaw, pitch) };
        }

        public (float Yaw, float Pitch) CalculateYawPitchFromLookAtPosition(Vector3 lookAtWorldPosition)
        {
            var localPosition = LookAtOriginTransform.worldToLocalMatrix.MultiplyPoint(lookAtWorldPosition);
            Matrix4x4.identity.CalcYawPitch(localPosition, out var yaw, out var pitch);
            return (yaw, pitch);
        }

        /// <summary>
        /// Generate empty object for gaze calculation.
        /// NOTE: このメソッドを実行するとき、モデル全体は初期姿勢（T-Pose）でなければならない。
        /// NOTE: Vrm10Instance.Runtime 呼び出しによりトリガーされる。
        /// From v0.127.0: VRM Root（ Vrm10Instance が Add されている）GameObject は初期姿勢でなくてもよい #2445
        /// </summary>
        /// <param name="humanoid"></param>
        /// <param name="controlRig">if provided parent is controlrig head else humanoid head</param>
        /// <param name="eyeOffsetValue">A humanoid head local offset</param>
        /// <param name="rootRotation">world rotation of vrm model</param>
        /// <returns></returns>
        private static Transform InitializeLookAtOriginTransform(UniHumanoid.Humanoid humanoid, Vrm10RuntimeControlRig controlRig,
            Vector3 eyeOffsetValue, Quaternion rootRotation)
        {
            var lookAtOrigin = new GameObject("_look_at_origin_").transform;
            if (controlRig != null)
            {
                // controlRig のHeadに連結
                lookAtOrigin.SetParent(controlRig.GetBoneTransform(HumanBodyBones.Head));
            }
            else
            {
                // humanoid のHeadに連結
                lookAtOrigin.SetParent(humanoid.Head);
            }

            lookAtOrigin.position = humanoid.Head.TransformPoint(eyeOffsetValue);
            // v0.127.0
            lookAtOrigin.rotation = rootRotation;

            return lookAtOrigin;
        }

        #region Obsolete
        [Obsolete("Use " + nameof(LookAtOriginTransform))]
        public Transform GetLookAtOrigin(Transform head)
        {
            return LookAtOriginTransform;
        }

        [Obsolete("Use " + nameof(SetYawPitchManually))]
        public void SetLookAtYawPitch(float yaw, float pitch)
        {
            SetYawPitchManually(yaw, pitch);
        }

        [Obsolete("Use " + nameof(Yaw) + " & " + nameof(Pitch))]
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
