using System;
using UniJSON;

namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_materials_unlit : JsonSerializableBase
    {
        public static string ExtensionName
        {
            get
            {
                return "KHR_materials_unlit";
            }
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            //throw new System.NotImplementedException();
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
                extensions = new glTFMaterial_extensions
                {
                    KHR_materials_unlit = new glTF_KHR_materials_unlit(),
                },
            };
        }
    }

    [Serializable]
    public partial class glTFMaterial_extensions : ExtensionsBase<glTFMaterial_extensions>
    {
        [JsonSchema(Required = true)]
        public glTF_KHR_materials_unlit KHR_materials_unlit;

        [JsonSerializeMembers]
        void SerializeMembers_unlit(GLTFJsonFormatter f)
        {
            if (KHR_materials_unlit != null)
            {
                f.KeyValue(() => KHR_materials_unlit);
            }
        }
    }
}
