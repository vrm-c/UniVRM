using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public class TextureExtractor
    {
        const string TextureDirName = "Textures";

        GltfData m_data;
        public GltfData Data => m_data;

        public glTF GLTF => m_data.GLTF;

        public readonly Dictionary<SubAssetKey, UnityPath> Textures = new Dictionary<SubAssetKey, UnityPath>();
        private readonly IReadOnlyDictionary<SubAssetKey, Texture> m_subAssets;
        UnityPath m_textureDirectory;

        private static ProfilerMarker s_MarkerStartExtractTextures = new ProfilerMarker("Start Extract Textures");
        private static ProfilerMarker s_MarkerDelayedExtractTextures = new ProfilerMarker("Delayed Extract Textures");

        public TextureExtractor(GltfData data, UnityPath textureDirectory, IReadOnlyDictionary<SubAssetKey, Texture> subAssets)
        {
            m_data = data;
            m_textureDirectory = textureDirectory;
            m_textureDirectory.EnsureFolder();
            m_subAssets = subAssets;
        }

        public static string GetExt(string mime, string uri)
        {
            switch (mime)
            {
                case "image/png": return ".png";
                case "image/jpeg": return ".jpg";
            }

            return Path.GetExtension(uri).ToLower();
        }

        public void Extract(SubAssetKey key, TextureDescriptor texDesc)
        {
            if (Textures.ContainsKey(key))
            {
                return;
            }

            // write converted texture
            if (m_subAssets.TryGetValue(key, out var texture) && texture is Texture2D tex2D)
            {
                var targetPath = m_textureDirectory.Child($"{key.Name}.png");
                File.WriteAllBytes(targetPath.FullPath, tex2D.EncodeToPNG().ToArray());
                targetPath.ImportAsset();

                Textures.Add(key, targetPath);
            }
            else
            {
                // throw new Exception($"{key} is not converted.");
                UniGLTFLogger.Warning($"{key} is not converted.");
            }
        }

        /// <summary>
        ///
        /// * Texture(.png etc...)をディスクに書き出す
        /// * EditorApplication.delayCall で処理を進めて 書き出した画像が Asset として成立するのを待つ
        /// * 書き出した Asset から TextureImporter を取得して設定する
        ///
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="dirName"></param>
        /// <param name="onCompleted"></param>
        public static void ExtractTextures(GltfData data, UnityPath textureDirectory,
            ITextureDescriptorGenerator textureDescriptorGenerator, IReadOnlyDictionary<SubAssetKey, Texture> subAssets,
            Action<SubAssetKey, Texture2D> addRemap,
            Action<IEnumerable<UnityPath>> onCompleted = null)
        {
            s_MarkerStartExtractTextures.Begin();

            var extractor = new TextureExtractor(data, textureDirectory, subAssets);
            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var param in textureDescriptorGenerator.Get().GetEnumerable())
                {
                    extractor.Extract(param.SubAssetKey, param);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            s_MarkerStartExtractTextures.End();

            EditorApplication.delayCall += () =>
            {
                s_MarkerDelayedExtractTextures.Begin();

                // Wait for the texture assets to be imported

                foreach (var (key, targetPath) in extractor.Textures)
                {
                    // remap
                    var externalObject = targetPath.LoadAsset<Texture2D>();
                    if (externalObject != null)
                    {
                        addRemap(key, externalObject);
                    }
                }

                s_MarkerDelayedExtractTextures.End();

                if (onCompleted != null)
                {
                    onCompleted(extractor.Textures.Values);
                }
            };
        }
    }
}
