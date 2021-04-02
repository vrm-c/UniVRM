using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public sealed class RootAnimationImporter : IAnimationImporter
    {
        public List<AnimationClip> Import(glTF gltf, GameObject root, List<Transform> _nodes, List<AnimationClip> _clips, Axises invertAxis)
        {
            var animationClips = new List<AnimationClip>();
            if (gltf.animations != null && gltf.animations.Any())
            {
                var animation = root.AddComponent<Animation>();
                animationClips.AddRange(ImportAnimationClips(gltf, invertAxis));

                foreach (var clip in animationClips)
                {
                    animation.AddClip(clip, clip.name);
                }
                if (animationClips.Count > 0)
                {
                    animation.clip = animationClips.First();
                }
            }
            return animationClips;
        }

        private IEnumerable<AnimationClip> ImportAnimationClips(glTF gltf, Axises invertAxis)
        {
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

                yield return AnimationImporterUtil.ConvertAnimationClip(gltf, animation, invertAxis.Create());
            }
        }
    }
}
