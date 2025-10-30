using System.Collections.Generic;
using UnityEngine;


namespace UniHumanoid
{
    public static class AnimationClipUtility
    {
        static Dictionary<string, string> TraitPropMap = new Dictionary<string, string>
{
{"Left Thumb 1 Stretched", "LeftHand.Thumb.1 Stretched"},
{"Left Thumb Spread", "LeftHand.Thumb Spread"},
{"Left Thumb 2 Stretched", "LeftHand.Thumb.2 Stretched"},
{"Left Thumb 3 Stretched", "LeftHand.Thumb.3 Stretched"},
{"Left Index 1 Stretched", "LeftHand.Index.1 Stretched"},
{"Left Index Spread", "LeftHand.Index Spread"},
{"Left Index 2 Stretched", "LeftHand.Index.2 Stretched"},
{"Left Index 3 Stretched", "LeftHand.Index.3 Stretched"},
{"Left Middle 1 Stretched", "LeftHand.Middle.1 Stretched"},
{"Left Middle Spread", "LeftHand.Middle Spread"},
{"Left Middle 2 Stretched", "LeftHand.Middle.2 Stretched"},
{"Left Middle 3 Stretched", "LeftHand.Middle.3 Stretched"},
{"Left Ring 1 Stretched", "LeftHand.Ring.1 Stretched"},
{"Left Ring Spread", "LeftHand.Ring Spread"},
{"Left Ring 2 Stretched", "LeftHand.Ring.2 Stretched"},
{"Left Ring 3 Stretched", "LeftHand.Ring.3 Stretched"},
{"Left Little 1 Stretched", "LeftHand.Little.1 Stretched"},
{"Left Little Spread", "LeftHand.Little Spread"},
{"Left Little 2 Stretched", "LeftHand.Little.2 Stretched"},
{"Left Little 3 Stretched", "LeftHand.Little.3 Stretched"},
{"Right Thumb 1 Stretched", "RightHand.Thumb.1 Stretched"},
{"Right Thumb Spread", "RightHand.Thumb Spread"},
{"Right Thumb 2 Stretched", "RightHand.Thumb.2 Stretched"},
{"Right Thumb 3 Stretched", "RightHand.Thumb.3 Stretched"},
{"Right Index 1 Stretched", "RightHand.Index.1 Stretched"},
{"Right Index Spread", "RightHand.Index Spread"},
{"Right Index 2 Stretched", "RightHand.Index.2 Stretched"},
{"Right Index 3 Stretched", "RightHand.Index.3 Stretched"},
{"Right Middle 1 Stretched", "RightHand.Middle.1 Stretched"},
{"Right Middle Spread", "RightHand.Middle Spread"},
{"Right Middle 2 Stretched", "RightHand.Middle.2 Stretched"},
{"Right Middle 3 Stretched", "RightHand.Middle.3 Stretched"},
{"Right Ring 1 Stretched", "RightHand.Ring.1 Stretched"},
{"Right Ring Spread", "RightHand.Ring Spread"},
{"Right Ring 2 Stretched", "RightHand.Ring.2 Stretched"},
{"Right Ring 3 Stretched", "RightHand.Ring.3 Stretched"},
{"Right Little 1 Stretched", "RightHand.Little.1 Stretched"},
{"Right Little Spread", "RightHand.Little Spread"},
{"Right Little 2 Stretched", "RightHand.Little.2 Stretched"},
{"Right Little 3 Stretched", "RightHand.Little.3 Stretched"},
};

        public static AnimationClip CreateAnimationClipFromHumanPose(HumanPose pose)
        {
            var clip = new AnimationClip();

            // pos
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyPosition.x),
                });
                var muscle = "RootT.x";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyPosition.y),
                });
                var muscle = "RootT.y";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyPosition.z),
                });
                var muscle = "RootT.z";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }

            // rot
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyRotation.x),
                });
                var muscle = "RootQ.x";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyRotation.y),
                });
                var muscle = "RootQ.y";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyRotation.z),
                });
                var muscle = "RootQ.z";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                new Keyframe(0, pose.bodyRotation.w),
                });
                var muscle = "RootQ.w";
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }

            // muscles
            for (int i = 0; i < HumanTrait.MuscleCount; ++i)
            {
                var curve = new AnimationCurve(new Keyframe[]
                {
                    new Keyframe(0, pose.muscles[i]),
                });
                var muscle = HumanTrait.MuscleName[i];
                if (TraitPropMap.ContainsKey(muscle))
                {
                    muscle = TraitPropMap[muscle];
                }
                clip.SetCurve(string.Empty, typeof(Animator), muscle, curve);
            }
            return clip;
        }
    }
}
