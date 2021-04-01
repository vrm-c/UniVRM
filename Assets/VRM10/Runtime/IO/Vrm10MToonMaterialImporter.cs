using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;


namespace UniVRM10
{
    public static class Vrm10MToonMaterialImporter
    {
        /// <summary>
        /// VMRC_materials_mtoon の場合にマテリアル生成情報を作成する
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="i"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            var m = parser.GLTF.materials[i];
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions,
                out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
            {
                // fallback to gltf
                param = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            var name = m.name;
            param = new MaterialImportParam(name, MToon.Utils.ShaderName);

            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    param.TextureSlots.Add("_MainTex", GltfPBRMaterial.BaseColorTexture(parser, m));
                }
            }

            if (m.normalTexture != null && m.normalTexture.index != -1)
            {
                // normal map
                param.Actions.Add(material => material.EnableKeyword("_NORMALMAP"));
                var textureParam = GltfPBRMaterial.NormalTexture(parser, m);
                param.TextureSlots.Add("_BumpMap", textureParam);
                // param.FloatValues.Add("_BumpScale", m.normalTexture.scale);
            }

            if (m.emissiveTexture != null && m.emissiveTexture.index != -1)
            {
                var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(m.emissiveTexture);
                var textureParam = GltfTextureImporter.CreateSRGB(parser, m.emissiveTexture.index, offset, scale);
                param.TextureSlots.Add("_EmissionMap", textureParam);
            }

            // TODO:

            return true;
        }

        /// <summary>
        /// Material一つ分のテクスチャーを列挙する。重複する場合がある
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static IEnumerable<TextureImportParam> EnumerateTexturesForMaterial(GltfParser parser, int i)
        {
            // mtoon
            if (!TryCreateParam(parser, i, out MaterialImportParam param))
            {
                // unlit
                if (!GltfUnlitMaterial.TryCreateParam(parser, i, out param))
                {
                    // pbr
                    GltfPBRMaterial.TryCreateParam(parser, i, out param);
                }
            }

            foreach (var kv in param.TextureSlots)
            {
                yield return kv.Value;
            }
        }

        /// <summary>
        /// VRM-1 の thumbnail テクスチャー。gltf.textures ではなく gltf.images の参照であることに注意(sampler等の設定が無い)
        /// 
        /// MToonとは無関係だがとりあえずここに
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="vrm"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetMetaThumbnailTextureImportParam(GltfParser parser, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, out TextureImportParam value)
        {
            if (!vrm.Meta.ThumbnailImage.HasValue)
            {
                value = default;
                return false;
            }

            // thumbnail
            var imageIndex = vrm.Meta.ThumbnailImage.Value;
            var gltfImage = parser.GLTF.images[imageIndex];
            var name = new TextureImportName(TextureImportTypes.sRGB, gltfImage.name, gltfImage.GetExt(), "");

            GetTextureBytesAsync getBytesAsync = () =>
            {
                var bytes = parser.GLTF.GetImageBytes(parser.Storage, imageIndex);
                return Task.FromResult(GltfTextureImporter.ToArray(bytes));
            };
            value = new TextureImportParam(name, Vector2.zero, Vector2.one, default, TextureImportTypes.sRGB, default, default,
               getBytesAsync, default, default,
               default, default, default
               );
            return true;
        }

        /// <summary>
        /// glTF 全体で使うテクスチャーをユニークになるように列挙する
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IEnumerable<TextureImportParam> EnumerateAllTexturesDistinct(GltfParser parser)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                throw new System.Exception("not vrm");
            }

            if (TryGetMetaThumbnailTextureImportParam(parser, vrm, out TextureImportParam thumbnail))
            {
                yield return thumbnail;
            }

            var used = new HashSet<string>();
            for (int i = 0; i < parser.GLTF.materials.Count; ++i)
            {
                foreach (var textureInfo in EnumerateTexturesForMaterial(parser, i))
                {
                    if (used.Add(textureInfo.ExtractKey))
                    {
                        yield return textureInfo;
                    }
                }
            }
        }
    }
}
