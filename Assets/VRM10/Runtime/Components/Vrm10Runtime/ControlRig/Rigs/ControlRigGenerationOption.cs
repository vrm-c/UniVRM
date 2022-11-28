using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public enum ControlRigGenerationOption
    {
        /// <summary>
        /// コントロールリグを生成しません。
        /// </summary>
        None = 0,

        /// <summary>
        /// 推奨されるオプションです。
        /// コントロールリグのボーン Transform を生成し、Root の Animator はコントロールリグのボーンを制御するようになります。
        /// </summary>
        Generate = 1,
        Vrm0XCompatibleRig = 1,

        /// <summary>
        /// コントロールリグのボーン Transform を生成し、Root の Animator はコントロールリグのボーンを制御するようになります。
        /// 手と指に関して、XR_EXT_hand_tracking の初期回転を持ちます。
        /// https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_EXT_hand_tracking
        /// </summary>
        Vrm0XCompatibleWithXR_EXT_hand_tracking = 2,
    }

    internal static class ControlRigGenerationOptionExtensions
    {
        internal static IReadOnlyDictionary<HumanBodyBones, Quaternion> ToInitialRotations(this ControlRigGenerationOption option)
        {
            switch (option)
            {
                case ControlRigGenerationOption.None:
                    return null;

                case ControlRigGenerationOption.Generate:
                    // case ControlRigGenerationOption.Vrm0XCompatibleRig:
                    return Vrm0XCompatibleRig.InitialRotations;

                case ControlRigGenerationOption.Vrm0XCompatibleWithXR_EXT_hand_tracking:
                    return XR_EXT_hand_tracking.InitialRotations;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
