using System;
using System.Collections.Generic;
using UniGLTF;


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

    [Serializable]
    public class glTF_extensions : JsonSerializableBase
    {
        public glTF_VRM_extensions VRM = new glTF_VRM_extensions();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => VRM);
        }
    }

    [Serializable]
    public class glTF_VRM : glTF
    {
        public glTF_extensions extensions = new glTF_extensions();
        public List<string> extensionsUsed = new List<string>
        {
            "VRM",
        };

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => extensionsUsed);
            f.KeyValue(() => extensions);
            base.SerializeMembers(f);
        }
    }
}
