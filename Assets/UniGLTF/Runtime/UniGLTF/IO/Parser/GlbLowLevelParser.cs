using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// Low-level API.
    /// Parse from specified path & specified binary.
    /// </summary>
    public sealed class GlbLowLevelParser
    {
        public static readonly string UniqueFixResourceSuffix = "__UNIGLTF__DUPLICATED__";
        private static readonly Regex _removeUniqueFixResourceSuffix = new Regex($@"^(.+){UniqueFixResourceSuffix}(\d+)$");

        private readonly string _path;
        private readonly byte[] _binary;

        public GlbLowLevelParser(string path, byte[] specifiedBinary)
        {
            _path = path;
            _binary = specifiedBinary;
        }

        public GltfData Parse()
        {
            try
            {
                var chunks = ParseGlbChunks(_binary);
                var jsonBytes = chunks[0].Bytes;
                return ParseGltf(
                    _path,
                    Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                    chunks,
                    default,
                    new MigrationFlags()
                );
            }
            catch (StackOverflowException ex)
            {
                throw new Exception("[UniVRM Import Error] json parsing failed, nesting is too deep.\n" + ex);
            }
            catch
            {
                throw;
            }
        }

        public static List<GlbChunk> ParseGlbChunks(byte[] data)
        {
            var chunks = glbImporter.ParseGlbChunks(data);

            if (chunks.Count < 2)
            {
                throw new Exception("unknown chunk count: " + chunks.Count);
            }

            if (chunks[0].ChunkType != GlbChunkType.JSON)
            {
                throw new Exception("chunk 0 is not JSON");
            }

            if (chunks[1].ChunkType != GlbChunkType.BIN)
            {
                throw new Exception("chunk 1 is not BIN");
            }

            return chunks;
        }

        public static GltfData ParseGltf(string path, string json, IReadOnlyList<GlbChunk> chunks, IStorage storage, MigrationFlags migrationFlags)
        {
            var GLTF = GltfDeserializer.Deserialize(json.ParseAsJson());
            if (GLTF.asset.version != "2.0")
            {
                throw new UniGLTFException("unknown gltf version {0}", GLTF.asset.version);
            }

            // Version Compatibility
            RestoreOlderVersionValues(json, GLTF);

            FixMeshNameUnique(GLTF);
            foreach (var image in GLTF.images)
            {
                image.uri = PrepareUri(image.uri);
            }
            FixTextureNameUnique(GLTF);
            FixMaterialNameUnique(GLTF);
            FixNodeName(GLTF);
            FixAnimationNameUnique(GLTF);

            return new GltfData(path, json, GLTF, chunks, storage, migrationFlags);
        }

        private static void FixMeshNameUnique(glTF GLTF)
        {
            var used = new HashSet<string>();
            foreach (var mesh in GLTF.meshes)
            {
                if (string.IsNullOrEmpty(mesh.name))
                {
                    // empty
                    mesh.name = "mesh_" + Guid.NewGuid().ToString("N");
                    // Debug.LogWarning($"mesh.name: => {mesh.name}");
                    used.Add(mesh.name);
                }
                else
                {
                    var lower = mesh.name.ToLower();
                    if (used.Contains(lower))
                    {
                        // rename
                        var uname = lower + "_" + Guid.NewGuid().ToString("N");
                        // Debug.LogWarning($"mesh.name: {lower} => {uname}");
                        mesh.name = uname;
                        lower = uname;
                    }
                    used.Add(lower);
                }
            }
        }

        private static void RenameImageFromTexture(glTF GLTF, int i)
        {
            foreach (var texture in GLTF.textures)
            {
                if (texture.source == i)
                {
                    if (!string.IsNullOrEmpty(texture.name))
                    {
                        GLTF.images[i].name = texture.name;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// image.uri を前理
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string PrepareUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return uri;
            }

            if (uri.StartsWith("./"))
            {
                // skip
                uri = uri.Substring(2);
            }

            // %20 to ' ' etc...
            var unescape = Uri.UnescapeDataString(uri);
            return unescape;
        }

        /// <summary>
        /// gltfTexture.name を Unity Asset 名として運用する。
        /// ユニークである必要がある。
        /// </summary>
        private static void FixTextureNameUnique(glTF GLTF)
        {
            // NOTE: Windows FileSystem は大文字小文字の違いは同名ファイルとして扱ってしまうため, IgnoreCase で評価する.
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var textureIdx = 0; textureIdx < GLTF.textures.Count; ++textureIdx)
            {
                var gltfTexture = GLTF.textures[textureIdx];
                if (gltfTexture.source.HasValidIndex())
                {
                    var gltfImage = GLTF.images[gltfTexture.source.Value];
                    if (!string.IsNullOrEmpty(gltfImage.uri) && !gltfImage.uri.StartsWith("data:"))
                    {
                        // from image uri
                        gltfTexture.name = Path.GetFileNameWithoutExtension(gltfImage.uri);
                    }
                    if (string.IsNullOrEmpty(gltfTexture.name))
                    {
                        // use image name
                        gltfTexture.name = gltfImage.name;
                    }
                }
                if (string.IsNullOrEmpty(gltfTexture.name))
                {
                    gltfTexture.name = $"texture_{textureIdx}";
                }

                gltfTexture.name = FixNameUnique(used, gltfTexture.name);
            }
        }

        private static void FixMaterialNameUnique(glTF GLTF)
        {
            // NOTE: Windows FileSystem は大文字小文字の違いは同名ファイルとして扱ってしまうため, IgnoreCase で評価する.
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var materialIdx = 0; materialIdx < GLTF.materials.Count; ++materialIdx)
            {
                var material = GLTF.materials[materialIdx];

                if (string.IsNullOrEmpty(material.name))
                {
                    material.name = $"material_{materialIdx}";
                }

                material.name = FixNameUnique(used, material.name);
            }
        }

        /// <summary>
        /// rename empty name to $"{index}"
        /// </summary>
        private static void FixNodeName(glTF GLTF)
        {
            for (var i = 0; i < GLTF.nodes.Count; ++i)
            {
                var node = GLTF.nodes[i];
                if (string.IsNullOrWhiteSpace(node.name))
                {
                    node.name = $"{i}";
                }
            }
        }

        private static void FixAnimationNameUnique(glTF GLTF)
        {
            // NOTE: Windows FileSystem は大文字小文字の違いは同名ファイルとして扱ってしまうため, IgnoreCase で評価する.
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < GLTF.animations.Count; ++i)
            {
                var animation = GLTF.animations[i];

                if (string.IsNullOrEmpty(animation.name))
                {
                    animation.name = $"animation_{i}";
                }

                animation.name = FixNameUnique(used, animation.name);
            }
        }

        private static void RestoreOlderVersionValues(string Json, glTF GLTF)
        {
            var parsed = UniJSON.JsonParser.Parse(Json);
            for (int i = 0; i < GLTF.images.Count; ++i)
            {
                if (string.IsNullOrEmpty(GLTF.images[i].name))
                {
                    try
                    {
                        var extraName = parsed["images"][i]["extra"]["name"].Value.GetString();
                        if (!string.IsNullOrEmpty(extraName))
                        {
                            //Debug.LogFormat("restore texturename: {0}", extraName);
                            GLTF.images[i].name = extraName;
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }
        }

        public static void AppendImageExtension(glTFImage texture, string extension)
        {
            if (!texture.name.EndsWith(extension))
            {
                texture.name = texture.name + extension;
            }
        }

        public static string FixNameUnique(HashSet<string> used, string originalName)
        {
            if (used.Add(originalName))
            {
                return originalName;
            }

            var duplicatedIdx = 2;
            while (true)
            {
                var newName = $"{originalName}{UniqueFixResourceSuffix}{duplicatedIdx++}";
                if (used.Add(newName))
                {
                    return newName;
                }
            }
        }

        public static bool TryGetOriginalName(string name, out string originalName)
        {
            var match = _removeUniqueFixResourceSuffix.Match(name);
            if (match.Success)
            {
                originalName = match.Groups[1].Value;
                return true;
            }
            else
            {
                originalName = name;
                return false;
            }
        }
    }
}