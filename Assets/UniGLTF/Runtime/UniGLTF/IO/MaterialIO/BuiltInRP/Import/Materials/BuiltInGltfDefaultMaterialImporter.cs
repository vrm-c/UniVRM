using System;
using System.Collections.Generic;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// Generate the descriptor of the glTF default material.
    /// </summary>
    public static class BuiltInGltfDefaultMaterialImporter
    {
        public static MaterialDescriptor CreateParam()
        {
            // FIXME
            return new MaterialDescriptor(
                "__default__",
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