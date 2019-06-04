using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace UniGLTF
{
    public partial class glTF_extensions : ExtensionsBase<glTF_extensions>
    {
        public VRM.glTF_VRM_extensions VRM;

        [JsonSerializeMembers]
        void VRMSerializeMembers(GLTFJsonFormatter f)
        {
            if (VRM != null)
            {
                f.Key("VRM"); f.GLTFValue(VRM);
            }
        }
    }
}


namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm", Description = @"
VRM extension is for 3d humanoid avatars (and models) in VR applications.
")]
    public class glTF_VRM_extensions : JsonSerializableBase
    {
        public static string ExtensionName
        {
            get
            {
                return "VRM";
            }
        }

        [JsonSchema(Description = @"Version of exporter that vrm created. " + VRMVersion.VRM_VERSION)]
        public string exporterVersion = "UniVRM-" + VRMVersion.VERSION;

        [JsonSchema(Description = @"Version of VRM specification. " + VRMSpecVersion.VERSION)]
        public string specVersion  = VRMSpecVersion.Version;

        public glTF_VRM_Meta meta = new glTF_VRM_Meta();
        public glTF_VRM_Humanoid humanoid = new glTF_VRM_Humanoid();
        public glTF_VRM_Firstperson firstPerson = new glTF_VRM_Firstperson();
        public glTF_VRM_BlendShapeMaster blendShapeMaster = new glTF_VRM_BlendShapeMaster();
        public glTF_VRM_SecondaryAnimation secondaryAnimation = new glTF_VRM_SecondaryAnimation();
        public List<glTF_VRM_Material> materialProperties = new List<glTF_VRM_Material>();

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => exporterVersion);
            f.KeyValue(() => specVersion);
            f.Key("meta"); f.GLTFValue(meta);
            f.Key("humanoid"); f.GLTFValue(humanoid);
            f.Key("firstPerson"); f.GLTFValue(firstPerson);
            f.Key("blendShapeMaster"); f.GLTFValue(blendShapeMaster);
            f.Key("secondaryAnimation"); f.GLTFValue(secondaryAnimation);
            f.Key("materialProperties"); f.GLTFValue(materialProperties);
        }
    }
}
