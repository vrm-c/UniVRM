using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UniJSON;
using UnityEngine;

namespace UniGLTF
{
    public class GltfParser
    {
        /// <summary>
        /// JSON source
        /// </summary>
        public String Json;

        /// <summary>
        /// GLTF parsed from JSON
        /// </summary>
        public glTF GLTF;

        /// <summary>
        /// URI access
        /// </summary>
        public IStorage Storage;


        #region Parse
        public void ParsePath(string path)
        {
            Parse(path, File.ReadAllBytes(path));
        }

        string m_targetPath;
        public string TargetPath
        {
            get => m_targetPath;
            set
            {
                m_targetPath = Path.GetFullPath(value);
            }
        }

        /// <summary>
        /// Parse gltf json or Parse json chunk of glb
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public virtual void Parse(string path, Byte[] bytes)
        {
            TargetPath = path;
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".gltf":
                    ParseJson(Encoding.UTF8.GetString(bytes), new FileSystemStorage(Path.GetDirectoryName(path)));
                    break;

                case ".zip":
                    {
                        var zipArchive = Zip.ZipArchiveStorage.Parse(bytes);
                        var gltf = zipArchive.Entries.FirstOrDefault(x => x.FileName.ToLower().EndsWith(".gltf"));
                        if (gltf == null)
                        {
                            throw new Exception("no gltf in archive");
                        }
                        var jsonBytes = zipArchive.Extract(gltf);
                        var json = Encoding.UTF8.GetString(jsonBytes);
                        ParseJson(json, zipArchive);
                    }
                    break;

                default:
                    ParseGlb(bytes);
                    break;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bytes"></param>
        public void ParseGlb(Byte[] bytes)
        {
            var chunks = glbImporter.ParseGlbChunks(bytes);

            if (chunks.Count != 2)
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

            try
            {
                var jsonBytes = chunks[0].Bytes;
                ParseJson(Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                    new SimpleStorage(chunks[1].Bytes));
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

        public virtual void ParseJson(string json, IStorage storage)
        {
            Json = json;
            Storage = storage;
            GLTF = GltfDeserializer.Deserialize(json.ParseAsJson());
            if (GLTF.asset.version != "2.0")
            {
                throw new UniGLTFException("unknown gltf version {0}", GLTF.asset.version);
            }

            // Version Compatibility
            RestoreOlderVersionValues();

            FixMeshNameUnique();
            foreach(var image in GLTF.images)
            {
                image.uri = PrepareUri(image.uri);
            }
            FixTextureNameUnique();
            FixMaterialNameUnique();
            FixNodeName();

            // parepare byte buffer
            //GLTF.baseDir = System.IO.Path.GetDirectoryName(Path);
            foreach (var buffer in GLTF.buffers)
            {
                buffer.OpenStorage(storage);
            }
        }

        void FixMeshNameUnique()
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

        void RenameImageFromTexture(int i)
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
        void FixTextureNameUnique()
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

        public void FixMaterialNameUnique()
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
        void FixNodeName()
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

        void RestoreOlderVersionValues()
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
        #endregion

        public static void AppendImageExtension(glTFImage texture, string extension)
        {
            if (!texture.name.EndsWith(extension))
            {
                texture.name = texture.name + extension;
            }
        }
    }
}
