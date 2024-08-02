using System;
using UnityEngine;

namespace UniGLTF
{
    public class UrpUnlitMaterialExporter
    {
        public Shader Shader { get; set; }

        /// <summary>
        /// "Universal Render Pipeline/Unlit" シェーダのマテリアルをエクスポートする。
        ///
        /// プロパティに互換性がある他のシェーダを指定することもできる。
        /// </summary>
        public UrpUnlitMaterialExporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("Universal Render Pipeline/Unlit");
        }

        public bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            try
            {
                if (src == null) throw new ArgumentNullException(nameof(src));
                if (src.shader != Shader) throw new ArgumentException(nameof(src));
                if (textureExporter == null) throw new ArgumentNullException(nameof(textureExporter));

                dst = glTF_KHR_materials_unlit.CreateDefault();
                dst.name = src.name;

                var context = new UrpLitContext(src);

                UrpLitMaterialExporter.ExportSurfaceSettings(context, dst, textureExporter);
                UrpLitMaterialExporter.ExportBaseColor(context, dst, textureExporter);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                dst = default;
                return false;
            }
        }
    }
}