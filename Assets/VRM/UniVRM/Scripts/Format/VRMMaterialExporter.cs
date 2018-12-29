using System;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public class VRMMaterialExporter : MaterialExporter
    {
        protected override glTFMaterial CreateMaterial(Material m)
        {
            switch (m.shader.name)
            {
                case "VRM/UnlitTexture":
                    return Export_VRMUnlitTexture(m);

                case "VRM/UnlitTransparent":
                    return Export_VRMUnlitTransparent(m);

                case "VRM/UnlitCutout":
                    return Export_VRMUnlitCutout(m);

                case "VRM/UnlitTransparentZWrite":
                    return Export_VRMUnlitTransparentZWrite(m);

                case "VRM/MToon":
                    return Export_VRMMToon(m);

                default:
                    return base.CreateMaterial(m);
            }
        }

        static glTFMaterial Export_VRMUnlitTexture(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "OPAQUE";
            return material;
        }
        static glTFMaterial Export_VRMUnlitTransparent(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "BLEND";
            return material;
        }
        static glTFMaterial Export_VRMUnlitCutout(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "MASK";
            return material;
        }
        static glTFMaterial Export_VRMUnlitTransparentZWrite(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();
            material.alphaMode = "BLEND";
            return material;
        }

        static glTFMaterial Export_VRMMToon(Material m)
        {
            var material = glTF_KHR_materials_unlit.CreateDefault();

            switch (m.GetTag("RenderType", true))
            {
                case "Transparent":
                    material.alphaMode = "BLEND";
                    break;

                case "TransparentCutout":
                    material.alphaMode = "MASK";
                    material.alphaCutoff = m.GetFloat("_Cutoff");
                    break;

                default:
                    material.alphaMode = "OPAQUE";
                    break;
            }

            switch ((int)m.GetFloat("_CullMode"))
            {
                case 0:
                    material.doubleSided = true;
                    break;

                case 1:
                    Debug.LogWarning("ignore cull front");
                    break;

                case 2:
                    // cull back
                    break;

                default:
                    throw new NotImplementedException();
            }

            return material;
        }
    }
}
