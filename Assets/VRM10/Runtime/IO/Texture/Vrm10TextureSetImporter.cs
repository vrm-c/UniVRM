using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10TextureSetImporter : ITextureSetImporter
    {
        private readonly GltfParser m_parser;

        public Vrm10TextureSetImporter(GltfParser parser)
        {
            m_parser = parser;
        }

        public IEnumerable<TextureImportParam> GetTextureParamsDistinct()
        {
            var usedTextures = new HashSet<SubAssetKey>();
            foreach (var (_, param) in EnumerateAllTexturesDistinct(m_parser))
            {
                if (usedTextures.Add(param.SubAssetKey))
                {
                    yield return param;
                }
            }
        }

        /// <summary>
        /// glTF 全体で使うテクスチャーを列挙する
        /// </summary>
        private static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateAllTexturesDistinct(GltfParser parser)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                throw new System.Exception("not vrm");
            }

            // Textures referenced by Materials.
            for (var materialIdx = 0; materialIdx < parser.GLTF.materials.Count; ++materialIdx)
            {
                var m = parser.GLTF.materials[materialIdx];
                if (UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions, out var mToon))
                {
                    foreach (var (_, tex) in Vrm10MToonTextureImporter.TryGetAllTextures(parser, m, mToon))
                    {
                        yield return tex;
                    }
                }
                else
                {
                    // Fallback to glTF PBR & glTF Unlit
                    foreach (var tex in GltfPbrTextureImporter.EnumerateTexturesReferencedByMaterial(parser, materialIdx))
                    {
                        yield return tex;
                    }
                }
            }

            // Thumbnail Texture referenced by VRM Meta.
            if (TryGetMetaThumbnailTextureImportParam(parser, vrm, out (SubAssetKey key, TextureImportParam) thumbnail))
            {
                yield return thumbnail;
            }
        }

        /// <summary>
        /// VRM-1 の thumbnail テクスチャー。gltf.textures ではなく gltf.images の参照であることに注意(sampler等の設定が無い)
        /// </summary>
        public static bool TryGetMetaThumbnailTextureImportParam(GltfParser parser, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, out (SubAssetKey, TextureImportParam) value)
        {
            if (vrm?.Meta?.ThumbnailImage == null)
            {
                value = default;
                return false;
            }

            var imageIndex = vrm.Meta.ThumbnailImage.Value;
            var gltfImage = parser.GLTF.images[imageIndex];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.sRGB, gltfImage.name, gltfImage.uri);

            GetTextureBytesAsync getThumbnailImageBytesAsync = () =>
            {
                var bytes = parser.GLTF.GetImageBytes(parser.Storage, imageIndex);
                return Task.FromResult(GltfTextureImporter.ToArray(bytes));
            };
            var param = new TextureImportParam(name, gltfImage.GetExt(), gltfImage.uri, Vector2.zero, Vector2.one, default, TextureImportTypes.sRGB, default, default,
               getThumbnailImageBytesAsync, default, default,
               default, default, default
               );
            value = (param.SubAssetKey, param);
            return true;
        }
    }
}
