using System.Collections.Generic;
using System.Linq;
using MeshUtility.M17N;
using UnityEngine;


namespace MeshUtility.Validators
{
    public static class MaterialValidator
    {
        public enum ValidationMessages
        {
            [LangMsg(Languages.ja, "Standard, Unlit, MToon 以外のマテリアルは、Standard になります")]
            [LangMsg(Languages.en, "It will export as `Standard` fallback")]
            UNKNOWN_SHADER,
        }

        public static IEnumerable<Validation> Validate(GameObject ExportRoot)
        {
            if (ExportRoot == null)
            {
                yield break;
            }

            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                for (int i = 0; i < r.sharedMaterials.Length; ++i)
                    if (r.sharedMaterials[i] == null)
                    {
                        yield return Validation.Error($"Renderer: {r.name}.Materials[{i}] is null. please fix it");
                    }
            }

            var materials = renderers.SelectMany(x => x.sharedMaterials).Where(x => x != null).Distinct();
            foreach (var material in materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.shader.name == "Standard")
                {
                    // standard
                    continue;
                }

                if (UniGLTF.ShaderPropExporter.PreShaderPropExporter.UseUnlit(material.shader.name))
                {
                    // unlit
                    continue;
                }

                if (UniGLTF.ShaderPropExporter.PreShaderPropExporter.VRMExtensionShaders.Contains(material.shader.name))
                {
                    // VRM supported
                    continue;
                }

                yield return Validation.Warning($"Material: {material.name}. Unknown Shader: \"{material.shader.name}\" is used. {ValidationMessages.UNKNOWN_SHADER.Msg()}");
            }
        }
    }
}
