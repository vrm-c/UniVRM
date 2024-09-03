using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// Generate the descriptor of the glTF default material.
    ///
    /// https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#default-material
    /// </summary>
    public class UrpGltfDefaultMaterialImporter
    {
        public Shader Shader { get; set; }

        public UrpGltfDefaultMaterialImporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("Universal Render Pipeline/Lit");
        }

        public MaterialDescriptor CreateParam(string materialName)
        {
            return new MaterialDescriptor(
                string.IsNullOrEmpty(materialName) ? "__default__" : materialName,
                Shader,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>> { GenerateDefaultMaterial }
            );
        }

        public static void GenerateDefaultMaterial(Material dst)
        {
            var context = new UrpLitContext(dst);
            context.UnsafeEditMode = true;

            context.SurfaceType = UrpLitSurfaceType.Opaque;
            context.IsAlphaClipEnabled = false;
            context.CullMode = CullMode.Back;

            context.BaseColorSrgb = new Color(1, 1, 1, 1);
            context.BaseTexture = null;

            context.WorkflowType = UrpLitWorkflowType.Metallic;
            context.SmoothnessTextureChannel = UrpLitSmoothnessMapChannel.SpecularMetallicAlpha;
            context.Metallic = 1f;
            context.Smoothness = 0f;
            context.MetallicGlossMap = null;

            context.OcclusionTexture = null;

            context.BumpMap = null;

            context.IsEmissionEnabled = false;
            context.EmissionColorLinear = new Color(0, 0, 0, 0);
            context.EmissionTexture = null;

            context.Validate();
        }
    }
}