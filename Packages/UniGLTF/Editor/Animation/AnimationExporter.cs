using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace UniGLTF
{

    public static class AnimationExporter
    {
        public class InputOutputValues
        {
            public float[] Input;
            public float[] Output;
        }

        public class AnimationWithSampleCurves
        {
            public glTFAnimation Animation;
            public Dictionary<int, InputOutputValues> SamplerMap = new Dictionary<int, InputOutputValues>();
        }

        public static List<AnimationClip> GetAnimationClips(Animation animation)
        {
            var clips = new List<AnimationClip>();
            foreach (AnimationState state in animation)
            {
                clips.Add(state.clip);
            }
            return clips;
        }

        public static List<AnimationClip> GetAnimationClips(Animator animator)
        {
            var clips = new List<AnimationClip>();

            RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
            UnityEditor.Animations.AnimatorController animationController = runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

            if (animationController == null)
            {
                return clips;
            }

            foreach (var layer in animationController.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    clips.Add(state.state.motion as AnimationClip);
                }
            }
            return clips;
        }

        static int GetNodeIndex(Transform root, List<Transform> nodes, string path)
        {
            var descendant = root.GetFromPath(path);
            return nodes.IndexOf(descendant);
        }

        public static glTFAnimationTarget.AnimationProperties PropertyToTarget(string property)
        {
            if (property.FastStartsWith("m_LocalPosition."))
            {
                return glTFAnimationTarget.AnimationProperties.Translation;
            }
            else if (property.FastStartsWith("localEulerAnglesRaw."))
            {
                return glTFAnimationTarget.AnimationProperties.EulerRotation;
            }
            else if (property.FastStartsWith("m_LocalRotation."))
            {
                return glTFAnimationTarget.AnimationProperties.Rotation;
            }
            else if (property.FastStartsWith("m_LocalScale."))
            {
                return glTFAnimationTarget.AnimationProperties.Scale;
            }
            else if (property.FastStartsWith("blendShape."))
            {
                return glTFAnimationTarget.AnimationProperties.BlendShape;
            }
            else
            {
                return glTFAnimationTarget.AnimationProperties.NotImplemented;
            }
        }

        public static int GetElementOffset(string property)
        {
            if (property.EndsWith(".x"))
            {
                return 0;
            }
            if (property.EndsWith(".y") || property.FastStartsWith("blendShape."))
            {
                return 1;
            }
            if (property.EndsWith(".z"))
            {
                return 2;
            }
            if (property.EndsWith(".w"))
            {
                return 3;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static AnimationWithSampleCurves Export(AnimationClip clip, Transform root, List<Transform> nodes)
        {
            var animation = new AnimationWithSampleCurves
            {
                Animation = new glTFAnimation(),
            };

            List<AnimationCurveData> curveDatum = new List<AnimationCurveData>();

            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);

                if (!curve.keys.Any())
                {
                    throw new Exception("There is no keyframe in AnimationCurve : " + clip.name);
                }

                var property = AnimationExporter.PropertyToTarget(binding.propertyName);
                if (property == glTFAnimationTarget.AnimationProperties.NotImplemented)
                {
                    UniGLTFLogger.Warning("Not Implemented keyframe property : " + binding.propertyName);
                    continue;
                }
                if (property == glTFAnimationTarget.AnimationProperties.EulerRotation)
                {
                    UniGLTFLogger.Warning("Interpolation setting of AnimationClip should be Quaternion");
                    continue;
                }

                var nodeIndex = GetNodeIndex(root, nodes, binding.path);
                var samplerIndex = animation.Animation.AddChannelAndGetSampler(nodeIndex, property);
                var elementCount = 0;
                if (property == glTFAnimationTarget.AnimationProperties.BlendShape)
                {
                    var mesh = nodes[nodeIndex].GetComponentOrThrow<SkinnedMeshRenderer>().sharedMesh;
                    elementCount = mesh.blendShapeCount;
                }
                else
                {
                    elementCount = glTFAnimationTarget.GetElementCount(property);
                }

                // 同一のsamplerIndexが割り当てられているcurveDataがある場合はそれを使用し、無ければ作る
                var curveData = curveDatum.FirstOrDefault(x => x.SamplerIndex == samplerIndex);
                if (curveData == null)
                {
                    curveData = new AnimationCurveData(AnimationUtility.GetKeyRightTangentMode(curve, 0), property, samplerIndex, elementCount);
                    curveDatum.Add(curveData);
                }

                // 全てのキーフレームを回収
                int elementOffset = 0;
                float valueFactor = 1.0f;
                if (property == glTFAnimationTarget.AnimationProperties.BlendShape)
                {
                    var mesh = nodes[nodeIndex].GetComponentOrThrow<SkinnedMeshRenderer>().sharedMesh;
                    var blendShapeName = binding.propertyName.Replace("blendShape.", "");
                    elementOffset = mesh.GetBlendShapeIndex(blendShapeName);
                    valueFactor = 0.01f;
                }
                else
                {
                    elementOffset = AnimationExporter.GetElementOffset(binding.propertyName);
                }

                if (elementOffset >= 0 && elementOffset < elementCount)
                {
                    for (int i = 0; i < curve.keys.Length; i++)
                    {
                        curveData.SetKeyframeData(curve.keys[i].time, curve.keys[i].value * valueFactor, elementOffset);
                    }
                }
            }

            //キー挿入
            foreach (var curve in curveDatum)
            {
                if (curve.Keyframes.Count == 0)
                    continue;

                curve.RecountEmptyKeyframe();

                var elementNum = curve.Keyframes.First().Values.Length;
                var values = default(InputOutputValues);
                if (!animation.SamplerMap.TryGetValue(curve.SamplerIndex, out values))
                {
                    values = new InputOutputValues();
                    values.Input = new float[curve.Keyframes.Count];
                    values.Output = new float[curve.Keyframes.Count * elementNum];
                    animation.SamplerMap[curve.SamplerIndex] = values;
                    animation.Animation.samplers[curve.SamplerIndex].interpolation = curve.GetInterpolation();
                }

                int keyframeIndex = 0;
                foreach (var keyframe in curve.Keyframes)
                {
                    values.Input[keyframeIndex] = keyframe.Time;
                    Buffer.BlockCopy(keyframe.GetRightHandCoordinate(), 0, values.Output, keyframeIndex * elementNum * sizeof(float), elementNum * sizeof(float));
                    keyframeIndex++;
                }
            }

            return animation;
        }
    }
}
