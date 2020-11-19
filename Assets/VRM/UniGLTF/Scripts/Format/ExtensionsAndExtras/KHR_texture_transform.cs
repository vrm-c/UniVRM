using System;
using System.Collections;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_texture_transform
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
    }
}
