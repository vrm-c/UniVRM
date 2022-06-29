using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public class EditorAnimationExporter : IAnimationExporter
    {
        /// <summary>
        /// AnimationClip を収集する。
        /// </summary>
        static List<AnimationClip> GetAnimationClips(GameObject Copy)
        {
            var clips = new List<AnimationClip>();

            var animator = Copy.GetComponent<Animator>();
            if (animator != null)
            {
                clips.AddRange(AnimationExporter.GetAnimationClips(animator));
            }

            var animation = Copy.GetComponent<Animation>();
            if (animation != null)
            {
                clips.AddRange(AnimationExporter.GetAnimationClips(animation));
            }

            return clips;
        }

        public void Export(ExportingGltfData _data, GameObject Copy, List<Transform> Nodes)
        {
            var clips = GetAnimationClips(Copy);

            foreach (AnimationClip clip in clips)
            {
                var animationWithCurve = AnimationExporter.Export(clip, Copy.transform, Nodes);

                foreach (var kv in animationWithCurve.SamplerMap)
                {
                    var sampler = animationWithCurve.Animation.samplers[kv.Key];

                    var inputAccessorIndex = _data.ExtendBufferAndGetAccessorIndex(kv.Value.Input);
                    sampler.input = inputAccessorIndex;

                    var outputAccessorIndex = _data.ExtendBufferAndGetAccessorIndex(kv.Value.Output);
                    sampler.output = outputAccessorIndex;

                    // modify accessors
                    var outputAccessor = _data.Gltf.accessors[outputAccessorIndex];
                    var channel = animationWithCurve.Animation.channels.First(x => x.sampler == kv.Key);
                    switch (glTFAnimationTarget.GetElementCount(channel.target.path))
                    {
                        case 1:
                            outputAccessor.type = "SCALAR";
                            //outputAccessor.count = ;
                            break;
                        case 3:
                            outputAccessor.type = "VEC3";
                            outputAccessor.count /= 3;
                            break;

                        case 4:
                            outputAccessor.type = "VEC4";
                            outputAccessor.count /= 4;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
                animationWithCurve.Animation.name = clip.name;
                _data.Gltf.animations.Add(animationWithCurve.Animation);
            }
        }
    }
}
