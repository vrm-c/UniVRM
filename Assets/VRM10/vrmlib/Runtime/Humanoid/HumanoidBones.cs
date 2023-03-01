using System;

namespace VrmLib
{
    /// <summary>
    /// Required for skeleton.
    /// 15 bones.
    /// </summary>
    public class BoneRequiredAttribute : Attribute
    {
    }

    /// <summary>
    /// hips -> spine -> (chest) -> (heck) -> head: Y+
    /// </summary>
    public enum HumanoidBones
    {
        unknown,

        [BoneRequired]
        hips,

        #region leg
        [BoneRequired]
        leftUpperLeg,
        [BoneRequired]
        rightUpperLeg,
        [BoneRequired]
        leftLowerLeg,
        [BoneRequired]
        rightLowerLeg,
        [BoneRequired]
        leftFoot,
        [BoneRequired]
        rightFoot,
        #endregion

        #region spine
        [BoneRequired]
        spine,
        chest,
        neck,
        [BoneRequired]
        head,
        #endregion

        #region arm
        leftShoulder,
        rightShoulder,
        [BoneRequired]
        leftUpperArm,
        [BoneRequired]
        rightUpperArm,
        [BoneRequired]
        leftLowerArm,
        [BoneRequired]
        rightLowerArm,
        [BoneRequired]
        leftHand,
        [BoneRequired]
        rightHand,
        #endregion

        leftToes,
        rightToes,
        leftEye,
        rightEye,
        jaw,

        #region fingers
        leftThumbMetacarpal,
        leftThumbProximal,
        leftThumbDistal,
        leftIndexProximal,
        leftIndexIntermediate,
        leftIndexDistal,
        leftMiddleProximal,
        leftMiddleIntermediate,
        leftMiddleDistal,
        leftRingProximal,
        leftRingIntermediate,
        leftRingDistal,
        leftLittleProximal,
        leftLittleIntermediate,
        leftLittleDistal,
        rightThumbMetacarpal,
        rightThumbProximal,
        rightThumbDistal,
        rightIndexProximal,
        rightIndexIntermediate,
        rightIndexDistal,
        rightMiddleProximal,
        rightMiddleIntermediate,
        rightMiddleDistal,
        rightRingProximal,
        rightRingIntermediate,
        rightRingDistal,
        rightLittleProximal,
        rightLittleIntermediate,
        rightLittleDistal,
        #endregion

        upperChest,
    }
}
