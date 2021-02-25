using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniJSON;


namespace UniGLTF
{
    [Serializable]
    public class gltfScene
    {
        [JsonSchema(MinItems = 1)]
        [ItemJsonSchema(Minimum = 0)]
        public int[] nodes;

        public glTFExtension extensions;
        public glTFExtension extras;
        public string name;
    }

    [Serializable]
    public class glTF : IEquatable<glTF>
    {
        [JsonSchema(Required = true)]
        public glTFAssets asset = new glTFAssets();

        #region Buffer
        [JsonSchema(MinItems = 1, ExplicitIgnorableItemLength = 0)]
        public List<glTFBuffer> buffers = new List<glTFBuffer>();

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

        public ArraySegment<Byte> GetViewBytes(int bufferView)
        {
            var view = bufferViews[bufferView];
            var segment = buffers[view.buffer].GetBytes();
            return new ArraySegment<byte>(segment.Array, segment.Offset + view.byteOffset, view.byteLength);
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

        public glTFTextureSampler GetSamplerFromTextureIndex(int textureIndex)
        {
            var samplerIndex = textures[textureIndex].sampler;
            return GetSampler(samplerIndex);
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

        public glTFExtension extensions;
        public glTFExtension extras;

        public override string ToString()
        {
            return string.Format("{0}", asset);
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
    }
}
