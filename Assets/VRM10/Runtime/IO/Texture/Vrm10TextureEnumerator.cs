using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public static class Vrm10TextureEnumerator
    {
        /// <summary>
        /// glTF 全体で使うテクスチャーをユニークになるように列挙する
        /// </summary>
        public static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateAllTexturesDistinct(GltfParser parser)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                throw new System.Exception("not vrm");
            }

            var usedTextures = new HashSet<SubAssetKey>();
            
            // Thumbnail Texture referenced by VRM Meta.
            if (TryGetMetaThumbnailTextureImportParam(parser, vrm, out (SubAssetKey key, TextureImportParam) thumbnail))
            {
                if (usedTextures.Add(thumbnail.key))
                {
                    yield return thumbnail;
                }
            }

            // Textures referenced by Materials.
            for (int i = 0; i < parser.GLTF.materials.Count; ++i)
            {
                foreach ((SubAssetKey key, TextureImportParam) kv in EnumerateTexturesReferencedByMaterials(parser, i))
                {
                    if (usedTextures.Add(kv.key))
                    {
                        yield return kv;
                    }
                }
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
            var key = new SubAssetKey(typeof(Texture2D), name);
            value = (key, param);
            return true;
        }
        
        /// <summary>
        /// Material によって参照されている Texture を Enumerate する.
        /// まず VRM Material だと仮定して処理し, 失敗すれば glTF Material だとして処理する.
        /// </summary>
        public static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateTexturesReferencedByMaterials(GltfParser parser, int i)
        {
            var m = parser.GLTF.materials[i];
            if (UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions, out var mToon))
            {
                // Enumerate VRM MToon Textures
                if (Vrm10MToonMaterialTextureImporter.TryGetBaseColorTexture(parser, m, out var litTex))
                    yield return litTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetNormalTexture(parser, m, out var normalTex))
                    yield return normalTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetShadeMultiplyTexture(parser, mToon, out var shadeTex))
                    yield return shadeTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetShadingShiftTexture(parser, mToon, out var shadeShiftTex))
                    yield return shadeShiftTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetMatcapTexture(parser, mToon, out var matcapTex))
                    yield return matcapTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetRimMultiplyTexture(parser, mToon, out var rimTex))
                    yield return rimTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetOutlineWidthMultiplyTexture(parser, mToon, out var outlineTex))
                    yield return outlineTex;
                if (Vrm10MToonMaterialTextureImporter.TryGetUvAnimationMaskTexture(parser, mToon, out var uvAnimMaskTex))
                    yield return uvAnimMaskTex;
            }
            else
            {
                // Fallback to glTF PBR & glTF Unlit
                foreach (var kv in GltfTextureEnumerator.EnumerateTexturesReferencedByMaterials(parser, i))
                {
                    yield return kv;
                }
            }
        }
    }
}