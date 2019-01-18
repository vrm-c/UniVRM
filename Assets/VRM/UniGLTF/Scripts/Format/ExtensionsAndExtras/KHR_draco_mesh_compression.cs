using System;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    public partial class glTFPrimitives_extensions : ExtensionsBase<glTFPrimitives_extensions>
    {
        [JsonSerializeMembers]
        void SerializeMembers_draco(GLTFJsonFormatter f)
        {
            //throw new NotImplementedException();
        }
    }
}
