using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// MeshExportValidator から使われる
    /// </summary>
    public interface IMaterialValidator
    {
        /// <summary>
        /// shaderName から glTF マテリアルタイプ を得る 
        /// 
        /// shaderName が エクスポートできるものでないときは null を返す(gltfデフォルトの pbr として処理される)
        /// </summary>
        /// <param name="shaderName"></param>
        /// <returns></returns>
        string GetGltfMaterialTypeFromUnityShaderName(string shaderName);

        /// <summary>
        /// テクスチャーを使うプロパティを列挙する
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        IEnumerable<(string propertyName, Texture texture)> EnumerateTextureProperties(Material m);
    }

    public class DefaultMaterialValidator : IMaterialValidator
    {
        public virtual string GetGltfMaterialTypeFromUnityShaderName(string shaderName)
        {
            if (BuiltInGltfMaterialExporter.SupportedShaderNames.Contains(shaderName))
            {
                return "gltf";
            }

            return null;
        }

        public virtual IEnumerable<(string propertyName, Texture texture)> EnumerateTextureProperties(Material m)
        {
            foreach (var texturePropertyName in m.GetTexturePropertyNames())
            {
                var tex = m.GetTexture(texturePropertyName);
                if (tex != null)
                {
                    yield return (texturePropertyName, tex);
                }
            }
        }
    }
}
