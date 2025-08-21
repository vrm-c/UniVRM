using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// A class that generates MaterialDescriptor for "Standard" shader based on glTF default Material specification.
    ///
    /// https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#default-material
    /// </summary>
    public class BuiltInGltfDefaultMaterialImporter
    {
        /// <summary>
        /// Can be replaced with custom shaders that are compatible with "Standard" properties and keywords.
        /// </summary>
        public Shader Shader { get; set; }

        public BuiltInGltfDefaultMaterialImporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("Standard");
        }

        public MaterialDescriptor CreateParam(string materialName = null)
        {
            // FIXME
            return new MaterialDescriptor(
                string.IsNullOrEmpty(materialName) ? "__default__" : materialName,
                Shader,
                default,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>>()
            );
        }
    }
}