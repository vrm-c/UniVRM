using System.Collections.Generic;
using System.Linq;
using MeshUtility;
using MeshUtility.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.Animation
{
    public static class AnimationValidator
    {
        private enum ExporterValidatorMessages
        {
            [LangMsg(Languages.ja, "ExportRootをanimateすることはできません")]
            [LangMsg(Languages.en, "ExportRoot cannot be animated")]
            ROOT_ANIMATED,
        }

        public static IEnumerable<Validation> Validate(GameObject root)
        {
            if (root == null)
            {
                yield break;
            }

            var animationClips = new List<AnimationClip>();
            var animator = root.GetComponent<Animator>();
            var animation = root.GetComponent<UnityEngine.Animation>();
            if (animator != null)
            {
                animationClips = AnimationExporter.GetAnimationClips(animator);
            }
            else if (animation != null)
            {
                animationClips = AnimationExporter.GetAnimationClips(animation);
            }

            if (!animationClips.Any())
            {
                yield break;
            }

            foreach (var animationClip in animationClips)
            {
                foreach (var editorCurveBinding in AnimationUtility.GetCurveBindings(animationClip))
                {
                    // is root included in animation?
                    if (string.IsNullOrEmpty(editorCurveBinding.path))
                    {
                        yield return Validation.Error(ExporterValidatorMessages.ROOT_ANIMATED.Msg());
                        yield break;
                    }
                }
            }
        }
    }
}