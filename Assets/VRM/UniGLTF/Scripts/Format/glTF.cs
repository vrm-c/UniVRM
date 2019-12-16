using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    public class gltfScene : JsonSerializableBase
    {
        [JsonSchema(MinItems = 1)]
        [ItemJsonSchema(Minimum = 0)]
        public int[] nodes;

        public object extensions;
        public object extras;
        public string name;

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => nodes);
        }
    }

    [Serializable]
    public class glTF : JsonSerializableBase, IEquatable<glTF>
    {
        [JsonSchema(Required = true)]
        public glTFAssets asset = new glTFAssets();

        #region Buffer
        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFBuffer> buffers = new List<glTFBuffer>();
        public int AddBuffer(IBytesBuffer bytesBuffer)
        {
            var index = buffers.Count;
            buffers.Add(new glTFBuffer(bytesBuffer));
            return index;
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFBufferView> bufferViews = new List<glTFBufferView>();
        public int AddBufferView(glTFBufferView view)
        {
            var index = bufferViews.Count;
            bufferViews.Add(view);
            return index;
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFAccessor> accessors = new List<glTFAccessor>();

        T[] GetAttrib<T>(glTFAccessor accessor, glTFBufferView view) where T : struct
        {
            return GetAttrib<T>(accessor.count, accessor.byteOffset, view);
        }
        T[] GetAttrib<T>(int count, int byteOffset, glTFBufferView view) where T : struct
        {
            var attrib = new T[count];
            var segment = buffers[view.buffer].GetBytes();
            var bytes = new ArraySegment<Byte>(segment.Array, segment.Offset + view.byteOffset + byteOffset, count * view.byteStride);
            bytes.MarshalCopyTo(attrib);
            return attrib;
        }

        public ArraySegment<Byte> GetViewBytes(int bufferView)
        {
            var view = bufferViews[bufferView];
            var segment = buffers[view.buffer].GetBytes();
            return new ArraySegment<byte>(segment.Array, segment.Offset + view.byteOffset, view.byteLength);
        }

        IEnumerable<int> _GetIndices(glTFAccessor accessor, out int count)
        {
            count = accessor.count;
            var view = bufferViews[accessor.bufferView];
            switch ((glComponentType)accessor.componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        return GetAttrib<Byte>(accessor, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        return GetAttrib<UInt16>(accessor, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_INT:
                    {
                        return GetAttrib<UInt32>(accessor, view).Select(x => (int)(x));
                    }
            }
            throw new NotImplementedException("GetIndices: unknown componenttype: " + accessor.componentType);
        }

        IEnumerable<int> _GetIndices(glTFBufferView view, int count, int byteOffset, glComponentType componentType)
        {
            switch (componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        return GetAttrib<Byte>(count, byteOffset, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        return GetAttrib<UInt16>(count, byteOffset, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_INT:
                    {
                        return GetAttrib<UInt32>(count, byteOffset, view).Select(x => (int)(x));
                    }
            }
            throw new NotImplementedException("GetIndices: unknown componenttype: " + componentType);
        }

        public int[] GetIndices(int accessorIndex)
        {
            int count;
            var result = _GetIndices(accessors[accessorIndex], out count);
            var indices = new int[count];

            // flip triangles
            var it = result.GetEnumerator();
            {
                for (int i = 0; i < count; i += 3)
                {
                    it.MoveNext(); indices[i + 2] = it.Current;
                    it.MoveNext(); indices[i + 1] = it.Current;
                    it.MoveNext(); indices[i] = it.Current;
                }
            }

            return indices;
        }

        public T[] GetArrayFromAccessor<T>(int accessorIndex) where T : struct
        {
            var vertexAccessor = accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return new T[] { };

            var result = (vertexAccessor.bufferView != -1)
                ? GetAttrib<T>(vertexAccessor, bufferViews[vertexAccessor.bufferView])
                : new T[vertexAccessor.count]
                ;

            var sparse = vertexAccessor.sparse;
            if (sparse != null && sparse.count > 0)
            {
                // override sparse values
                var indices = _GetIndices(bufferViews[sparse.indices.bufferView], sparse.count, sparse.indices.byteOffset, sparse.indices.componentType);
                var values = GetAttrib<T>(sparse.count, sparse.values.byteOffset, bufferViews[sparse.values.bufferView]);

                var it = indices.GetEnumerator();
                for (int i = 0; i < sparse.count; ++i)
                {
                    it.MoveNext();
                    result[it.Current] = values[i];
                }
            }
            return result;
        }
        #endregion

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFTexture> textures = new List<glTFTexture>();

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFTextureSampler> samplers = new List<glTFTextureSampler>();
        public glTFTextureSampler GetSampler(int index)
        {
            if (samplers.Count == 0)
            {
                samplers.Add(new glTFTextureSampler()); // default sampler
            }

            return samplers[index];
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFImage> images = new List<glTFImage>();

        public int GetImageIndexFromTextureIndex(int textureIndex)
        {
            return textures[textureIndex].source;
        }

        public glTFImage GetImageFromTextureIndex(int textureIndex)
        {
            return images[GetImageIndexFromTextureIndex(textureIndex)];
        }

        public glTFTextureSampler GetSamplerFromTextureIndex(int textureIndex)
        {
            var samplerIndex = textures[textureIndex].sampler;
            return GetSampler(samplerIndex);
        }

        public ArraySegment<Byte> GetImageBytes(IStorage storage, int imageIndex, out string textureName)
        {
            var image = images[imageIndex];
            if (string.IsNullOrEmpty(image.uri))
            {
                //
                // use buffer view (GLB)
                //
                //m_imageBytes = ToArray(byteSegment);
                textureName = !string.IsNullOrEmpty(image.name) ? image.name : string.Format("{0:00}#GLB", imageIndex);
                return GetViewBytes(image.bufferView);
            }
            else
            {
                if (image.uri.StartsWith("data:"))
                {
                    textureName = !string.IsNullOrEmpty(image.name) ? image.name : string.Format("{0:00}#Base64Embedded", imageIndex);
                }
                else
                {
                    textureName = !string.IsNullOrEmpty(image.name) ? image.name : Path.GetFileNameWithoutExtension(image.uri);
                }
                return storage.Get(image.uri);
            }
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFMaterial> materials = new List<glTFMaterial>();
        public string GetUniqueMaterialName(int index)
        {
            if (materials.Any(x => string.IsNullOrEmpty(x.name))
                || materials.Select(x => x.name).Distinct().Count() != materials.Count)
            {
                return String.Format("{0:00}_{1}", index, materials[index].name);
            }
            else
            {
                return materials[index].name;
            }
        }

        public bool MaterialHasVertexColor(glTFMaterial material)
        {
            if (material == null)
            {
                return false;
            }

            var materialIndex = materials.IndexOf(material);
            if (materialIndex == -1)
            {
                return false;
            }

            return MaterialHasVertexColor(materialIndex);
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFMesh> meshes = new List<glTFMesh>();

        public bool MaterialHasVertexColor(int materialIndex)
        {
            if (materialIndex < 0 || materialIndex >= materials.Count)
            {
                return false;
            }

            var hasVertexColor = meshes.SelectMany(x => x.primitives).Any(x => x.material == materialIndex && x.HasVertexColor);
            return hasVertexColor;
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFNode> nodes = new List<glTFNode>();

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFSkin> skins = new List<glTFSkin>();

        [JsonSchema(Dependencies = new string[] { "scenes" }, Minimum = 0)]
        public int scene;

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<gltfScene> scenes = new List<gltfScene>();
        public int[] rootnodes
        {
            get
            {
                return scenes[scene].nodes;
            }
        }

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFAnimation> animations = new List<glTFAnimation>();

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFCamera> cameras = new List<glTFCamera>();

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<string> extensionsUsed = new List<string>();

        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<string> extensionsRequired = new List<string>();

        public glTF_extensions extensions = new glTF_extensions();
        public gltf_extras extras = new gltf_extras();

        public override string ToString()
        {
            return string.Format("{0}", asset);
        }

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            if (extensionsUsed.Count > 0)
            {
                f.Key("extensionsUsed"); f.GLTFValue(extensionsUsed);
            }
            if (extensions.__count > 0)
            {
                f.Key("extensions"); f.GLTFValue(extensions);
            }
            if (extras.__count > 0)
            {
                f.Key("extras"); f.GLTFValue(extras);
            }

            f.Key("asset"); f.GLTFValue(asset);

            // buffer
            if (buffers.Any())
            {
                f.Key("buffers"); f.GLTFValue(buffers);
            }
            if (bufferViews.Any())
            {
                f.Key("bufferViews"); f.GLTFValue(bufferViews);
            }
            if (accessors.Any())
            {
                f.Key("accessors"); f.GLTFValue(accessors);
            }

            // materials
            if (images.Any())
            {
                f.Key("images"); f.GLTFValue(images);
                if (samplers.Count == 0)
                {
                    samplers.Add(new glTFTextureSampler());
                }
            }

            if (samplers.Any())
            {
                f.Key("samplers"); f.GLTFValue(samplers);
            }

            if (textures.Any())
            {
                f.Key("textures"); f.GLTFValue(textures);
            }
            if (materials.Any())
            {
                f.Key("materials"); f.GLTFValue(materials);
            }

            // meshes
            if (meshes.Any())
            {
                f.Key("meshes"); f.GLTFValue(meshes);
            }
            if (skins.Any())
            {
                f.Key("skins"); f.GLTFValue(skins);
            }

            // scene
            if (nodes.Any())
            {
                f.Key("nodes"); f.GLTFValue(nodes);
            }
            if (scenes.Any())
            {
                f.Key("scenes"); f.GLTFValue(scenes);
                if (scene >= 0)
                {
                    f.KeyValue(() => scene);
                }
            }

            // animations
            if (animations.Any())
            {
                f.Key("animations"); f.GLTFValue(animations);
            }
        }

        public bool Equals(glTF other)
        {
            return
                textures.SequenceEqual(other.textures)
                && samplers.SequenceEqual(other.samplers)
                && images.SequenceEqual(other.images)
                && materials.SequenceEqual(other.materials)
                && meshes.SequenceEqual(other.meshes)
                && nodes.SequenceEqual(other.nodes)
                && skins.SequenceEqual(other.skins)
                && scene == other.scene
                && scenes.SequenceEqual(other.scenes)
                && animations.SequenceEqual(other.animations)
                ;
        }

        bool UsedExtension(string key)
        {
            if (extensionsUsed.Contains(key))
            {
                return true;
            }

            return false;
        }

        static Utf8String s_extensions = Utf8String.From("extensions");

        void Traverse(ListTreeNode<JsonValue> node, JsonFormatter f, Utf8String parentKey)
        {
            if(node.IsMap())
            {
                f.BeginMap();
                foreach(var kv in node.ObjectItems())
                {
                    if (parentKey == s_extensions)
                    {
                        if (!UsedExtension(kv.Key.GetString()))
                        {
                            continue;
                        }
                    }
                    f.Key(kv.Key.GetUtf8String());
                    Traverse(kv.Value, f, kv.Key.GetUtf8String());
                }
                f.EndMap();
            }
            else if(node.IsArray())
            {
                f.BeginList();
                foreach(var x in node.ArrayItems())
                {
                    Traverse(x, f, default(Utf8String));
                }
                f.EndList();
            }
            else
            {
                f.Value(node);
            }
        }

        string RemoveUnusedExtensions(string json)
        {
            var f = new JsonFormatter();

            Traverse(JsonParser.Parse(json), f, default(Utf8String));

            return f.ToString();
        }

        public byte[] ToGlbBytes(SerializerTypes serializer = SerializerTypes.UniJSON)
        {
            string json;
            if (serializer == SerializerTypes.UniJSON)
            {
                var c = new JsonSchemaValidationContext(this)
                {
                    EnableDiagnosisForNotRequiredFields = true,
                };
                json = JsonSchema.FromType(GetType()).Serialize(this, c);
            }
            else if (serializer == SerializerTypes.Generated)
            {
                var f = new JsonFormatter();
                f.GenSerialize(this);
                json = f.ToString().ParseAsJson().ToString("  ");
            }
            else if(serializer == SerializerTypes.JsonSerializable)
            {
                // Obsolete
                json = ToJson();
            }
            else
            {
                throw new Exception("[UniVRM Export Error] unknown serializer type");
            }

            RemoveUnusedExtensions(json);

            return Glb.ToBytes(json, buffers[0].GetBytes());
        }
    }
}
