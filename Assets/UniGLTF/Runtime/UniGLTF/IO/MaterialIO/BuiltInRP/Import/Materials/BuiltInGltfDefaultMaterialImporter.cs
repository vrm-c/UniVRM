using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Generate the descriptor of the glTF default material.
    /// </summary>
    public static class BuiltInGltfDefaultMaterialImporter
    {
        public static MaterialDescriptor CreateParam(string materialName = null)
        {
            // FIXME
            return new MaterialDescriptor(
                string.IsNullOrEmpty(materialName) ? "__default__" : materialName,
                BuiltInGltfPbrMaterialImporter.Shader,
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