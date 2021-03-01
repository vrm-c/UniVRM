using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public sealed class RootAnimationImporter : IAnimationImporter
    {
        public void Import(ImporterContext context)
        {
            // animation
            if (context.GLTF.animations != null && context.GLTF.animations.Any())
            {
                var animation = context.Root.AddComponent<Animation>();
                context.AnimationClips = ImportAnimationClips(context.GLTF, context.InvertAxis);

                foreach (var clip in context.AnimationClips)
                {
                    animation.AddClip(clip, clip.name);
                }
                if (context.AnimationClips.Count > 0)
                {
                    animation.clip = context.AnimationClips.First();
                }
            }
        }

        private List<AnimationClip> ImportAnimationClips(glTF gltf, Axises invertAxis)
        {
            var animationClips = new List<AnimationClip>();
            for (var i = 0; i < gltf.animations.Count; ++i)
            {
                var clip = new AnimationClip();
                clip.ClearCurves();
                clip.legacy = true;
                clip.name = gltf.animations[i].name;
                if (string.IsNullOrEmpty(clip.name))
                {
                    clip.name = $"legacy_{i}";
                }
                clip.wrapMode = WrapMode.Loop;

                var animation = gltf.animations[i];
                if (string.IsNullOrEmpty(animation.name))
                {
                    animation.name = $"animation:{i}";
                }

                animationClips.Add(AnimationImporterUtil.ConvertAnimationClip(gltf, animation, invertAxis.Create()));
            }

            return animationClips;
        }
    }
}
