using System;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    public class glTF_KHR_draco_mesh_compression : JsonSerializableBase
    {
        [JsonSchema(Required = true, Minimum = 0)]
        public int bufferView = -1;
        public glTFAttributes attributes;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            //throw new NotImplementedException();
        }
    }

    [Serializable]
    public partial class glTFPrimitives_extensions : ExtensionsBase<glTFPrimitives_extensions>
    {
        [JsonSchema(Required = true)]
        public glTF_KHR_draco_mesh_compression KHR_draco_mesh_compression;

        [JsonSerializeMembers]
        void SerializeMembers_draco(GLTFJsonFormatter f)
        {
            //throw new NotImplementedException();
        }
    }
}
