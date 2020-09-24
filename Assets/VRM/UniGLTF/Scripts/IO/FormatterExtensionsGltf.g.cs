
using System;
using System.Collections.Generic;
using UniJSON;
using UnityEngine;
using VRM;

namespace UniGLTF {

    static public class IFormatterExtensionsGltf
    {


    /// gltf
    public static void GenSerialize(this IFormatter f, glTF value)
    {
        f.BeginMap(0); // dummy

        if(value.asset!=null)
        {
            f.Key("asset"); f.GenSerialize(value.asset);
        }

        if(value.buffers!=null && value.buffers.Count>0)
        {
            f.Key("buffers"); f.GenSerialize(value.buffers);
        }

        if(value.bufferViews!=null && value.bufferViews.Count>0)
        {
            f.Key("bufferViews"); f.GenSerialize(value.bufferViews);
        }

        if(value.accessors!=null && value.accessors.Count>0)
        {
            f.Key("accessors"); f.GenSerialize(value.accessors);
        }

        if(value.textures!=null && value.textures.Count>0)
        {
            f.Key("textures"); f.GenSerialize(value.textures);
        }

        if(value.samplers!=null && value.samplers.Count>0)
        {
            f.Key("samplers"); f.GenSerialize(value.samplers);
        }

        if(value.images!=null && value.images.Count>0)
        {
            f.Key("images"); f.GenSerialize(value.images);
        }

        if(value.materials!=null && value.materials.Count>0)
        {
            f.Key("materials"); f.GenSerialize(value.materials);
        }

        if(value.meshes!=null && value.meshes.Count>0)
        {
            f.Key("meshes"); f.GenSerialize(value.meshes);
        }

        if(value.nodes!=null && value.nodes.Count>0)
        {
            f.Key("nodes"); f.GenSerialize(value.nodes);
        }

        if(value.skins!=null && value.skins.Count>0)
        {
            f.Key("skins"); f.GenSerialize(value.skins);
        }

        
        {
            f.Key("scene"); f.GenSerialize(value.scene);
        }

        if(value.scenes!=null && value.scenes.Count>0)
        {
            f.Key("scenes"); f.GenSerialize(value.scenes);
        }

        if(value.animations!=null && value.animations.Count>0)
        {
            f.Key("animations"); f.GenSerialize(value.animations);
        }

        if(value.cameras!=null && value.cameras.Count>0)
        {
            f.Key("cameras"); f.GenSerialize(value.cameras);
        }

        if(value.extensionsUsed!=null && value.extensionsUsed.Count>0)
        {
            f.Key("extensionsUsed"); f.GenSerialize(value.extensionsUsed);
        }

        if(false && value.extensionsRequired!=null && value.extensionsRequired.Count>0)
        {
            f.Key("extensionsRequired"); f.GenSerialize(value.extensionsRequired);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/asset
    public static void GenSerialize(this IFormatter f, glTFAssets value)
    {
        f.BeginMap(0); // dummy

        if(value.generator!=null)
        {
            f.Key("generator"); f.GenSerialize(value.generator);
        }

        if(value.version!=null)
        {
            f.Key("version"); f.GenSerialize(value.version);
        }

        if(value.copyright!=null)
        {
            f.Key("copyright"); f.GenSerialize(value.copyright);
        }

        if(value.minVersion!=null)
        {
            f.Key("minVersion"); f.GenSerialize(value.minVersion);
        }

        f.EndMap();
    }

    public static void GenSerialize(this IFormatter f, String value)
    {
        f.Value(value);
    }

    /// gltf/buffers
    public static void GenSerialize(this IFormatter f, List<glTFBuffer> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/buffers[]
    public static void GenSerialize(this IFormatter f, glTFBuffer value)
    {
        f.BeginMap(0); // dummy

        if(value.uri!=null)
        {
            f.Key("uri"); f.GenSerialize(value.uri);
        }

        
        {
            f.Key("byteLength"); f.GenSerialize(value.byteLength);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    public static void GenSerialize(this IFormatter f, Int32 value)
    {
        f.Value(value);
    }

    /// gltf/bufferViews
    public static void GenSerialize(this IFormatter f, List<glTFBufferView> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/bufferViews[]
    public static void GenSerialize(this IFormatter f, glTFBufferView value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("buffer"); f.GenSerialize(value.buffer);
        }

        
        {
            f.Key("byteOffset"); f.GenSerialize(value.byteOffset);
        }

        
        {
            f.Key("byteLength"); f.GenSerialize(value.byteLength);
        }

        if(value.target!=0)
        {
            f.Key("target"); f.GenSerialize(value.target);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    public static void GenSerialize(this IFormatter f, glBufferTarget value)
    {
        f.Value((int)value);
    }

    /// gltf/accessors
    public static void GenSerialize(this IFormatter f, List<glTFAccessor> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/accessors[]
    public static void GenSerialize(this IFormatter f, glTFAccessor value)
    {
        f.BeginMap(0); // dummy

        if(value.bufferView>=0)
        {
            f.Key("bufferView"); f.GenSerialize(value.bufferView);
        }

        if(value.bufferView>=0)
        {
            f.Key("byteOffset"); f.GenSerialize(value.byteOffset);
        }

        if(value.type!=null)
        {
            f.Key("type"); f.GenSerialize(value.type);
        }

        
        {
            f.Key("componentType"); f.GenSerialize(value.componentType);
        }

        
        {
            f.Key("count"); f.GenSerialize(value.count);
        }

        if(value.max!=null && value.max.Length>0)
        {
            f.Key("max"); f.GenSerialize(value.max);
        }

        if(value.min!=null && value.min.Length>0)
        {
            f.Key("min"); f.GenSerialize(value.min);
        }

        
        {
            f.Key("normalized"); f.GenSerialize(value.normalized);
        }

        if(value.sparse!=null && value.sparse.count>0)
        {
            f.Key("sparse"); f.GenSerialize(value.sparse);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    public static void GenSerialize(this IFormatter f, glComponentType value)
    {
        f.Value((int)value);
    }

    /// gltf/accessors[]/max
    public static void GenSerialize(this IFormatter f, Single[] value)
    {
        f.BeginList(value.Length);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
                    
    public static void GenSerialize(this IFormatter f, Single value)
    {
        f.Value(value);
    }

    public static void GenSerialize(this IFormatter f, Boolean value)
    {
        f.Value(value);
    }

    /// gltf/accessors[]/sparse
    public static void GenSerialize(this IFormatter f, glTFSparse value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("count"); f.GenSerialize(value.count);
        }

        if(value.indices!=null)
        {
            f.Key("indices"); f.GenSerialize(value.indices);
        }

        if(value.values!=null)
        {
            f.Key("values"); f.GenSerialize(value.values);
        }

        f.EndMap();
    }

    /// gltf/accessors[]/sparse/indices
    public static void GenSerialize(this IFormatter f, glTFSparseIndices value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("bufferView"); f.GenSerialize(value.bufferView);
        }

        
        {
            f.Key("byteOffset"); f.GenSerialize(value.byteOffset);
        }

        
        {
            f.Key("componentType"); f.GenSerialize(value.componentType);
        }

        f.EndMap();
    }

    /// gltf/accessors[]/sparse/values
    public static void GenSerialize(this IFormatter f, glTFSparseValues value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("bufferView"); f.GenSerialize(value.bufferView);
        }

        
        {
            f.Key("byteOffset"); f.GenSerialize(value.byteOffset);
        }

        f.EndMap();
    }

    /// gltf/textures
    public static void GenSerialize(this IFormatter f, List<glTFTexture> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/textures[]
    public static void GenSerialize(this IFormatter f, glTFTexture value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("sampler"); f.GenSerialize(value.sampler);
        }

        
        {
            f.Key("source"); f.GenSerialize(value.source);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    /// gltf/samplers
    public static void GenSerialize(this IFormatter f, List<glTFTextureSampler> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/samplers[]
    public static void GenSerialize(this IFormatter f, glTFTextureSampler value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("magFilter"); f.GenSerialize(value.magFilter);
        }

        
        {
            f.Key("minFilter"); f.GenSerialize(value.minFilter);
        }

        
        {
            f.Key("wrapS"); f.GenSerialize(value.wrapS);
        }

        
        {
            f.Key("wrapT"); f.GenSerialize(value.wrapT);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    public static void GenSerialize(this IFormatter f, glFilter value)
    {
        f.Value((int)value);
    }

    public static void GenSerialize(this IFormatter f, glWrap value)
    {
        f.Value((int)value);
    }

    /// gltf/images
    public static void GenSerialize(this IFormatter f, List<glTFImage> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/images[]
    public static void GenSerialize(this IFormatter f, glTFImage value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.uri!=null)
        {
            f.Key("uri"); f.GenSerialize(value.uri);
        }

        
        {
            f.Key("bufferView"); f.GenSerialize(value.bufferView);
        }

        if(value.mimeType!=null)
        {
            f.Key("mimeType"); f.GenSerialize(value.mimeType);
        }

        f.EndMap();
    }

    /// gltf/materials
    public static void GenSerialize(this IFormatter f, List<glTFMaterial> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/materials[]
    public static void GenSerialize(this IFormatter f, glTFMaterial value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.pbrMetallicRoughness!=null)
        {
            f.Key("pbrMetallicRoughness"); f.GenSerialize(value.pbrMetallicRoughness);
        }

        if(value.normalTexture!=null)
        {
            f.Key("normalTexture"); f.GenSerialize(value.normalTexture);
        }

        if(value.occlusionTexture!=null)
        {
            f.Key("occlusionTexture"); f.GenSerialize(value.occlusionTexture);
        }

        if(value.emissiveTexture!=null)
        {
            f.Key("emissiveTexture"); f.GenSerialize(value.emissiveTexture);
        }

        if(value.emissiveFactor!=null)
        {
            f.Key("emissiveFactor"); f.GenSerialize(value.emissiveFactor);
        }

        if(value.alphaMode!=null)
        {
            f.Key("alphaMode"); f.GenSerialize(value.alphaMode);
        }

        if(value.alphaMode == "MASK")
        {
            f.Key("alphaCutoff"); f.GenSerialize(value.alphaCutoff);
        }

        
        {
            f.Key("doubleSided"); f.GenSerialize(value.doubleSided);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/materials[]/pbrMetallicRoughness
    public static void GenSerialize(this IFormatter f, glTFPbrMetallicRoughness value)
    {
        f.BeginMap(0); // dummy

        if(value.baseColorTexture!=null)
        {
            f.Key("baseColorTexture"); f.GenSerialize(value.baseColorTexture);
        }

        if(value.baseColorFactor!=null)
        {
            f.Key("baseColorFactor"); f.GenSerialize(value.baseColorFactor);
        }

        if(value.metallicRoughnessTexture!=null)
        {
            f.Key("metallicRoughnessTexture"); f.GenSerialize(value.metallicRoughnessTexture);
        }

        
        {
            f.Key("metallicFactor"); f.GenSerialize(value.metallicFactor);
        }

        
        {
            f.Key("roughnessFactor"); f.GenSerialize(value.roughnessFactor);
        }

        f.EndMap();
    }

    /// gltf/materials[]/pbrMetallicRoughness/baseColorTexture
    public static void GenSerialize(this IFormatter f, glTFMaterialBaseColorTextureInfo value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("texCoord"); f.GenSerialize(value.texCoord);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/materials[]/pbrMetallicRoughness/baseColorTexture/extensions
    public static void GenSerialize(this IFormatter f, glTFTextureInfo_extensions value)
    {
        f.BeginMap(0); // dummy

        if(value.KHR_texture_transform!=null)
        {
            f.Key("KHR_texture_transform"); f.GenSerialize(value.KHR_texture_transform);
        }

        f.EndMap();
    }

    /// gltf/materials[]/pbrMetallicRoughness/baseColorTexture/extensions/KHR_texture_transform
    public static void GenSerialize(this IFormatter f, glTF_KHR_texture_transform value)
    {
        f.BeginMap(0); // dummy

        if(value.offset!=null)
        {
            f.Key("offset"); f.GenSerialize(value.offset);
        }

        
        {
            f.Key("rotation"); f.GenSerialize(value.rotation);
        }

        if(value.scale!=null)
        {
            f.Key("scale"); f.GenSerialize(value.scale);
        }

        
        {
            f.Key("texCoord"); f.GenSerialize(value.texCoord);
        }

        f.EndMap();
    }

    /// gltf/materials[]/pbrMetallicRoughness/metallicRoughnessTexture
    public static void GenSerialize(this IFormatter f, glTFMaterialMetallicRoughnessTextureInfo value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("texCoord"); f.GenSerialize(value.texCoord);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/materials[]/normalTexture
    public static void GenSerialize(this IFormatter f, glTFMaterialNormalTextureInfo value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("scale"); f.GenSerialize(value.scale);
        }

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("texCoord"); f.GenSerialize(value.texCoord);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/materials[]/occlusionTexture
    public static void GenSerialize(this IFormatter f, glTFMaterialOcclusionTextureInfo value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("strength"); f.GenSerialize(value.strength);
        }

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("texCoord"); f.GenSerialize(value.texCoord);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/materials[]/emissiveTexture
    public static void GenSerialize(this IFormatter f, glTFMaterialEmissiveTextureInfo value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("texCoord"); f.GenSerialize(value.texCoord);
        }

        if(value.extensions!=null)
        {
            f.Key("extensions"); f.GenSerialize(value.extensions);
        }

        f.EndMap();
    }

    /// gltf/materials[]/extensions
    public static void GenSerialize(this IFormatter f, glTFMaterial_extensions value)
    {
        f.BeginMap(0); // dummy

        if(value.KHR_materials_unlit!=null)
        {
            f.Key("KHR_materials_unlit"); f.GenSerialize(value.KHR_materials_unlit);
        }

        f.EndMap();
    }

    /// gltf/materials[]/extensions/KHR_materials_unlit
    public static void GenSerialize(this IFormatter f, glTF_KHR_materials_unlit value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/meshes
    public static void GenSerialize(this IFormatter f, List<glTFMesh> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/meshes[]
    public static void GenSerialize(this IFormatter f, glTFMesh value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.primitives!=null && value.primitives.Count>0)
        {
            f.Key("primitives"); f.GenSerialize(value.primitives);
        }

        if(value.weights!=null && value.weights.Length>0)
        {
            f.Key("weights"); f.GenSerialize(value.weights);
        }

        if(value.extras!=null)
        {
            f.Key("extras"); f.GenSerialize(value.extras);
        }

        f.EndMap();
    }

    /// gltf/meshes[]/primitives
    public static void GenSerialize(this IFormatter f, List<glTFPrimitives> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/meshes[]/primitives[]
    public static void GenSerialize(this IFormatter f, glTFPrimitives value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("mode"); f.GenSerialize(value.mode);
        }

        
        {
            f.Key("indices"); f.GenSerialize(value.indices);
        }

        if(value.attributes!=null)
        {
            f.Key("attributes"); f.GenSerialize(value.attributes);
        }

        
        {
            f.Key("material"); f.GenSerialize(value.material);
        }

        if(value.targets!=null && value.targets.Count>0)
        {
            f.Key("targets"); f.GenSerialize(value.targets);
        }

        if(value.extras!=null && value.extras.targetNames!=null && value.extras.targetNames.Count>0)
        {
            f.Key("extras"); f.GenSerialize(value.extras);
        }

        f.EndMap();
    }

    /// gltf/meshes[]/primitives[]/attributes
    public static void GenSerialize(this IFormatter f, glTFAttributes value)
    {
        f.BeginMap(0); // dummy

        if(value.POSITION!=-1)
        {
            f.Key("POSITION"); f.GenSerialize(value.POSITION);
        }

        if(value.NORMAL!=-1)
        {
            f.Key("NORMAL"); f.GenSerialize(value.NORMAL);
        }

        if(value.TANGENT!=-1)
        {
            f.Key("TANGENT"); f.GenSerialize(value.TANGENT);
        }

        if(value.TEXCOORD_0!=-1)
        {
            f.Key("TEXCOORD_0"); f.GenSerialize(value.TEXCOORD_0);
        }

        if(value.COLOR_0!=-1)
        {
            f.Key("COLOR_0"); f.GenSerialize(value.COLOR_0);
        }

        if(value.JOINTS_0!=-1)
        {
            f.Key("JOINTS_0"); f.GenSerialize(value.JOINTS_0);
        }

        if(value.WEIGHTS_0!=-1)
        {
            f.Key("WEIGHTS_0"); f.GenSerialize(value.WEIGHTS_0);
        }

        f.EndMap();
    }

    /// gltf/meshes[]/primitives[]/targets
    public static void GenSerialize(this IFormatter f, List<gltfMorphTarget> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/meshes[]/primitives[]/targets[]
    public static void GenSerialize(this IFormatter f, gltfMorphTarget value)
    {
        f.BeginMap(0); // dummy

        if(value.POSITION!=-1)
        {
            f.Key("POSITION"); f.GenSerialize(value.POSITION);
        }

        if(value.NORMAL!=-1)
        {
            f.Key("NORMAL"); f.GenSerialize(value.NORMAL);
        }

        if(value.TANGENT!=-1)
        {
            f.Key("TANGENT"); f.GenSerialize(value.TANGENT);
        }

        f.EndMap();
    }

    /// gltf/meshes[]/primitives[]/extras
    public static void GenSerialize(this IFormatter f, glTFPrimitives_extras value)
    {
        f.BeginMap(0); // dummy

        if(value.targetNames!=null)
        {
            f.Key("targetNames"); f.GenSerialize(value.targetNames);
        }

        f.EndMap();
    }

    /// gltf/meshes[]/primitives[]/extras/targetNames
    public static void GenSerialize(this IFormatter f, List<String> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/meshes[]/primitives[]/extensions
    public static void GenSerialize(this IFormatter f, glTFPrimitives_extensions value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/meshes[]/extras
    public static void GenSerialize(this IFormatter f, glTFMesh_extras value)
    {
        f.BeginMap(0); // dummy

        if(value.targetNames!=null)
        {
            f.Key("targetNames"); f.GenSerialize(value.targetNames);
        }

        f.EndMap();
    }

    /// gltf/nodes
    public static void GenSerialize(this IFormatter f, List<glTFNode> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/nodes[]
    public static void GenSerialize(this IFormatter f, glTFNode value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.children != null && value.children.Length>0)
        {
            f.Key("children"); f.GenSerialize(value.children);
        }

        if(value.matrix!=null)
        {
            f.Key("matrix"); f.GenSerialize(value.matrix);
        }

        if(value.translation!=null)
        {
            f.Key("translation"); f.GenSerialize(value.translation);
        }

        if(value.rotation!=null)
        {
            f.Key("rotation"); f.GenSerialize(value.rotation);
        }

        if(value.scale!=null)
        {
            f.Key("scale"); f.GenSerialize(value.scale);
        }

        if(value.mesh!=-1)
        {
            f.Key("mesh"); f.GenSerialize(value.mesh);
        }

        if(value.skin!=-1)
        {
            f.Key("skin"); f.GenSerialize(value.skin);
        }

        if(value.weights!=null)
        {
            f.Key("weights"); f.GenSerialize(value.weights);
        }

        if(value.camera!=-1)
        {
            f.Key("camera"); f.GenSerialize(value.camera);
        }

        f.EndMap();
    }

    /// gltf/nodes[]/children
    public static void GenSerialize(this IFormatter f, Int32[] value)
    {
        f.BeginList(value.Length);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }
                    
    /// gltf/nodes[]/extensions
    public static void GenSerialize(this IFormatter f, glTFNode_extensions value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/nodes[]/extras
    public static void GenSerialize(this IFormatter f, glTFNode_extra value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/skins
    public static void GenSerialize(this IFormatter f, List<glTFSkin> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/skins[]
    public static void GenSerialize(this IFormatter f, glTFSkin value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("inverseBindMatrices"); f.GenSerialize(value.inverseBindMatrices);
        }

        if(value.joints!=null && value.joints.Length>0)
        {
            f.Key("joints"); f.GenSerialize(value.joints);
        }

        if(value.skeleton!=-1)
        {
            f.Key("skeleton"); f.GenSerialize(value.skeleton);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    /// gltf/scenes
    public static void GenSerialize(this IFormatter f, List<gltfScene> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/scenes[]
    public static void GenSerialize(this IFormatter f, gltfScene value)
    {
        f.BeginMap(0); // dummy

        if(value.nodes!=null && value.nodes.Length>0)
        {
            f.Key("nodes"); f.GenSerialize(value.nodes);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    /// gltf/animations
    public static void GenSerialize(this IFormatter f, List<glTFAnimation> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/animations[]
    public static void GenSerialize(this IFormatter f, glTFAnimation value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.channels!=null && value.channels.Count>0)
        {
            f.Key("channels"); f.GenSerialize(value.channels);
        }

        if(value.samplers!=null && value.samplers.Count>0)
        {
            f.Key("samplers"); f.GenSerialize(value.samplers);
        }

        f.EndMap();
    }

    /// gltf/animations[]/channels
    public static void GenSerialize(this IFormatter f, List<glTFAnimationChannel> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/animations[]/channels[]
    public static void GenSerialize(this IFormatter f, glTFAnimationChannel value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("sampler"); f.GenSerialize(value.sampler);
        }

        if(value!=null)
        {
            f.Key("target"); f.GenSerialize(value.target);
        }

        f.EndMap();
    }

    /// gltf/animations[]/channels[]/target
    public static void GenSerialize(this IFormatter f, glTFAnimationTarget value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("node"); f.GenSerialize(value.node);
        }

        if(value.path!=null)
        {
            f.Key("path"); f.GenSerialize(value.path);
        }

        f.EndMap();
    }

    /// gltf/animations[]/samplers
    public static void GenSerialize(this IFormatter f, List<glTFAnimationSampler> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/animations[]/samplers[]
    public static void GenSerialize(this IFormatter f, glTFAnimationSampler value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("input"); f.GenSerialize(value.input);
        }

        if(value.interpolation!=null)
        {
            f.Key("interpolation"); f.GenSerialize(value.interpolation);
        }

        
        {
            f.Key("output"); f.GenSerialize(value.output);
        }

        f.EndMap();
    }

    /// gltf/cameras
    public static void GenSerialize(this IFormatter f, List<glTFCamera> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/cameras[]
    public static void GenSerialize(this IFormatter f, glTFCamera value)
    {
        f.BeginMap(0); // dummy

        if(value.orthographic!=null)
        {
            f.Key("orthographic"); f.GenSerialize(value.orthographic);
        }

        if(value.perspective!=null)
        {
            f.Key("perspective"); f.GenSerialize(value.perspective);
        }

        
        {
            f.Key("type"); f.GenSerialize(value.type);
        }

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        f.EndMap();
    }

    /// gltf/cameras[]/orthographic
    public static void GenSerialize(this IFormatter f, glTFOrthographic value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("xmag"); f.GenSerialize(value.xmag);
        }

        
        {
            f.Key("ymag"); f.GenSerialize(value.ymag);
        }

        
        {
            f.Key("zfar"); f.GenSerialize(value.zfar);
        }

        
        {
            f.Key("znear"); f.GenSerialize(value.znear);
        }

        f.EndMap();
    }

    /// gltf/cameras[]/orthographic/extensions
    public static void GenSerialize(this IFormatter f, glTFOrthographic_extensions value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/cameras[]/orthographic/extras
    public static void GenSerialize(this IFormatter f, glTFOrthographic_extras value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/cameras[]/perspective
    public static void GenSerialize(this IFormatter f, glTFPerspective value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("aspectRatio"); f.GenSerialize(value.aspectRatio);
        }

        
        {
            f.Key("yfov"); f.GenSerialize(value.yfov);
        }

        
        {
            f.Key("zfar"); f.GenSerialize(value.zfar);
        }

        
        {
            f.Key("znear"); f.GenSerialize(value.znear);
        }

        f.EndMap();
    }

    /// gltf/cameras[]/perspective/extensions
    public static void GenSerialize(this IFormatter f, glTFPerspective_extensions value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/cameras[]/perspective/extras
    public static void GenSerialize(this IFormatter f, glTFPerspective_extras value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    public static void GenSerialize(this IFormatter f, ProjectionType value)
    {
        f.Value((int)value);
    }

    /// gltf/cameras[]/extensions
    public static void GenSerialize(this IFormatter f, glTFCamera_extensions value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/cameras[]/extras
    public static void GenSerialize(this IFormatter f, glTFCamera_extras value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    /// gltf/extensions
    public static void GenSerialize(this IFormatter f, glTF_extensions value)
    {
        f.BeginMap(0); // dummy

        if(value.VRM!=null)
        {
            f.Key("VRM"); f.GenSerialize(value.VRM);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM
    public static void GenSerialize(this IFormatter f, glTF_VRM_extensions value)
    {
        f.BeginMap(0); // dummy

        if(value.exporterVersion!=null)
        {
            f.Key("exporterVersion"); f.GenSerialize(value.exporterVersion);
        }

        if(value.specVersion!=null)
        {
            f.Key("specVersion"); f.GenSerialize(value.specVersion);
        }

        if(value.meta!=null)
        {
            f.Key("meta"); f.GenSerialize(value.meta);
        }

        if(value.humanoid!=null)
        {
            f.Key("humanoid"); f.GenSerialize(value.humanoid);
        }

        if(value.firstPerson!=null)
        {
            f.Key("firstPerson"); f.GenSerialize(value.firstPerson);
        }

        if(value.blendShapeMaster!=null)
        {
            f.Key("blendShapeMaster"); f.GenSerialize(value.blendShapeMaster);
        }

        if(value.secondaryAnimation!=null)
        {
            f.Key("secondaryAnimation"); f.GenSerialize(value.secondaryAnimation);
        }

        if(value.materialProperties!=null)
        {
            f.Key("materialProperties"); f.GenSerialize(value.materialProperties);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/meta
    public static void GenSerialize(this IFormatter f, glTF_VRM_Meta value)
    {
        f.BeginMap(0); // dummy

        if(value.title!=null)
        {
            f.Key("title"); f.GenSerialize(value.title);
        }

        if(value.version!=null)
        {
            f.Key("version"); f.GenSerialize(value.version);
        }

        if(value.author!=null)
        {
            f.Key("author"); f.GenSerialize(value.author);
        }

        if(value.contactInformation!=null)
        {
            f.Key("contactInformation"); f.GenSerialize(value.contactInformation);
        }

        if(value.reference!=null)
        {
            f.Key("reference"); f.GenSerialize(value.reference);
        }

        
        {
            f.Key("texture"); f.GenSerialize(value.texture);
        }

        if(value.allowedUserName!=null)
        {
            f.Key("allowedUserName"); f.GenSerialize(value.allowedUserName);
        }

        if(value.violentUssageName!=null)
        {
            f.Key("violentUssageName"); f.GenSerialize(value.violentUssageName);
        }

        if(value.sexualUssageName!=null)
        {
            f.Key("sexualUssageName"); f.GenSerialize(value.sexualUssageName);
        }

        if(value.commercialUssageName!=null)
        {
            f.Key("commercialUssageName"); f.GenSerialize(value.commercialUssageName);
        }

        if(value.otherPermissionUrl!=null)
        {
            f.Key("otherPermissionUrl"); f.GenSerialize(value.otherPermissionUrl);
        }

        if(value.licenseName!=null)
        {
            f.Key("licenseName"); f.GenSerialize(value.licenseName);
        }

        if(value.otherLicenseUrl!=null)
        {
            f.Key("otherLicenseUrl"); f.GenSerialize(value.otherLicenseUrl);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/humanoid
    public static void GenSerialize(this IFormatter f, glTF_VRM_Humanoid value)
    {
        f.BeginMap(0); // dummy

        if(value.humanBones!=null)
        {
            f.Key("humanBones"); f.GenSerialize(value.humanBones);
        }

        
        {
            f.Key("armStretch"); f.GenSerialize(value.armStretch);
        }

        
        {
            f.Key("legStretch"); f.GenSerialize(value.legStretch);
        }

        
        {
            f.Key("upperArmTwist"); f.GenSerialize(value.upperArmTwist);
        }

        
        {
            f.Key("lowerArmTwist"); f.GenSerialize(value.lowerArmTwist);
        }

        
        {
            f.Key("upperLegTwist"); f.GenSerialize(value.upperLegTwist);
        }

        
        {
            f.Key("lowerLegTwist"); f.GenSerialize(value.lowerLegTwist);
        }

        
        {
            f.Key("feetSpacing"); f.GenSerialize(value.feetSpacing);
        }

        
        {
            f.Key("hasTranslationDoF"); f.GenSerialize(value.hasTranslationDoF);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/humanoid/humanBones
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_HumanoidBone> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/humanoid/humanBones[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_HumanoidBone value)
    {
        f.BeginMap(0); // dummy

        if(value.bone!=null)
        {
            f.Key("bone"); f.GenSerialize(value.bone);
        }

        
        {
            f.Key("node"); f.GenSerialize(value.node);
        }

        
        {
            f.Key("useDefaultValues"); f.GenSerialize(value.useDefaultValues);
        }

        if(value.min!=Vector3.zero)
        {
            f.Key("min"); f.GenSerialize(value.min);
        }

        if(value.max!=Vector3.zero)
        {
            f.Key("max"); f.GenSerialize(value.max);
        }

        if(value.center!=Vector3.zero)
        {
            f.Key("center"); f.GenSerialize(value.center);
        }

        if(value.axisLength>0)
        {
            f.Key("axisLength"); f.GenSerialize(value.axisLength);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/humanoid/humanBones[]/min
    public static void GenSerialize(this IFormatter f, Vector3 value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("x"); f.GenSerialize(value.x);
        }

        
        {
            f.Key("y"); f.GenSerialize(value.y);
        }

        
        {
            f.Key("z"); f.GenSerialize(value.z);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/firstPerson
    public static void GenSerialize(this IFormatter f, glTF_VRM_Firstperson value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("firstPersonBone"); f.GenSerialize(value.firstPersonBone);
        }

        
        {
            f.Key("firstPersonBoneOffset"); f.GenSerialize(value.firstPersonBoneOffset);
        }

        if(value.meshAnnotations!=null)
        {
            f.Key("meshAnnotations"); f.GenSerialize(value.meshAnnotations);
        }

        if(value.lookAtTypeName!=null)
        {
            f.Key("lookAtTypeName"); f.GenSerialize(value.lookAtTypeName);
        }

        if(value.lookAtHorizontalInner!=null)
        {
            f.Key("lookAtHorizontalInner"); f.GenSerialize(value.lookAtHorizontalInner);
        }

        if(value.lookAtHorizontalOuter!=null)
        {
            f.Key("lookAtHorizontalOuter"); f.GenSerialize(value.lookAtHorizontalOuter);
        }

        if(value.lookAtVerticalDown!=null)
        {
            f.Key("lookAtVerticalDown"); f.GenSerialize(value.lookAtVerticalDown);
        }

        if(value.lookAtVerticalUp!=null)
        {
            f.Key("lookAtVerticalUp"); f.GenSerialize(value.lookAtVerticalUp);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/firstPerson/meshAnnotations
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_MeshAnnotation> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/firstPerson/meshAnnotations[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_MeshAnnotation value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("mesh"); f.GenSerialize(value.mesh);
        }

        if(value.firstPersonFlag!=null)
        {
            f.Key("firstPersonFlag"); f.GenSerialize(value.firstPersonFlag);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/firstPerson/lookAtHorizontalInner
    public static void GenSerialize(this IFormatter f, glTF_VRM_DegreeMap value)
    {
        f.BeginMap(0); // dummy

        if(value.curve!=null)
        {
            f.Key("curve"); f.GenSerialize(value.curve);
        }

        
        {
            f.Key("xRange"); f.GenSerialize(value.xRange);
        }

        
        {
            f.Key("yRange"); f.GenSerialize(value.yRange);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/blendShapeMaster
    public static void GenSerialize(this IFormatter f, glTF_VRM_BlendShapeMaster value)
    {
        f.BeginMap(0); // dummy

        if(value.blendShapeGroups!=null)
        {
            f.Key("blendShapeGroups"); f.GenSerialize(value.blendShapeGroups);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/blendShapeMaster/blendShapeGroups
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_BlendShapeGroup> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/blendShapeMaster/blendShapeGroups[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_BlendShapeGroup value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.presetName!=null)
        {
            f.Key("presetName"); f.GenSerialize(value.presetName);
        }

        if(value.binds!=null)
        {
            f.Key("binds"); f.GenSerialize(value.binds);
        }

        if(value.materialValues!=null)
        {
            f.Key("materialValues"); f.GenSerialize(value.materialValues);
        }

        
        {
            f.Key("isBinary"); f.GenSerialize(value.isBinary);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/blendShapeMaster/blendShapeGroups[]/binds
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_BlendShapeBind> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/blendShapeMaster/blendShapeGroups[]/binds[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_BlendShapeBind value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("mesh"); f.GenSerialize(value.mesh);
        }

        
        {
            f.Key("index"); f.GenSerialize(value.index);
        }

        
        {
            f.Key("weight"); f.GenSerialize(value.weight);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/blendShapeMaster/blendShapeGroups[]/materialValues
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_MaterialValueBind> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/blendShapeMaster/blendShapeGroups[]/materialValues[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_MaterialValueBind value)
    {
        f.BeginMap(0); // dummy

        if(value.materialName!=null)
        {
            f.Key("materialName"); f.GenSerialize(value.materialName);
        }

        if(value.propertyName!=null)
        {
            f.Key("propertyName"); f.GenSerialize(value.propertyName);
        }

        if(value.targetValue!=null)
        {
            f.Key("targetValue"); f.GenSerialize(value.targetValue);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/secondaryAnimation
    public static void GenSerialize(this IFormatter f, glTF_VRM_SecondaryAnimation value)
    {
        f.BeginMap(0); // dummy

        if(value.boneGroups!=null)
        {
            f.Key("boneGroups"); f.GenSerialize(value.boneGroups);
        }

        if(value.colliderGroups!=null)
        {
            f.Key("colliderGroups"); f.GenSerialize(value.colliderGroups);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/secondaryAnimation/boneGroups
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_SecondaryAnimationGroup> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/secondaryAnimation/boneGroups[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_SecondaryAnimationGroup value)
    {
        f.BeginMap(0); // dummy

        if(value.comment!=null)
        {
            f.Key("comment"); f.GenSerialize(value.comment);
        }

        
        {
            f.Key("stiffiness"); f.GenSerialize(value.stiffiness);
        }

        
        {
            f.Key("gravityPower"); f.GenSerialize(value.gravityPower);
        }

        
        {
            f.Key("gravityDir"); f.GenSerialize(value.gravityDir);
        }

        
        {
            f.Key("dragForce"); f.GenSerialize(value.dragForce);
        }

        
        {
            f.Key("center"); f.GenSerialize(value.center);
        }

        
        {
            f.Key("hitRadius"); f.GenSerialize(value.hitRadius);
        }

        if(value.bones!=null)
        {
            f.Key("bones"); f.GenSerialize(value.bones);
        }

        if(value.colliderGroups!=null)
        {
            f.Key("colliderGroups"); f.GenSerialize(value.colliderGroups);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/secondaryAnimation/colliderGroups
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_SecondaryAnimationColliderGroup> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/secondaryAnimation/colliderGroups[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_SecondaryAnimationColliderGroup value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("node"); f.GenSerialize(value.node);
        }

        if(value.colliders!=null)
        {
            f.Key("colliders"); f.GenSerialize(value.colliders);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/secondaryAnimation/colliderGroups[]/colliders
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_SecondaryAnimationCollider> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/secondaryAnimation/colliderGroups[]/colliders[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_SecondaryAnimationCollider value)
    {
        f.BeginMap(0); // dummy

        
        {
            f.Key("offset"); f.GenSerialize(value.offset);
        }

        
        {
            f.Key("radius"); f.GenSerialize(value.radius);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/materialProperties
    public static void GenSerialize(this IFormatter f, List<glTF_VRM_Material> value)
    {
        f.BeginList(value.Count);
        foreach (var x in value)
        {
            f.GenSerialize(x);
        }
        f.EndList();
    }

    /// gltf/extensions/VRM/materialProperties[]
    public static void GenSerialize(this IFormatter f, glTF_VRM_Material value)
    {
        f.BeginMap(0); // dummy

        if(value.name!=null)
        {
            f.Key("name"); f.GenSerialize(value.name);
        }

        if(value.shader!=null)
        {
            f.Key("shader"); f.GenSerialize(value.shader);
        }

        
        {
            f.Key("renderQueue"); f.GenSerialize(value.renderQueue);
        }

        if(value.floatProperties!=null)
        {
            f.Key("floatProperties"); f.GenSerialize(value.floatProperties);
        }

        if(value.vectorProperties!=null)
        {
            f.Key("vectorProperties"); f.GenSerialize(value.vectorProperties);
        }

        if(value.textureProperties!=null)
        {
            f.Key("textureProperties"); f.GenSerialize(value.textureProperties);
        }

        if(value.keywordMap!=null)
        {
            f.Key("keywordMap"); f.GenSerialize(value.keywordMap);
        }

        if(value.tagMap!=null)
        {
            f.Key("tagMap"); f.GenSerialize(value.tagMap);
        }

        f.EndMap();
    }

    /// gltf/extensions/VRM/materialProperties[]/floatProperties
    public static void GenSerialize(this IFormatter f, Dictionary<string, Single> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// gltf/extensions/VRM/materialProperties[]/vectorProperties
    public static void GenSerialize(this IFormatter f, Dictionary<string, Single[]> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// gltf/extensions/VRM/materialProperties[]/textureProperties
    public static void GenSerialize(this IFormatter f, Dictionary<string, Int32> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// gltf/extensions/VRM/materialProperties[]/keywordMap
    public static void GenSerialize(this IFormatter f, Dictionary<string, Boolean> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// gltf/extensions/VRM/materialProperties[]/tagMap
    public static void GenSerialize(this IFormatter f, Dictionary<string, String> value)
    {
        f.BeginMap(value.Count);
        foreach (var kv in value)
        {
            f.Key(kv.Key);
            f.GenSerialize(kv.Value);
        }
        f.EndMap();
    }


    /// gltf/extras
    public static void GenSerialize(this IFormatter f, gltf_extras value)
    {
        f.BeginMap(0); // dummy

        f.EndMap();
    }

    } // class
} // namespace
