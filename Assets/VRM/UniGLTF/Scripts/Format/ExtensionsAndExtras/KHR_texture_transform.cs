using System;
using System.Collections;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_texture_transform : JsonSerializableBase
    {
        public static string ExtensionName
        {
            get
            {
                return "KHR_texture_transform";
            }
        }

        [JsonSchema(MinItems = 2, MaxItems = 2)]
        public float[] offset = new float[2] { 0.0f, 0.0f };

        public float rotation;

        [JsonSchema(MinItems = 2, MaxItems = 2)]
        public float[] scale = new float[2] { 1.0f, 1.0f };

        [ItemJsonSchema(Minimum = 0)]
        public int texCoord;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => offset);
            f.KeyValue(() => rotation);
            f.KeyValue(() => scale);
            f.KeyValue(() => texCoord);
        }
    }

    [Serializable]
    public class glTFTextureInfo_extensions : ExtensionsBase<glTFTextureInfo_extensions>
    {
        [JsonSchema(Required = true)]
        public glTF_KHR_texture_transform KHR_texture_transform;

        /// <summary>
        /// リフレクションでシリアライズする時は使われない
        /// </summary>
        /// <param name="f"></param>
        [JsonSerializeMembers]
        void SerializeMembers_textureInfo(GLTFJsonFormatter f)
        {
            if (KHR_texture_transform != null)
            {
                f.KeyValue(() => KHR_texture_transform);
            }
        }
    }
}
