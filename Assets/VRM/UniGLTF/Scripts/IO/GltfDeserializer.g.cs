
using UniJSON;
using System;
using System.Collections.Generic;
using VRM;
using UnityEngine;

namespace UniGLTF {

public static class GltfDeserializer
{


public static glTF Deserialize(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="asset"){
            value.asset = Deserialize_gltf_asset(kv.Value);
            continue;
        }

        if(key=="buffers"){
            value.buffers = Deserialize_gltf_buffers(kv.Value);
            continue;
        }

        if(key=="bufferViews"){
            value.bufferViews = Deserialize_gltf_bufferViews(kv.Value);
            continue;
        }

        if(key=="accessors"){
            value.accessors = Deserialize_gltf_accessors(kv.Value);
            continue;
        }

        if(key=="textures"){
            value.textures = Deserialize_gltf_textures(kv.Value);
            continue;
        }

        if(key=="samplers"){
            value.samplers = Deserialize_gltf_samplers(kv.Value);
            continue;
        }

        if(key=="images"){
            value.images = Deserialize_gltf_images(kv.Value);
            continue;
        }

        if(key=="materials"){
            value.materials = Deserialize_gltf_materials(kv.Value);
            continue;
        }

        if(key=="meshes"){
            value.meshes = Deserialize_gltf_meshes(kv.Value);
            continue;
        }

        if(key=="nodes"){
            value.nodes = Deserialize_gltf_nodes(kv.Value);
            continue;
        }

        if(key=="skins"){
            value.skins = Deserialize_gltf_skins(kv.Value);
            continue;
        }

        if(key=="scene"){
            value.scene = kv.Value.GetInt32();
            continue;
        }

        if(key=="scenes"){
            value.scenes = Deserialize_gltf_scenes(kv.Value);
            continue;
        }

        if(key=="animations"){
            value.animations = Deserialize_gltf_animations(kv.Value);
            continue;
        }

        if(key=="cameras"){
            value.cameras = Deserialize_gltf_cameras(kv.Value);
            continue;
        }

        if(key=="extensionsUsed"){
            value.extensionsUsed = Deserialize_gltf_extensionsUsed(kv.Value);
            continue;
        }

        if(key=="extensionsRequired"){
            value.extensionsRequired = Deserialize_gltf_extensionsRequired(kv.Value);
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_extensions(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = Deserialize_gltf_extras(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFAssets Deserialize_gltf_asset(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAssets();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="generator"){
            value.generator = kv.Value.GetString();
            continue;
        }

        if(key=="version"){
            value.version = kv.Value.GetString();
            continue;
        }

        if(key=="copyright"){
            value.copyright = kv.Value.GetString();
            continue;
        }

        if(key=="minVersion"){
            value.minVersion = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFBuffer> Deserialize_gltf_buffers(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFBuffer>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_buffers_LIST(x));
    }
	return value;
}
public static glTFBuffer Deserialize_gltf_buffers_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFBuffer();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="uri"){
            value.uri = kv.Value.GetString();
            continue;
        }

        if(key=="byteLength"){
            value.byteLength = kv.Value.GetInt32();
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFBufferView> Deserialize_gltf_bufferViews(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFBufferView>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_bufferViews_LIST(x));
    }
	return value;
}
public static glTFBufferView Deserialize_gltf_bufferViews_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFBufferView();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="buffer"){
            value.buffer = kv.Value.GetInt32();
            continue;
        }

        if(key=="byteOffset"){
            value.byteOffset = kv.Value.GetInt32();
            continue;
        }

        if(key=="byteLength"){
            value.byteLength = kv.Value.GetInt32();
            continue;
        }

        if(key=="byteStride"){
            value.byteStride = kv.Value.GetInt32();
            continue;
        }

        if(key=="target"){
            value.target = (glBufferTarget)kv.Value.GetInt32();
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFAccessor> Deserialize_gltf_accessors(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFAccessor>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_accessors_LIST(x));
    }
	return value;
}
public static glTFAccessor Deserialize_gltf_accessors_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAccessor();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="bufferView"){
            value.bufferView = kv.Value.GetInt32();
            continue;
        }

        if(key=="byteOffset"){
            value.byteOffset = kv.Value.GetInt32();
            continue;
        }

        if(key=="type"){
            value.type = kv.Value.GetString();
            continue;
        }

        if(key=="componentType"){
            value.componentType = (glComponentType)kv.Value.GetInt32();
            continue;
        }

        if(key=="count"){
            value.count = kv.Value.GetInt32();
            continue;
        }

        if(key=="max"){
            value.max = Deserialize_gltf_accessors__max(kv.Value);
            continue;
        }

        if(key=="min"){
            value.min = Deserialize_gltf_accessors__min(kv.Value);
            continue;
        }

        if(key=="normalized"){
            value.normalized = kv.Value.GetBoolean();
            continue;
        }

        if(key=="sparse"){
            value.sparse = Deserialize_gltf_accessors__sparse(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_accessors__max(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_accessors__min(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTFSparse Deserialize_gltf_accessors__sparse(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFSparse();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="count"){
            value.count = kv.Value.GetInt32();
            continue;
        }

        if(key=="indices"){
            value.indices = Deserialize_gltf_accessors__sparse_indices(kv.Value);
            continue;
        }

        if(key=="values"){
            value.values = Deserialize_gltf_accessors__sparse_values(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFSparseIndices Deserialize_gltf_accessors__sparse_indices(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFSparseIndices();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="bufferView"){
            value.bufferView = kv.Value.GetInt32();
            continue;
        }

        if(key=="byteOffset"){
            value.byteOffset = kv.Value.GetInt32();
            continue;
        }

        if(key=="componentType"){
            value.componentType = (glComponentType)kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static glTFSparseValues Deserialize_gltf_accessors__sparse_values(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFSparseValues();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="bufferView"){
            value.bufferView = kv.Value.GetInt32();
            continue;
        }

        if(key=="byteOffset"){
            value.byteOffset = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFTexture> Deserialize_gltf_textures(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFTexture>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_textures_LIST(x));
    }
	return value;
}
public static glTFTexture Deserialize_gltf_textures_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFTexture();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="sampler"){
            value.sampler = kv.Value.GetInt32();
            continue;
        }

        if(key=="source"){
            value.source = kv.Value.GetInt32();
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFTextureSampler> Deserialize_gltf_samplers(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFTextureSampler>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_samplers_LIST(x));
    }
	return value;
}
public static glTFTextureSampler Deserialize_gltf_samplers_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFTextureSampler();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="magFilter"){
            value.magFilter = (glFilter)kv.Value.GetInt32();
            continue;
        }

        if(key=="minFilter"){
            value.minFilter = (glFilter)kv.Value.GetInt32();
            continue;
        }

        if(key=="wrapS"){
            value.wrapS = (glWrap)kv.Value.GetInt32();
            continue;
        }

        if(key=="wrapT"){
            value.wrapT = (glWrap)kv.Value.GetInt32();
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFImage> Deserialize_gltf_images(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFImage>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_images_LIST(x));
    }
	return value;
}
public static glTFImage Deserialize_gltf_images_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFImage();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="uri"){
            value.uri = kv.Value.GetString();
            continue;
        }

        if(key=="bufferView"){
            value.bufferView = kv.Value.GetInt32();
            continue;
        }

        if(key=="mimeType"){
            value.mimeType = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFMaterial> Deserialize_gltf_materials(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFMaterial>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_materials_LIST(x));
    }
	return value;
}
public static glTFMaterial Deserialize_gltf_materials_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterial();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="pbrMetallicRoughness"){
            value.pbrMetallicRoughness = Deserialize_gltf_materials__pbrMetallicRoughness(kv.Value);
            continue;
        }

        if(key=="normalTexture"){
            value.normalTexture = Deserialize_gltf_materials__normalTexture(kv.Value);
            continue;
        }

        if(key=="occlusionTexture"){
            value.occlusionTexture = Deserialize_gltf_materials__occlusionTexture(kv.Value);
            continue;
        }

        if(key=="emissiveTexture"){
            value.emissiveTexture = Deserialize_gltf_materials__emissiveTexture(kv.Value);
            continue;
        }

        if(key=="emissiveFactor"){
            value.emissiveFactor = Deserialize_gltf_materials__emissiveFactor(kv.Value);
            continue;
        }

        if(key=="alphaMode"){
            value.alphaMode = kv.Value.GetString();
            continue;
        }

        if(key=="alphaCutoff"){
            value.alphaCutoff = kv.Value.GetSingle();
            continue;
        }

        if(key=="doubleSided"){
            value.doubleSided = kv.Value.GetBoolean();
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_materials__extensions(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFPbrMetallicRoughness Deserialize_gltf_materials__pbrMetallicRoughness(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPbrMetallicRoughness();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="baseColorTexture"){
            value.baseColorTexture = Deserialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(kv.Value);
            continue;
        }

        if(key=="baseColorFactor"){
            value.baseColorFactor = Deserialize_gltf_materials__pbrMetallicRoughness_baseColorFactor(kv.Value);
            continue;
        }

        if(key=="metallicRoughnessTexture"){
            value.metallicRoughnessTexture = Deserialize_gltf_materials__pbrMetallicRoughness_metallicRoughnessTexture(kv.Value);
            continue;
        }

        if(key=="metallicFactor"){
            value.metallicFactor = kv.Value.GetSingle();
            continue;
        }

        if(key=="roughnessFactor"){
            value.roughnessFactor = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static glTFMaterialBaseColorTextureInfo Deserialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterialBaseColorTextureInfo();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.texCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_materials__pbrMetallicRoughness_baseColorFactor(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTFMaterialMetallicRoughnessTextureInfo Deserialize_gltf_materials__pbrMetallicRoughness_metallicRoughnessTexture(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterialMetallicRoughnessTextureInfo();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.texCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static glTFMaterialNormalTextureInfo Deserialize_gltf_materials__normalTexture(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterialNormalTextureInfo();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="scale"){
            value.scale = kv.Value.GetSingle();
            continue;
        }

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.texCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static glTFMaterialOcclusionTextureInfo Deserialize_gltf_materials__occlusionTexture(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterialOcclusionTextureInfo();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="strength"){
            value.strength = kv.Value.GetSingle();
            continue;
        }

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.texCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static glTFMaterialEmissiveTextureInfo Deserialize_gltf_materials__emissiveTexture(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterialEmissiveTextureInfo();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="texCoord"){
            value.texCoord = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_materials__emissiveFactor(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTFMaterial_extensions Deserialize_gltf_materials__extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMaterial_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="KHR_materials_unlit"){
            value.KHR_materials_unlit = Deserialize_gltf_materials__extensions_KHR_materials_unlit(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTF_KHR_materials_unlit Deserialize_gltf_materials__extensions_KHR_materials_unlit(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_KHR_materials_unlit();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static List<UniGLTF.glTFMesh> Deserialize_gltf_meshes(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFMesh>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_meshes_LIST(x));
    }
	return value;
}
public static glTFMesh Deserialize_gltf_meshes_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFMesh();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="primitives"){
            value.primitives = Deserialize_gltf_meshes__primitives(kv.Value);
            continue;
        }

        if(key=="weights"){
            value.weights = Deserialize_gltf_meshes__weights(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFPrimitives> Deserialize_gltf_meshes__primitives(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFPrimitives>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_meshes__primitives_LIST(x));
    }
	return value;
}
public static glTFPrimitives Deserialize_gltf_meshes__primitives_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPrimitives();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="mode"){
            value.mode = kv.Value.GetInt32();
            continue;
        }

        if(key=="indices"){
            value.indices = kv.Value.GetInt32();
            continue;
        }

        if(key=="attributes"){
            value.attributes = Deserialize_gltf_meshes__primitives__attributes(kv.Value);
            continue;
        }

        if(key=="material"){
            value.material = kv.Value.GetInt32();
            continue;
        }

        if(key=="targets"){
            value.targets = Deserialize_gltf_meshes__primitives__targets(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = Deserialize_gltf_meshes__primitives__extras(kv.Value);
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_meshes__primitives__extensions(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFAttributes Deserialize_gltf_meshes__primitives__attributes(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAttributes();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="POSITION"){
            value.POSITION = kv.Value.GetInt32();
            continue;
        }

        if(key=="NORMAL"){
            value.NORMAL = kv.Value.GetInt32();
            continue;
        }

        if(key=="TANGENT"){
            value.TANGENT = kv.Value.GetInt32();
            continue;
        }

        if(key=="TEXCOORD_0"){
            value.TEXCOORD_0 = kv.Value.GetInt32();
            continue;
        }

        if(key=="COLOR_0"){
            value.COLOR_0 = kv.Value.GetInt32();
            continue;
        }

        if(key=="JOINTS_0"){
            value.JOINTS_0 = kv.Value.GetInt32();
            continue;
        }

        if(key=="WEIGHTS_0"){
            value.WEIGHTS_0 = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.gltfMorphTarget> Deserialize_gltf_meshes__primitives__targets(ListTreeNode<JsonValue> parsed)
{
    var value = new List<gltfMorphTarget>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_meshes__primitives__targets_LIST(x));
    }
	return value;
}
public static gltfMorphTarget Deserialize_gltf_meshes__primitives__targets_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new gltfMorphTarget();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="POSITION"){
            value.POSITION = kv.Value.GetInt32();
            continue;
        }

        if(key=="NORMAL"){
            value.NORMAL = kv.Value.GetInt32();
            continue;
        }

        if(key=="TANGENT"){
            value.TANGENT = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static glTFPrimitives_extras Deserialize_gltf_meshes__primitives__extras(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPrimitives_extras();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="targetNames"){
            value.targetNames = Deserialize_gltf_meshes__primitives__extras_targetNames(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<System.String> Deserialize_gltf_meshes__primitives__extras_targetNames(ListTreeNode<JsonValue> parsed)
{
    var value = new List<String>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
}
public static glTFPrimitives_extensions Deserialize_gltf_meshes__primitives__extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPrimitives_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static Single[] Deserialize_gltf_meshes__weights(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<UniGLTF.glTFNode> Deserialize_gltf_nodes(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFNode>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_nodes_LIST(x));
    }
	return value;
}
public static glTFNode Deserialize_gltf_nodes_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFNode();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="children"){
            value.children = Deserialize_gltf_nodes__children(kv.Value);
            continue;
        }

        if(key=="matrix"){
            value.matrix = Deserialize_gltf_nodes__matrix(kv.Value);
            continue;
        }

        if(key=="translation"){
            value.translation = Deserialize_gltf_nodes__translation(kv.Value);
            continue;
        }

        if(key=="rotation"){
            value.rotation = Deserialize_gltf_nodes__rotation(kv.Value);
            continue;
        }

        if(key=="scale"){
            value.scale = Deserialize_gltf_nodes__scale(kv.Value);
            continue;
        }

        if(key=="mesh"){
            value.mesh = kv.Value.GetInt32();
            continue;
        }

        if(key=="skin"){
            value.skin = kv.Value.GetInt32();
            continue;
        }

        if(key=="weights"){
            value.weights = Deserialize_gltf_nodes__weights(kv.Value);
            continue;
        }

        if(key=="camera"){
            value.camera = kv.Value.GetInt32();
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_nodes__extensions(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = Deserialize_gltf_nodes__extras(kv.Value);
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_nodes__children(ListTreeNode<JsonValue> parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__matrix(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__translation(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__rotation(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__scale(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__weights(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTFNode_extensions Deserialize_gltf_nodes__extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFNode_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static glTFNode_extra Deserialize_gltf_nodes__extras(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFNode_extra();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static List<UniGLTF.glTFSkin> Deserialize_gltf_skins(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFSkin>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_skins_LIST(x));
    }
	return value;
}
public static glTFSkin Deserialize_gltf_skins_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFSkin();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="inverseBindMatrices"){
            value.inverseBindMatrices = kv.Value.GetInt32();
            continue;
        }

        if(key=="joints"){
            value.joints = Deserialize_gltf_skins__joints(kv.Value);
            continue;
        }

        if(key=="skeleton"){
            value.skeleton = kv.Value.GetInt32();
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_skins__joints(ListTreeNode<JsonValue> parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<UniGLTF.gltfScene> Deserialize_gltf_scenes(ListTreeNode<JsonValue> parsed)
{
    var value = new List<gltfScene>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_scenes_LIST(x));
    }
	return value;
}
public static gltfScene Deserialize_gltf_scenes_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new gltfScene();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="nodes"){
            value.nodes = Deserialize_gltf_scenes__nodes(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_scenes__nodes(ListTreeNode<JsonValue> parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<UniGLTF.glTFAnimation> Deserialize_gltf_animations(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFAnimation>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_animations_LIST(x));
    }
	return value;
}
public static glTFAnimation Deserialize_gltf_animations_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAnimation();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="channels"){
            value.channels = Deserialize_gltf_animations__channels(kv.Value);
            continue;
        }

        if(key=="samplers"){
            value.samplers = Deserialize_gltf_animations__samplers(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFAnimationChannel> Deserialize_gltf_animations__channels(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFAnimationChannel>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_animations__channels_LIST(x));
    }
	return value;
}
public static glTFAnimationChannel Deserialize_gltf_animations__channels_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAnimationChannel();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="sampler"){
            value.sampler = kv.Value.GetInt32();
            continue;
        }

        if(key=="target"){
            value.target = Deserialize_gltf_animations__channels__target(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFAnimationTarget Deserialize_gltf_animations__channels__target(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAnimationTarget();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.node = kv.Value.GetInt32();
            continue;
        }

        if(key=="path"){
            value.path = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFAnimationSampler> Deserialize_gltf_animations__samplers(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFAnimationSampler>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_animations__samplers_LIST(x));
    }
	return value;
}
public static glTFAnimationSampler Deserialize_gltf_animations__samplers_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFAnimationSampler();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="input"){
            value.input = kv.Value.GetInt32();
            continue;
        }

        if(key=="interpolation"){
            value.interpolation = kv.Value.GetString();
            continue;
        }

        if(key=="output"){
            value.output = kv.Value.GetInt32();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFCamera> Deserialize_gltf_cameras(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTFCamera>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_cameras_LIST(x));
    }
	return value;
}
public static glTFCamera Deserialize_gltf_cameras_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFCamera();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="orthographic"){
            value.orthographic = Deserialize_gltf_cameras__orthographic(kv.Value);
            continue;
        }

        if(key=="perspective"){
            value.perspective = Deserialize_gltf_cameras__perspective(kv.Value);
            continue;
        }

        if(key=="type"){
            value.type = (ProjectionType)kv.Value.GetInt32();
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_cameras__extensions(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = Deserialize_gltf_cameras__extras(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFOrthographic Deserialize_gltf_cameras__orthographic(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFOrthographic();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="xmag"){
            value.xmag = kv.Value.GetSingle();
            continue;
        }

        if(key=="ymag"){
            value.ymag = kv.Value.GetSingle();
            continue;
        }

        if(key=="zfar"){
            value.zfar = kv.Value.GetSingle();
            continue;
        }

        if(key=="znear"){
            value.znear = kv.Value.GetSingle();
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_cameras__orthographic_extensions(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = Deserialize_gltf_cameras__orthographic_extras(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFOrthographic_extensions Deserialize_gltf_cameras__orthographic_extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFOrthographic_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static glTFOrthographic_extras Deserialize_gltf_cameras__orthographic_extras(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFOrthographic_extras();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static glTFPerspective Deserialize_gltf_cameras__perspective(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPerspective();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="aspectRatio"){
            value.aspectRatio = kv.Value.GetSingle();
            continue;
        }

        if(key=="yfov"){
            value.yfov = kv.Value.GetSingle();
            continue;
        }

        if(key=="zfar"){
            value.zfar = kv.Value.GetSingle();
            continue;
        }

        if(key=="znear"){
            value.znear = kv.Value.GetSingle();
            continue;
        }

        if(key=="extensions"){
            value.extensions = Deserialize_gltf_cameras__perspective_extensions(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = Deserialize_gltf_cameras__perspective_extras(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFPerspective_extensions Deserialize_gltf_cameras__perspective_extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPerspective_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static glTFPerspective_extras Deserialize_gltf_cameras__perspective_extras(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFPerspective_extras();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static glTFCamera_extensions Deserialize_gltf_cameras__extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFCamera_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static glTFCamera_extras Deserialize_gltf_cameras__extras(ListTreeNode<JsonValue> parsed)
{
    var value = new glTFCamera_extras();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

public static List<System.String> Deserialize_gltf_extensionsUsed(ListTreeNode<JsonValue> parsed)
{
    var value = new List<String>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
}
public static List<System.String> Deserialize_gltf_extensionsRequired(ListTreeNode<JsonValue> parsed)
{
    var value = new List<String>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
}
public static glTF_extensions Deserialize_gltf_extensions(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="VRM"){
            value.VRM = Deserialize_gltf_extensions_VRM(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTF_VRM_extensions Deserialize_gltf_extensions_VRM(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_extensions();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="exporterVersion"){
            value.exporterVersion = kv.Value.GetString();
            continue;
        }

        if(key=="specVersion"){
            value.specVersion = kv.Value.GetString();
            continue;
        }

        if(key=="meta"){
            value.meta = Deserialize_gltf_extensions_VRM_meta(kv.Value);
            continue;
        }

        if(key=="humanoid"){
            value.humanoid = Deserialize_gltf_extensions_VRM_humanoid(kv.Value);
            continue;
        }

        if(key=="firstPerson"){
            value.firstPerson = Deserialize_gltf_extensions_VRM_firstPerson(kv.Value);
            continue;
        }

        if(key=="blendShapeMaster"){
            value.blendShapeMaster = Deserialize_gltf_extensions_VRM_blendShapeMaster(kv.Value);
            continue;
        }

        if(key=="secondaryAnimation"){
            value.secondaryAnimation = Deserialize_gltf_extensions_VRM_secondaryAnimation(kv.Value);
            continue;
        }

        if(key=="materialProperties"){
            value.materialProperties = Deserialize_gltf_extensions_VRM_materialProperties(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTF_VRM_Meta Deserialize_gltf_extensions_VRM_meta(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_Meta();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="title"){
            value.title = kv.Value.GetString();
            continue;
        }

        if(key=="version"){
            value.version = kv.Value.GetString();
            continue;
        }

        if(key=="author"){
            value.author = kv.Value.GetString();
            continue;
        }

        if(key=="contactInformation"){
            value.contactInformation = kv.Value.GetString();
            continue;
        }

        if(key=="reference"){
            value.reference = kv.Value.GetString();
            continue;
        }

        if(key=="texture"){
            value.texture = kv.Value.GetInt32();
            continue;
        }

        if(key=="allowedUserName"){
            value.allowedUserName = kv.Value.GetString();
            continue;
        }

        if(key=="violentUssageName"){
            value.violentUssageName = kv.Value.GetString();
            continue;
        }

        if(key=="sexualUssageName"){
            value.sexualUssageName = kv.Value.GetString();
            continue;
        }

        if(key=="commercialUssageName"){
            value.commercialUssageName = kv.Value.GetString();
            continue;
        }

        if(key=="otherPermissionUrl"){
            value.otherPermissionUrl = kv.Value.GetString();
            continue;
        }

        if(key=="licenseName"){
            value.licenseName = kv.Value.GetString();
            continue;
        }

        if(key=="otherLicenseUrl"){
            value.otherLicenseUrl = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static glTF_VRM_Humanoid Deserialize_gltf_extensions_VRM_humanoid(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_Humanoid();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="humanBones"){
            value.humanBones = Deserialize_gltf_extensions_VRM_humanoid_humanBones(kv.Value);
            continue;
        }

        if(key=="armStretch"){
            value.armStretch = kv.Value.GetSingle();
            continue;
        }

        if(key=="legStretch"){
            value.legStretch = kv.Value.GetSingle();
            continue;
        }

        if(key=="upperArmTwist"){
            value.upperArmTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="lowerArmTwist"){
            value.lowerArmTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="upperLegTwist"){
            value.upperLegTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="lowerLegTwist"){
            value.lowerLegTwist = kv.Value.GetSingle();
            continue;
        }

        if(key=="feetSpacing"){
            value.feetSpacing = kv.Value.GetSingle();
            continue;
        }

        if(key=="hasTranslationDoF"){
            value.hasTranslationDoF = kv.Value.GetBoolean();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_HumanoidBone> Deserialize_gltf_extensions_VRM_humanoid_humanBones(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_HumanoidBone>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_humanoid_humanBones_LIST(x));
    }
	return value;
}
public static glTF_VRM_HumanoidBone Deserialize_gltf_extensions_VRM_humanoid_humanBones_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_HumanoidBone();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="bone"){
            value.bone = kv.Value.GetString();
            continue;
        }

        if(key=="node"){
            value.node = kv.Value.GetInt32();
            continue;
        }

        if(key=="useDefaultValues"){
            value.useDefaultValues = kv.Value.GetBoolean();
            continue;
        }

        if(key=="min"){
            value.min = Deserialize_gltf_extensions_VRM_humanoid_humanBones__min(kv.Value);
            continue;
        }

        if(key=="max"){
            value.max = Deserialize_gltf_extensions_VRM_humanoid_humanBones__max(kv.Value);
            continue;
        }

        if(key=="center"){
            value.center = Deserialize_gltf_extensions_VRM_humanoid_humanBones__center(kv.Value);
            continue;
        }

        if(key=="axisLength"){
            value.axisLength = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_gltf_extensions_VRM_humanoid_humanBones__min(ListTreeNode<JsonValue> parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_gltf_extensions_VRM_humanoid_humanBones__max(ListTreeNode<JsonValue> parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_gltf_extensions_VRM_humanoid_humanBones__center(ListTreeNode<JsonValue> parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static glTF_VRM_Firstperson Deserialize_gltf_extensions_VRM_firstPerson(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_Firstperson();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="firstPersonBone"){
            value.firstPersonBone = kv.Value.GetInt32();
            continue;
        }

        if(key=="firstPersonBoneOffset"){
            value.firstPersonBoneOffset = Deserialize_gltf_extensions_VRM_firstPerson_firstPersonBoneOffset(kv.Value);
            continue;
        }

        if(key=="meshAnnotations"){
            value.meshAnnotations = Deserialize_gltf_extensions_VRM_firstPerson_meshAnnotations(kv.Value);
            continue;
        }

        if(key=="lookAtTypeName"){
            value.lookAtTypeName = kv.Value.GetString();
            continue;
        }

        if(key=="lookAtHorizontalInner"){
            value.lookAtHorizontalInner = Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalInner(kv.Value);
            continue;
        }

        if(key=="lookAtHorizontalOuter"){
            value.lookAtHorizontalOuter = Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalOuter(kv.Value);
            continue;
        }

        if(key=="lookAtVerticalDown"){
            value.lookAtVerticalDown = Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalDown(kv.Value);
            continue;
        }

        if(key=="lookAtVerticalUp"){
            value.lookAtVerticalUp = Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalUp(kv.Value);
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_gltf_extensions_VRM_firstPerson_firstPersonBoneOffset(ListTreeNode<JsonValue> parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_MeshAnnotation> Deserialize_gltf_extensions_VRM_firstPerson_meshAnnotations(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_MeshAnnotation>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_firstPerson_meshAnnotations_LIST(x));
    }
	return value;
}
public static glTF_VRM_MeshAnnotation Deserialize_gltf_extensions_VRM_firstPerson_meshAnnotations_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_MeshAnnotation();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="mesh"){
            value.mesh = kv.Value.GetInt32();
            continue;
        }

        if(key=="firstPersonFlag"){
            value.firstPersonFlag = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static glTF_VRM_DegreeMap Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalInner(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalInner_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalInner_curve(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_DegreeMap Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalOuter(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalOuter_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_extensions_VRM_firstPerson_lookAtHorizontalOuter_curve(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_DegreeMap Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalDown(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalDown_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalDown_curve(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_DegreeMap Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalUp(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_DegreeMap();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="curve"){
            value.curve = Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalUp_curve(kv.Value);
            continue;
        }

        if(key=="xRange"){
            value.xRange = kv.Value.GetSingle();
            continue;
        }

        if(key=="yRange"){
            value.yRange = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_extensions_VRM_firstPerson_lookAtVerticalUp_curve(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_BlendShapeMaster Deserialize_gltf_extensions_VRM_blendShapeMaster(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_BlendShapeMaster();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="blendShapeGroups"){
            value.blendShapeGroups = Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_BlendShapeGroup> Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_BlendShapeGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups_LIST(x));
    }
	return value;
}
public static glTF_VRM_BlendShapeGroup Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_BlendShapeGroup();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="presetName"){
            value.presetName = kv.Value.GetString();
            continue;
        }

        if(key=="binds"){
            value.binds = Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__binds(kv.Value);
            continue;
        }

        if(key=="materialValues"){
            value.materialValues = Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__materialValues(kv.Value);
            continue;
        }

        if(key=="isBinary"){
            value.isBinary = kv.Value.GetBoolean();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_BlendShapeBind> Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__binds(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_BlendShapeBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__binds_LIST(x));
    }
	return value;
}
public static glTF_VRM_BlendShapeBind Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__binds_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_BlendShapeBind();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="mesh"){
            value.mesh = kv.Value.GetInt32();
            continue;
        }

        if(key=="index"){
            value.index = kv.Value.GetInt32();
            continue;
        }

        if(key=="weight"){
            value.weight = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_MaterialValueBind> Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__materialValues(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_MaterialValueBind>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__materialValues_LIST(x));
    }
	return value;
}
public static glTF_VRM_MaterialValueBind Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__materialValues_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_MaterialValueBind();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="materialName"){
            value.materialName = kv.Value.GetString();
            continue;
        }

        if(key=="propertyName"){
            value.propertyName = kv.Value.GetString();
            continue;
        }

        if(key=="targetValue"){
            value.targetValue = Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__materialValues__targetValue(kv.Value);
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_extensions_VRM_blendShapeMaster_blendShapeGroups__materialValues__targetValue(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTF_VRM_SecondaryAnimation Deserialize_gltf_extensions_VRM_secondaryAnimation(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_SecondaryAnimation();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="boneGroups"){
            value.boneGroups = Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups(kv.Value);
            continue;
        }

        if(key=="colliderGroups"){
            value.colliderGroups = Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_SecondaryAnimationGroup> Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_SecondaryAnimationGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups_LIST(x));
    }
	return value;
}
public static glTF_VRM_SecondaryAnimationGroup Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_SecondaryAnimationGroup();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="comment"){
            value.comment = kv.Value.GetString();
            continue;
        }

        if(key=="stiffiness"){
            value.stiffiness = kv.Value.GetSingle();
            continue;
        }

        if(key=="gravityPower"){
            value.gravityPower = kv.Value.GetSingle();
            continue;
        }

        if(key=="gravityDir"){
            value.gravityDir = Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups__gravityDir(kv.Value);
            continue;
        }

        if(key=="dragForce"){
            value.dragForce = kv.Value.GetSingle();
            continue;
        }

        if(key=="center"){
            value.center = kv.Value.GetInt32();
            continue;
        }

        if(key=="hitRadius"){
            value.hitRadius = kv.Value.GetSingle();
            continue;
        }

        if(key=="bones"){
            value.bones = Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups__bones(kv.Value);
            continue;
        }

        if(key=="colliderGroups"){
            value.colliderGroups = Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups__colliderGroups(kv.Value);
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups__gravityDir(ListTreeNode<JsonValue> parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups__bones(ListTreeNode<JsonValue> parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static Int32[] Deserialize_gltf_extensions_VRM_secondaryAnimation_boneGroups__colliderGroups(ListTreeNode<JsonValue> parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<VRM.glTF_VRM_SecondaryAnimationColliderGroup> Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_SecondaryAnimationColliderGroup>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups_LIST(x));
    }
	return value;
}
public static glTF_VRM_SecondaryAnimationColliderGroup Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_SecondaryAnimationColliderGroup();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="node"){
            value.node = kv.Value.GetInt32();
            continue;
        }

        if(key=="colliders"){
            value.colliders = Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups__colliders(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_SecondaryAnimationCollider> Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups__colliders(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_SecondaryAnimationCollider>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups__colliders_LIST(x));
    }
	return value;
}
public static glTF_VRM_SecondaryAnimationCollider Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups__colliders_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_SecondaryAnimationCollider();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="offset"){
            value.offset = Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups__colliders__offset(kv.Value);
            continue;
        }

        if(key=="radius"){
            value.radius = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static Vector3 Deserialize_gltf_extensions_VRM_secondaryAnimation_colliderGroups__colliders__offset(ListTreeNode<JsonValue> parsed)
{
    var value = new Vector3();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="x"){
            value.x = kv.Value.GetSingle();
            continue;
        }

        if(key=="y"){
            value.y = kv.Value.GetSingle();
            continue;
        }

        if(key=="z"){
            value.z = kv.Value.GetSingle();
            continue;
        }

    }
    return value;
}

public static List<VRM.glTF_VRM_Material> Deserialize_gltf_extensions_VRM_materialProperties(ListTreeNode<JsonValue> parsed)
{
    var value = new List<glTF_VRM_Material>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_extensions_VRM_materialProperties_LIST(x));
    }
	return value;
}
public static glTF_VRM_Material Deserialize_gltf_extensions_VRM_materialProperties_LIST(ListTreeNode<JsonValue> parsed)
{
    var value = new glTF_VRM_Material();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="shader"){
            value.shader = kv.Value.GetString();
            continue;
        }

        if(key=="renderQueue"){
            value.renderQueue = kv.Value.GetInt32();
            continue;
        }

        if(key=="floatProperties"){
            value.floatProperties = Deserialize_gltf_extensions_VRM_materialProperties__floatProperties(kv.Value);
            continue;
        }

        if(key=="vectorProperties"){
            value.vectorProperties = Deserialize_gltf_extensions_VRM_materialProperties__vectorProperties(kv.Value);
            continue;
        }

        if(key=="textureProperties"){
            value.textureProperties = Deserialize_gltf_extensions_VRM_materialProperties__textureProperties(kv.Value);
            continue;
        }

        if(key=="keywordMap"){
            value.keywordMap = Deserialize_gltf_extensions_VRM_materialProperties__keywordMap(kv.Value);
            continue;
        }

        if(key=="tagMap"){
            value.tagMap = Deserialize_gltf_extensions_VRM_materialProperties__tagMap(kv.Value);
            continue;
        }

    }
    return value;
}

 
public static Dictionary<String, Single> Deserialize_gltf_extensions_VRM_materialProperties__floatProperties(ListTreeNode<JsonValue> parsed)
{
    var value = new Dictionary<string, Single>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetSingle());
    }
	return value;
}

 
public static Dictionary<String, Single[]> Deserialize_gltf_extensions_VRM_materialProperties__vectorProperties(ListTreeNode<JsonValue> parsed)
{
    var value = new Dictionary<string, Single[]>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), Deserialize_gltf_extensions_VRM_materialProperties__vectorProperties_DICT(kv.Value));
    }
	return value;
}

public static Single[] Deserialize_gltf_extensions_VRM_materialProperties__vectorProperties_DICT(ListTreeNode<JsonValue> parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

 
public static Dictionary<String, Int32> Deserialize_gltf_extensions_VRM_materialProperties__textureProperties(ListTreeNode<JsonValue> parsed)
{
    var value = new Dictionary<string, Int32>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetInt32());
    }
	return value;
}

 
public static Dictionary<String, Boolean> Deserialize_gltf_extensions_VRM_materialProperties__keywordMap(ListTreeNode<JsonValue> parsed)
{
    var value = new Dictionary<string, Boolean>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetBoolean());
    }
	return value;
}

 
public static Dictionary<String, String> Deserialize_gltf_extensions_VRM_materialProperties__tagMap(ListTreeNode<JsonValue> parsed)
{
    var value = new Dictionary<string, String>();
    foreach(var kv in parsed.ObjectItems())
    {
        value.Add(kv.Key.GetString(), kv.Value.GetString());
    }
	return value;
}

public static gltf_extras Deserialize_gltf_extras(ListTreeNode<JsonValue> parsed)
{
    var value = new gltf_extras();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

    }
    return value;
}

} // GltfDeserializer
} // UniGLTF 
