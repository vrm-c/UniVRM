using System;
using System.Linq;
using System.Collections.Generic;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    public class glTFAnimationTarget
    {
        [JsonSchema(Minimum = 0)]
        public int node;

        [JsonSchema(Required = true, EnumValues = new object[] { "translation", "rotation", "scale", "weights" }, EnumSerializationType = EnumSerializationType.AsString)]
        public string path;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;

        public enum Interpolations
        {
            LINEAR,
            STEP,
            CUBICSPLINE
        }

        public const string PATH_TRANSLATION = "translation";
        public const string PATH_EULER_ROTATION = "rotation";
        public const string PATH_ROTATION = "rotation";
        public const string PATH_SCALE = "scale";
        public const string PATH_WEIGHT = "weights";
        public const string NOT_IMPLEMENTED = "NotImplemented";

        [Obsolete("Use AnimationProperties")]
        public enum AnimationPropertys
        {
            Translation = AnimationProperties.Translation,
            EulerRotation = AnimationProperties.EulerRotation,
            Rotation = AnimationProperties.Rotation,
            Scale = AnimationProperties.Scale,
            Weight = AnimationProperties.Weight,
            BlendShape = AnimationProperties.BlendShape,

            NotImplemented = AnimationProperties.NotImplemented
        }

        [Obsolete]
        public static AnimationProperties AnimationPropertysToAnimationProperties(AnimationPropertys property)
        {
            if (!Enum.IsDefined(typeof(AnimationProperties), property))
            {
                throw new InvalidCastException("Failed to convert AnimationPropertys '" + property + "' to AnimationProperties");
            }
            return (AnimationProperties)property;
        }

        public enum AnimationProperties
        {
            Translation,
            EulerRotation,
            Rotation,
            Scale,
            Weight,
            BlendShape,

            NotImplemented
        }

        [Obsolete]
        public static string GetPathName(AnimationPropertys property)
        {
            return GetPathName(AnimationPropertysToAnimationProperties(property));
        }

        public static string GetPathName(AnimationProperties property)
        {
            switch (property)
            {
                case AnimationProperties.Translation:
                    return PATH_TRANSLATION;
                case AnimationProperties.EulerRotation:
                case AnimationProperties.Rotation:
                    return PATH_ROTATION;
                case AnimationProperties.Scale:
                    return PATH_SCALE;
                case AnimationProperties.BlendShape:
                    return PATH_WEIGHT;
                default: throw new NotImplementedException();
            }
        }

        public static AnimationProperties GetAnimationProperty(string path)
        {
            switch (path)
            {
                case PATH_TRANSLATION:
                    return AnimationProperties.Translation;
                case PATH_ROTATION:
                    return AnimationProperties.Rotation;
                case PATH_SCALE:
                    return AnimationProperties.Scale;
                case PATH_WEIGHT:
                    return AnimationProperties.BlendShape;
                default: throw new NotImplementedException();
            }
        }

        [Obsolete]
        public static int GetElementCount(AnimationPropertys property)
        {
            return GetElementCount(AnimationPropertysToAnimationProperties(property));
        }

        public static int GetElementCount(AnimationProperties property)
        {
            switch (property)
            {
                case AnimationProperties.Translation: return 3;
                case AnimationProperties.EulerRotation: return 3;
                case AnimationProperties.Rotation: return 4;
                case AnimationProperties.Scale: return 3;
                case AnimationProperties.BlendShape: return 1;
                default: throw new NotImplementedException();
            }
        }

        public static int GetElementCount(string path)
        {
            return GetElementCount(GetAnimationProperty(path));
        }
    }

    [Serializable]
    public class glTFAnimationChannel
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int sampler = -1;

        [JsonSchema(Required = true)]
        public glTFAnimationTarget target;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFAnimationSampler
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int input = -1;

        [JsonSchema(EnumValues = new object[] { "LINEAR", "STEP", "CUBICSPLINE" }, EnumSerializationType = EnumSerializationType.AsString)]
        public string interpolation;

        [JsonSchema(Required = true, Minimum = 0)]
        public int output = -1;

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;
    }

    [Serializable]
    public class glTFAnimation
    {
        public string name = "";

        [JsonSchema(Required = true, MinItems = 1)]
        public List<glTFAnimationChannel> channels = new List<glTFAnimationChannel>();

        [JsonSchema(Required = true, MinItems = 1)]
        public List<glTFAnimationSampler> samplers = new List<glTFAnimationSampler>();

        // empty schemas
        public glTFExtension extensions;
        public glTFExtension extras;

        [Obsolete]
        public int AddChannelAndGetSampler(int nodeIndex, glTFAnimationTarget.AnimationPropertys property)
        {
            return AddChannelAndGetSampler(nodeIndex, glTFAnimationTarget.AnimationPropertysToAnimationProperties(property));
        }

        public int AddChannelAndGetSampler(int nodeIndex, glTFAnimationTarget.AnimationProperties property)
        {
            // find channel
            var channel = channels.FirstOrDefault(x => x.target.node == nodeIndex && x.target.path == glTFAnimationTarget.GetPathName(property));
            if (channel != null)
            {
                return channel.sampler;
            }

            // not found. create new
            var samplerIndex = samplers.Count;
            var sampler = new glTFAnimationSampler();
            samplers.Add(sampler);

            channel = new glTFAnimationChannel
            {
                sampler = samplerIndex,
                target = new glTFAnimationTarget
                {
                    node = nodeIndex,
                    path = glTFAnimationTarget.GetPathName(property),
                },
            };
            channels.Add(channel);

            return samplerIndex;
        }
    }
}
