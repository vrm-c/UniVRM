using System;
using System.Collections.Generic;
using UniGLTF;


namespace UniGLTF
{
    public partial class glTFUsedExtensions
    {
        [UsedExtension]
        static string VRMGetUsedExtension
        {
            get
            {
                return "VRM";
            }
        }
    }

    public partial class glTF_extensions : ExtensionsBase<glTF_extensions>
    {
        public VRM.glTF_VRM_extensions VRM = new VRM.glTF_VRM_extensions();

        [JsonSerializeMembers]
        void VRMSerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => VRM);
        }
    }
}


namespace VRM
{
    [Serializable]
    public class glTF_VRM_extensions : JsonSerializableBase
    {
        public string exporterVersion = "UniVRM-" + VRMVersion.VERSION;

        public glTF_VRM_Meta meta = new glTF_VRM_Meta();
        public glTF_VRM_Humanoid humanoid = new glTF_VRM_Humanoid();
        public glTF_VRM_Firstperson firstPerson = new glTF_VRM_Firstperson();
        public glTF_VRM_BlendShapeMaster blendShapeMaster = new glTF_VRM_BlendShapeMaster();
        public glTF_VRM_SecondaryAnimation secondaryAnimation = new glTF_VRM_SecondaryAnimation();
        public List<glTF_VRM_Material> materialProperties = new List<glTF_VRM_Material>();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => exporterVersion);
            f.KeyValue(() => meta);
            f.KeyValue(() => humanoid);
            f.KeyValue(() => firstPerson);
            f.KeyValue(() => blendShapeMaster);
            f.KeyValue(() => secondaryAnimation);
            f.KeyValue(() => materialProperties);
        }
    }
}
