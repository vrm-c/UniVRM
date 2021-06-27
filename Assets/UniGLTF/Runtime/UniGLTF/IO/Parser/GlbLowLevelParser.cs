using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UniJSON;

namespace UniGLTF
{
    public sealed class GlbLowLevelParser
    {
        private readonly string _path;
        private readonly byte[] _binary;
        
        public GlbLowLevelParser(string path, byte[] specifiedBinary)
        {
            _path = path;
            _binary = specifiedBinary;
        }

        public IGltfData Parse()
        {
            try
            {
                var chunks = ParseGlbChunks(_binary);
                var jsonBytes = chunks[0].Bytes;
                return ParseGltf(
                    _path,
                    Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                    chunks,
                    new SimpleStorage(chunks[1].Bytes),
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

        public static IGltfData ParseGltf(string path, string json, IReadOnlyList<GlbChunk> chunks, IStorage storage, MigrationFlags migrationFlags)
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

            // parepare byte buffer
            //GLTF.baseDir = System.IO.Path.GetDirectoryName(Path);
            foreach (var buffer in GLTF.buffers)
            {
                buffer.OpenStorage(storage);
            }

            return new IGltfData(path, json, GLTF, chunks, storage, migrationFlags);
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
            var used = new HashSet<string>();
            for (int i = 0; i < GLTF.textures.Count; ++i)
            {
                var gltfTexture = GLTF.textures[i];
                var gltfImage = GLTF.images[gltfTexture.source];
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
                if (string.IsNullOrEmpty(gltfTexture.name))
                {
                    // no name
                    var newName = $"texture_{i}";
                    if (!used.Add(newName))
                    {
                        newName = "texture_" + Guid.NewGuid().ToString("N");
                        if (!used.Add(newName))
                        {
                            throw new Exception();
                        }
                    }
                    gltfTexture.name = newName;
                }
                else
                {
                    var lower = gltfTexture.name.ToLower();
                    if (!used.Add(lower))
                    {
                        // rename
                        var uname = lower + "_" + Guid.NewGuid().ToString("N");
                        // Debug.LogWarning($"texture.name: {lower} => {uname}");
                        gltfTexture.name = uname;
                        if (!used.Add(uname))
                        {
                            throw new Exception();
                        }
                    }
                }
            }
        }

        private static void FixMaterialNameUnique(glTF GLTF)
        {
            var used = new HashSet<string>();
            for (int i = 0; i < GLTF.materials.Count; ++i)
            {
                var material = GLTF.materials[i];
                var originalName = material.name;
                int j = 2;

                if (string.IsNullOrEmpty(material.name))
                {
                    material.name = $"material_{i}";
                }

                while (true)
                {
                    if (used.Add(material.name))
                    {
#if VRM_DEVELOP
                        // Debug.Log($"Material: {material.name}");
#endif
                        break;
                    }
                    material.name = string.Format("{0}({1})", originalName, j++);
                }
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
            var used = new HashSet<string>();
            for (int i = 0; i < GLTF.animations.Count; ++i)
            {
                var animation = GLTF.animations[i];
                var originalName = animation.name;
                int j = 2;

                if (string.IsNullOrEmpty(animation.name))
                {
                    animation.name = $"animation_{i}";
                }

                while (true)
                {
                    if (used.Add(animation.name))
                    {
#if VRM_DEVELOP
                        // Debug.Log($"Material: {material.name}");
#endif
                        break;
                    }
                    animation.name = string.Format("{0}({1})", originalName, j++);
                }
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
    }
}