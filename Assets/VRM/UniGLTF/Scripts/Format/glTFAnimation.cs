using System;
using System.Linq;
using System.Collections.Generic;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    public class glTFAnimationTarget : JsonSerializableBase
    {
        [JsonSchema(Minimum = 0)]
        public int node;

        [JsonSchema(Required = true, EnumValues = new object[] { "translation", "rotation", "scale", "weights" }, EnumSerializationType = EnumSerializationType.AsString)]
        public string path;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => node);
            if (!string.IsNullOrEmpty(path))
            {
                f.KeyValue(() => path);
            }
        }

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

        public enum AnimationPropertys
        {
            Translation,
            EulerRotation,
            Rotation,
            Scale,
            Weight,
            BlendShape,

            NotImplemented
        }

        public static string GetPathName(AnimationPropertys property)
        {
            switch (property)
            {
                case AnimationPropertys.Translation:
                    return PATH_TRANSLATION;
                case AnimationPropertys.EulerRotation: 
                case AnimationPropertys.Rotation:
                    return PATH_ROTATION;
                case AnimationPropertys.Scale:
                    return PATH_SCALE;
                case AnimationPropertys.BlendShape:
                    return PATH_WEIGHT;
                default: throw new NotImplementedException();
            }
        }

        public static AnimationPropertys GetAnimationProperty(string path)
        {
            switch (path)
            {
                case PATH_TRANSLATION:
                    return AnimationPropertys.Translation;
                case PATH_ROTATION:
                    return AnimationPropertys.Rotation;
                case PATH_SCALE:
                    return AnimationPropertys.Scale;
                case PATH_WEIGHT:
                    return AnimationPropertys.BlendShape;
                default: throw new NotImplementedException();
            }
        }

        public static int GetElementCount(AnimationPropertys property)
        {
            switch (property)
            {
                case AnimationPropertys.Translation: return 3;
                case AnimationPropertys.EulerRotation: return 3;
                case AnimationPropertys.Rotation: return 4;
                case AnimationPropertys.Scale: return 3;
                case AnimationPropertys.BlendShape: return 1;
                default: throw new NotImplementedException();
            }
        }

        public static int GetElementCount(string path)
        {
            return GetElementCount(GetAnimationProperty(path));
        }
    }

    [Serializable]
    public class glTFAnimationChannel : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int sampler = -1;

        [JsonSchema(Required = true)]
        public glTFAnimationTarget target;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => sampler);
            f.Key("target"); f.GLTFValue(target);
        }
    }

    [Serializable]
    public class glTFAnimationSampler : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int input = -1;

        [JsonSchema(EnumValues = new object[] { "LINEAR", "STEP", "CUBICSPLINE" }, EnumSerializationType = EnumSerializationType.AsString)]
        public string interpolation;

        [JsonSchema(Required = true, Minimum = 0)]
        public int output = -1;

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => input);
            if (!string.IsNullOrEmpty(interpolation))
            {
                f.KeyValue(() => interpolation);
            }
            f.KeyValue(() => output);
        }
    }

    [Serializable]
    public class glTFAnimation : JsonSerializableBase
    {
        public string name = "";

        [JsonSchema(Required = true, MinItems = 1)]
        public List<glTFAnimationChannel> channels = new List<glTFAnimationChannel>();

        [JsonSchema(Required = true, MinItems = 1)]
        public List<glTFAnimationSampler> samplers = new List<glTFAnimationSampler>();

        // empty schemas
        public object extensions;
        public object extras;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            if (!string.IsNullOrEmpty(name))
            {
                f.KeyValue(() => name);
            }

            f.Key("channels"); f.GLTFValue(channels);
            f.Key("samplers"); f.GLTFValue(samplers);
        }

        public int AddChannelAndGetSampler(int nodeIndex, glTFAnimationTarget.AnimationPropertys property)
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
