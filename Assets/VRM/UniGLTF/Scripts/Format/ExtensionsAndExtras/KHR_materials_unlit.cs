using System;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_materials_unlit
    {
        public static string ExtensionName
        {
            get
            {
                return "KHR_materials_unlit";
            }
        }

        public static glTFMaterial CreateDefault()
        {
            return new glTFMaterial
            {
                pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    baseColorFactor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f },
                    roughnessFactor = 0.9f,
                    metallicFactor = 0.0f,
                },
                extensions = new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>("KHR_materials_unlit", new glTF_KHR_materials_unlit()),
                },
            };
        }
    }
}
