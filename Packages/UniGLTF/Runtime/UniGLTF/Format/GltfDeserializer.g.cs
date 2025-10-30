using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF {

public static class GltfDeserializer
{


public static glTF Deserialize(JsonNode parsed)
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
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFAssets Deserialize_gltf_asset(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFBuffer> Deserialize_gltf_buffers(JsonNode parsed)
{
    var value = new List<glTFBuffer>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_buffers_ITEM(x));
    }
	return value;
}
public static glTFBuffer Deserialize_gltf_buffers_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFBufferView> Deserialize_gltf_bufferViews(JsonNode parsed)
{
    var value = new List<glTFBufferView>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_bufferViews_ITEM(x));
    }
	return value;
}
public static glTFBufferView Deserialize_gltf_bufferViews_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFAccessor> Deserialize_gltf_accessors(JsonNode parsed)
{
    var value = new List<glTFAccessor>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_accessors_ITEM(x));
    }
	return value;
}
public static glTFAccessor Deserialize_gltf_accessors_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_accessors__max(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_accessors__min(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTFSparse Deserialize_gltf_accessors__sparse(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFSparseIndices Deserialize_gltf_accessors__sparse_indices(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFSparseValues Deserialize_gltf_accessors__sparse_values(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFTexture> Deserialize_gltf_textures(JsonNode parsed)
{
    var value = new List<glTFTexture>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_textures_ITEM(x));
    }
	return value;
}
public static glTFTexture Deserialize_gltf_textures_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFTextureSampler> Deserialize_gltf_samplers(JsonNode parsed)
{
    var value = new List<glTFTextureSampler>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_samplers_ITEM(x));
    }
	return value;
}
public static glTFTextureSampler Deserialize_gltf_samplers_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFImage> Deserialize_gltf_images(JsonNode parsed)
{
    var value = new List<glTFImage>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_images_ITEM(x));
    }
	return value;
}
public static glTFImage Deserialize_gltf_images_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFMaterial> Deserialize_gltf_materials(JsonNode parsed)
{
    var value = new List<glTFMaterial>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_materials_ITEM(x));
    }
	return value;
}
public static glTFMaterial Deserialize_gltf_materials_ITEM(JsonNode parsed)
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
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFPbrMetallicRoughness Deserialize_gltf_materials__pbrMetallicRoughness(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFMaterialBaseColorTextureInfo Deserialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_materials__pbrMetallicRoughness_baseColorFactor(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static glTFMaterialMetallicRoughnessTextureInfo Deserialize_gltf_materials__pbrMetallicRoughness_metallicRoughnessTexture(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFMaterialNormalTextureInfo Deserialize_gltf_materials__normalTexture(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFMaterialOcclusionTextureInfo Deserialize_gltf_materials__occlusionTexture(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFMaterialEmissiveTextureInfo Deserialize_gltf_materials__emissiveTexture(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static Single[] Deserialize_gltf_materials__emissiveFactor(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<UniGLTF.glTFMesh> Deserialize_gltf_meshes(JsonNode parsed)
{
    var value = new List<glTFMesh>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_meshes_ITEM(x));
    }
	return value;
}
public static glTFMesh Deserialize_gltf_meshes_ITEM(JsonNode parsed)
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

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFPrimitives> Deserialize_gltf_meshes__primitives(JsonNode parsed)
{
    var value = new List<glTFPrimitives>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_meshes__primitives_ITEM(x));
    }
	return value;
}
public static glTFPrimitives Deserialize_gltf_meshes__primitives_ITEM(JsonNode parsed)
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
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFAttributes Deserialize_gltf_meshes__primitives__attributes(JsonNode parsed)
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

        if(key=="TEXCOORD_1"){
            value.TEXCOORD_1 = kv.Value.GetInt32();
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

public static List<UniGLTF.gltfMorphTarget> Deserialize_gltf_meshes__primitives__targets(JsonNode parsed)
{
    var value = new List<gltfMorphTarget>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_meshes__primitives__targets_ITEM(x));
    }
	return value;
}
public static gltfMorphTarget Deserialize_gltf_meshes__primitives__targets_ITEM(JsonNode parsed)
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

public static Single[] Deserialize_gltf_meshes__weights(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<UniGLTF.glTFNode> Deserialize_gltf_nodes(JsonNode parsed)
{
    var value = new List<glTFNode>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_nodes_ITEM(x));
    }
	return value;
}
public static glTFNode Deserialize_gltf_nodes_ITEM(JsonNode parsed)
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
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_nodes__children(JsonNode parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__matrix(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__translation(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__rotation(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__scale(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static Single[] Deserialize_gltf_nodes__weights(JsonNode parsed)
{
    var value = new Single[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetSingle();
    }
	return value;
} 

public static List<UniGLTF.glTFSkin> Deserialize_gltf_skins(JsonNode parsed)
{
    var value = new List<glTFSkin>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_skins_ITEM(x));
    }
	return value;
}
public static glTFSkin Deserialize_gltf_skins_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_skins__joints(JsonNode parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<UniGLTF.gltfScene> Deserialize_gltf_scenes(JsonNode parsed)
{
    var value = new List<gltfScene>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_scenes_ITEM(x));
    }
	return value;
}
public static gltfScene Deserialize_gltf_scenes_ITEM(JsonNode parsed)
{
    var value = new gltfScene();

    foreach(var kv in parsed.ObjectItems())
    {
        var key = kv.Key.GetString();

        if(key=="nodes"){
            value.nodes = Deserialize_gltf_scenes__nodes(kv.Value);
            continue;
        }

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

    }
    return value;
}

public static Int32[] Deserialize_gltf_scenes__nodes(JsonNode parsed)
{
    var value = new Int32[parsed.GetArrayCount()];
    int i=0;
    foreach(var x in parsed.ArrayItems())
    {
        value[i++] = x.GetInt32();
    }
	return value;
} 

public static List<UniGLTF.glTFAnimation> Deserialize_gltf_animations(JsonNode parsed)
{
    var value = new List<glTFAnimation>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_animations_ITEM(x));
    }
	return value;
}
public static glTFAnimation Deserialize_gltf_animations_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFAnimationChannel> Deserialize_gltf_animations__channels(JsonNode parsed)
{
    var value = new List<glTFAnimationChannel>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_animations__channels_ITEM(x));
    }
	return value;
}
public static glTFAnimationChannel Deserialize_gltf_animations__channels_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFAnimationTarget Deserialize_gltf_animations__channels__target(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFAnimationSampler> Deserialize_gltf_animations__samplers(JsonNode parsed)
{
    var value = new List<glTFAnimationSampler>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_animations__samplers_ITEM(x));
    }
	return value;
}
public static glTFAnimationSampler Deserialize_gltf_animations__samplers_ITEM(JsonNode parsed)
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

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<UniGLTF.glTFCamera> Deserialize_gltf_cameras(JsonNode parsed)
{
    var value = new List<glTFCamera>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(Deserialize_gltf_cameras_ITEM(x));
    }
	return value;
}
public static glTFCamera Deserialize_gltf_cameras_ITEM(JsonNode parsed)
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
            value.type = (ProjectionType)Enum.Parse(typeof(ProjectionType), kv.Value.GetString(), true);
            continue;
        }

        if(key=="name"){
            value.name = kv.Value.GetString();
            continue;
        }

        if(key=="extensions"){
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFOrthographic Deserialize_gltf_cameras__orthographic(JsonNode parsed)
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
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static glTFPerspective Deserialize_gltf_cameras__perspective(JsonNode parsed)
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
            value.extensions = new glTFExtensionImport(kv.Value);
            continue;
        }

        if(key=="extras"){
            value.extras = new glTFExtensionImport(kv.Value);
            continue;
        }

    }
    return value;
}

public static List<System.String> Deserialize_gltf_extensionsUsed(JsonNode parsed)
{
    var value = new List<String>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
}
public static List<System.String> Deserialize_gltf_extensionsRequired(JsonNode parsed)
{
    var value = new List<String>();
    foreach(var x in parsed.ArrayItems())
    {
        value.Add(x.GetString());
    }
	return value;
}
} // GltfDeserializer
} // UniGLTF 
