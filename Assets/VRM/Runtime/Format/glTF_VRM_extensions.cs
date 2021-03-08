using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;

namespace VRM
{
    [Serializable]
    [JsonSchema(Title = "vrm", Description = @"
VRM extension is for 3d humanoid avatars (and models) in VR applications.
")]
    public class glTF_VRM_extensions
    {
        public const string ExtensionName = "VRM";

        public static readonly Utf8String ExtensionNameUtf8 = Utf8String.From(ExtensionName);

        [JsonSchema(Description = @"Version of exporter that vrm created. " + VRMVersion.VRM_VERSION)]
        public string exporterVersion = "UniVRM-" + VRMVersion.VERSION;

        [JsonSchema(Description = @"Version of VRM specification. " + VRMSpecVersion.VERSION)]
        public string specVersion = VRMSpecVersion.Version;

        public glTF_VRM_Meta meta = new glTF_VRM_Meta();
        public glTF_VRM_Humanoid humanoid = new glTF_VRM_Humanoid();
        public glTF_VRM_Firstperson firstPerson = new glTF_VRM_Firstperson();
        public glTF_VRM_BlendShapeMaster blendShapeMaster = new glTF_VRM_BlendShapeMaster();
        public glTF_VRM_SecondaryAnimation secondaryAnimation = new glTF_VRM_SecondaryAnimation();
        public List<glTF_VRM_Material> materialProperties = new List<glTF_VRM_Material>();

        public static bool TryDeserialize(glTFExtension extension, out glTF_VRM_extensions vrm)
        {            
            if (extension is glTFExtensionImport import)
            {
                foreach (var kv in import.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == ExtensionNameUtf8)
                    {
                        vrm = VrmDeserializer.Deserialize(kv.Value);
                        return true;
                    }
                }
            }

            vrm = default;
            return false;
        }
    }
}
