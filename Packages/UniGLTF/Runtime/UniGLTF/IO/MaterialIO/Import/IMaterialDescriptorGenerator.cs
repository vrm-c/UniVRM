namespace UniGLTF
{
    /// <summary>
    /// 指定の index の glTFMaterial から Import できる Material の生成情報を生成する。
    /// glTFMaterial と Unity Material は 1:1 対応する。
    /// 
    /// IMaterialDescriptorGenerator の使われ方は MaterialFactory.LoadAsync を参照 
    /// </summary>
    public interface IMaterialDescriptorGenerator
    {
        /// <summary>
        /// Generate the MaterialDescriptor generated from the index i.
        /// <code>
        /// glTFMaterial src = data.GLTF.materials[i];
        /// return new MaterialDescriptor();
        /// </code>        
        /// </summary>
        MaterialDescriptor Get(GltfData data, int i);

        /// <summary>
        /// Generate the fallback MaterialDescriptor for the non-specified glTF material.
        /// <see href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#default-material"/>
        /// </summary>
        MaterialDescriptor GetGltfDefault(string materialName = null);
    }
}
