using System;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF {

    static public class GltfSerializer
    {


public static void Serialize(JsonFormatter f, glTF value)
{
    f.BeginMap();

    f.Key("asset");
    Serialize_gltf_asset(f, value.asset);

    f.Key("buffers");
    Serialize_gltf_buffers(f, value.buffers);

    f.Key("bufferViews");
    Serialize_gltf_bufferViews(f, value.bufferViews);

    f.Key("accessors");
    Serialize_gltf_accessors(f, value.accessors);

    f.Key("textures");
    Serialize_gltf_textures(f, value.textures);

    f.Key("samplers");
    Serialize_gltf_samplers(f, value.samplers);

    f.Key("images");
    Serialize_gltf_images(f, value.images);

    f.Key("materials");
    Serialize_gltf_materials(f, value.materials);

    f.Key("meshes");
    Serialize_gltf_meshes(f, value.meshes);

    f.Key("nodes");
    Serialize_gltf_nodes(f, value.nodes);

    f.Key("skins");
    Serialize_gltf_skins(f, value.skins);

    f.Key("scene");
    f.Value(value.scene);

    f.Key("scenes");
    Serialize_gltf_scenes(f, value.scenes);

    f.Key("animations");
    Serialize_gltf_animations(f, value.animations);

    f.Key("cameras");
    Serialize_gltf_cameras(f, value.cameras);

    f.Key("extensionsUsed");
    Serialize_gltf_extensionsUsed(f, value.extensionsUsed);

    f.Key("extensionsRequired");
    Serialize_gltf_extensionsRequired(f, value.extensionsRequired);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_asset(JsonFormatter f, glTFAssets value)
{
    f.BeginMap();

    f.Key("generator");
    f.Value(value.generator);

    f.Key("version");
    f.Value(value.version);

    f.Key("copyright");
    f.Value(value.copyright);

    f.Key("minVersion");
    f.Value(value.minVersion);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_buffers(JsonFormatter f, List<glTFBuffer> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_buffers_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_buffers_LIST(JsonFormatter f, glTFBuffer value)
{
    f.BeginMap();

    f.Key("uri");
    f.Value(value.uri);

    f.Key("byteLength");
    f.Value(value.byteLength);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("name");
    f.Value(value.name);


    f.EndMap();
}

public static void Serialize_gltf_bufferViews(JsonFormatter f, List<glTFBufferView> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_bufferViews_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_bufferViews_LIST(JsonFormatter f, glTFBufferView value)
{
    f.BeginMap();

    f.Key("buffer");
    f.Value(value.buffer);

    f.Key("byteOffset");
    f.Value(value.byteOffset);

    f.Key("byteLength");
    f.Value(value.byteLength);

    f.Key("byteStride");
    f.Value(value.byteStride);

    f.Key("target");
    f.Value((int)value.target);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("name");
    f.Value(value.name);


    f.EndMap();
}

public static void Serialize_gltf_accessors(JsonFormatter f, List<glTFAccessor> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_accessors_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_accessors_LIST(JsonFormatter f, glTFAccessor value)
{
    f.BeginMap();

    f.Key("bufferView");
    f.Value(value.bufferView);

    f.Key("byteOffset");
    f.Value(value.byteOffset);

    f.Key("type");
    f.Value(value.type);

    f.Key("componentType");
    f.Value((int)value.componentType);

    f.Key("count");
    f.Value(value.count);

    f.Key("max");
    Serialize_gltf_accessors__max(f, value.max);

    f.Key("min");
    Serialize_gltf_accessors__min(f, value.min);

    f.Key("normalized");
    f.Value(value.normalized);

    f.Key("sparse");
    Serialize_gltf_accessors__sparse(f, value.sparse);

    f.Key("name");
    f.Value(value.name);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_accessors__max(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_accessors__min(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_accessors__sparse(JsonFormatter f, glTFSparse value)
{
    f.BeginMap();

    f.Key("count");
    f.Value(value.count);

    f.Key("indices");
    Serialize_gltf_accessors__sparse_indices(f, value.indices);

    f.Key("values");
    Serialize_gltf_accessors__sparse_values(f, value.values);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_accessors__sparse_indices(JsonFormatter f, glTFSparseIndices value)
{
    f.BeginMap();

    f.Key("bufferView");
    f.Value(value.bufferView);

    f.Key("byteOffset");
    f.Value(value.byteOffset);

    f.Key("componentType");
    f.Value((int)value.componentType);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_accessors__sparse_values(JsonFormatter f, glTFSparseValues value)
{
    f.BeginMap();

    f.Key("bufferView");
    f.Value(value.bufferView);

    f.Key("byteOffset");
    f.Value(value.byteOffset);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_textures(JsonFormatter f, List<glTFTexture> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_textures_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_textures_LIST(JsonFormatter f, glTFTexture value)
{
    f.BeginMap();

    f.Key("sampler");
    f.Value(value.sampler);

    f.Key("source");
    f.Value(value.source);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("name");
    f.Value(value.name);


    f.EndMap();
}

public static void Serialize_gltf_samplers(JsonFormatter f, List<glTFTextureSampler> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_samplers_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_samplers_LIST(JsonFormatter f, glTFTextureSampler value)
{
    f.BeginMap();

    f.Key("magFilter");
    f.Value((int)value.magFilter);

    f.Key("minFilter");
    f.Value((int)value.minFilter);

    f.Key("wrapS");
    f.Value((int)value.wrapS);

    f.Key("wrapT");
    f.Value((int)value.wrapT);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("name");
    f.Value(value.name);


    f.EndMap();
}

public static void Serialize_gltf_images(JsonFormatter f, List<glTFImage> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_images_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_images_LIST(JsonFormatter f, glTFImage value)
{
    f.BeginMap();

    f.Key("name");
    f.Value(value.name);

    f.Key("uri");
    f.Value(value.uri);

    f.Key("bufferView");
    f.Value(value.bufferView);

    f.Key("mimeType");
    f.Value(value.mimeType);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials(JsonFormatter f, List<glTFMaterial> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_materials_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_materials_LIST(JsonFormatter f, glTFMaterial value)
{
    f.BeginMap();

    f.Key("name");
    f.Value(value.name);

    f.Key("pbrMetallicRoughness");
    Serialize_gltf_materials__pbrMetallicRoughness(f, value.pbrMetallicRoughness);

    f.Key("normalTexture");
    Serialize_gltf_materials__normalTexture(f, value.normalTexture);

    f.Key("occlusionTexture");
    Serialize_gltf_materials__occlusionTexture(f, value.occlusionTexture);

    f.Key("emissiveTexture");
    Serialize_gltf_materials__emissiveTexture(f, value.emissiveTexture);

    f.Key("emissiveFactor");
    Serialize_gltf_materials__emissiveFactor(f, value.emissiveFactor);

    f.Key("alphaMode");
    f.Value(value.alphaMode);

    f.Key("alphaCutoff");
    f.Value(value.alphaCutoff);

    f.Key("doubleSided");
    f.Value(value.doubleSided);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__pbrMetallicRoughness(JsonFormatter f, glTFPbrMetallicRoughness value)
{
    f.BeginMap();

    f.Key("baseColorTexture");
    Serialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(f, value.baseColorTexture);

    f.Key("baseColorFactor");
    Serialize_gltf_materials__pbrMetallicRoughness_baseColorFactor(f, value.baseColorFactor);

    f.Key("metallicRoughnessTexture");
    Serialize_gltf_materials__pbrMetallicRoughness_metallicRoughnessTexture(f, value.metallicRoughnessTexture);

    f.Key("metallicFactor");
    f.Value(value.metallicFactor);

    f.Key("roughnessFactor");
    f.Value(value.roughnessFactor);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(JsonFormatter f, glTFMaterialBaseColorTextureInfo value)
{
    f.BeginMap();

    f.Key("index");
    f.Value(value.index);

    f.Key("texCoord");
    f.Value(value.texCoord);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__pbrMetallicRoughness_baseColorFactor(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_materials__pbrMetallicRoughness_metallicRoughnessTexture(JsonFormatter f, glTFMaterialMetallicRoughnessTextureInfo value)
{
    f.BeginMap();

    f.Key("index");
    f.Value(value.index);

    f.Key("texCoord");
    f.Value(value.texCoord);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__normalTexture(JsonFormatter f, glTFMaterialNormalTextureInfo value)
{
    f.BeginMap();

    f.Key("scale");
    f.Value(value.scale);

    f.Key("index");
    f.Value(value.index);

    f.Key("texCoord");
    f.Value(value.texCoord);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__occlusionTexture(JsonFormatter f, glTFMaterialOcclusionTextureInfo value)
{
    f.BeginMap();

    f.Key("strength");
    f.Value(value.strength);

    f.Key("index");
    f.Value(value.index);

    f.Key("texCoord");
    f.Value(value.texCoord);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__emissiveTexture(JsonFormatter f, glTFMaterialEmissiveTextureInfo value)
{
    f.BeginMap();

    f.Key("index");
    f.Value(value.index);

    f.Key("texCoord");
    f.Value(value.texCoord);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_materials__emissiveFactor(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes(JsonFormatter f, List<glTFMesh> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_meshes_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes_LIST(JsonFormatter f, glTFMesh value)
{
    f.BeginMap();

    f.Key("name");
    f.Value(value.name);

    f.Key("primitives");
    Serialize_gltf_meshes__primitives(f, value.primitives);

    f.Key("weights");
    Serialize_gltf_meshes__weights(f, value.weights);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("extensions");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_meshes__primitives(JsonFormatter f, List<glTFPrimitives> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_meshes__primitives_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes__primitives_LIST(JsonFormatter f, glTFPrimitives value)
{
    f.BeginMap();

    f.Key("mode");
    f.Value(value.mode);

    f.Key("indices");
    f.Value(value.indices);

    f.Key("attributes");
    Serialize_gltf_meshes__primitives__attributes(f, value.attributes);

    f.Key("material");
    f.Value(value.material);

    f.Key("targets");
    Serialize_gltf_meshes__primitives__targets(f, value.targets);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("extensions");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_meshes__primitives__attributes(JsonFormatter f, glTFAttributes value)
{
    f.BeginMap();

    f.Key("POSITION");
    f.Value(value.POSITION);

    f.Key("NORMAL");
    f.Value(value.NORMAL);

    f.Key("TANGENT");
    f.Value(value.TANGENT);

    f.Key("TEXCOORD_0");
    f.Value(value.TEXCOORD_0);

    f.Key("TEXCOORD_1");
    f.Value(value.TEXCOORD_1);

    f.Key("COLOR_0");
    f.Value(value.COLOR_0);

    f.Key("JOINTS_0");
    f.Value(value.JOINTS_0);

    f.Key("WEIGHTS_0");
    f.Value(value.WEIGHTS_0);


    f.EndMap();
}

public static void Serialize_gltf_meshes__primitives__targets(JsonFormatter f, List<gltfMorphTarget> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_meshes__primitives__targets_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes__primitives__targets_LIST(JsonFormatter f, gltfMorphTarget value)
{
    f.BeginMap();

    f.Key("POSITION");
    f.Value(value.POSITION);

    f.Key("NORMAL");
    f.Value(value.NORMAL);

    f.Key("TANGENT");
    f.Value(value.TANGENT);


    f.EndMap();
}

public static void Serialize_gltf_meshes__weights(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes(JsonFormatter f, List<glTFNode> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_nodes_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes_LIST(JsonFormatter f, glTFNode value)
{
    f.BeginMap();

    f.Key("name");
    f.Value(value.name);

    f.Key("children");
    Serialize_gltf_nodes__children(f, value.children);

    f.Key("matrix");
    Serialize_gltf_nodes__matrix(f, value.matrix);

    f.Key("translation");
    Serialize_gltf_nodes__translation(f, value.translation);

    f.Key("rotation");
    Serialize_gltf_nodes__rotation(f, value.rotation);

    f.Key("scale");
    Serialize_gltf_nodes__scale(f, value.scale);

    f.Key("mesh");
    f.Value(value.mesh);

    f.Key("skin");
    f.Value(value.skin);

    f.Key("weights");
    Serialize_gltf_nodes__weights(f, value.weights);

    f.Key("camera");
    f.Value(value.camera);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_nodes__children(JsonFormatter f, Int32[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes__matrix(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes__translation(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes__rotation(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes__scale(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes__weights(JsonFormatter f, Single[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_skins(JsonFormatter f, List<glTFSkin> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_skins_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_skins_LIST(JsonFormatter f, glTFSkin value)
{
    f.BeginMap();

    f.Key("inverseBindMatrices");
    f.Value(value.inverseBindMatrices);

    f.Key("joints");
    Serialize_gltf_skins__joints(f, value.joints);

    f.Key("skeleton");
    f.Value(value.skeleton);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("name");
    f.Value(value.name);


    f.EndMap();
}

public static void Serialize_gltf_skins__joints(JsonFormatter f, Int32[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_scenes(JsonFormatter f, List<gltfScene> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_scenes_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_scenes_LIST(JsonFormatter f, gltfScene value)
{
    f.BeginMap();

    f.Key("nodes");
    Serialize_gltf_scenes__nodes(f, value.nodes);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);

    f.Key("name");
    f.Value(value.name);


    f.EndMap();
}

public static void Serialize_gltf_scenes__nodes(JsonFormatter f, Int32[] value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations(JsonFormatter f, List<glTFAnimation> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_animations_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations_LIST(JsonFormatter f, glTFAnimation value)
{
    f.BeginMap();

    f.Key("name");
    f.Value(value.name);

    f.Key("channels");
    Serialize_gltf_animations__channels(f, value.channels);

    f.Key("samplers");
    Serialize_gltf_animations__samplers(f, value.samplers);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_animations__channels(JsonFormatter f, List<glTFAnimationChannel> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_animations__channels_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations__channels_LIST(JsonFormatter f, glTFAnimationChannel value)
{
    f.BeginMap();

    f.Key("sampler");
    f.Value(value.sampler);

    f.Key("target");
    Serialize_gltf_animations__channels__target(f, value.target);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_animations__channels__target(JsonFormatter f, glTFAnimationTarget value)
{
    f.BeginMap();

    f.Key("node");
    f.Value(value.node);

    f.Key("path");
    f.Value(value.path);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_animations__samplers(JsonFormatter f, List<glTFAnimationSampler> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_animations__samplers_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations__samplers_LIST(JsonFormatter f, glTFAnimationSampler value)
{
    f.BeginMap();

    f.Key("input");
    f.Value(value.input);

    f.Key("interpolation");
    f.Value(value.interpolation);

    f.Key("output");
    f.Value(value.output);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_cameras(JsonFormatter f, List<glTFCamera> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_cameras_LIST(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_cameras_LIST(JsonFormatter f, glTFCamera value)
{
    f.BeginMap();

    f.Key("orthographic");
    Serialize_gltf_cameras__orthographic(f, value.orthographic);

    f.Key("perspective");
    Serialize_gltf_cameras__perspective(f, value.perspective);

    f.Key("type");
    f.Value(value.type.ToString().ToLower());

    f.Key("name");
    f.Value(value.name);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_cameras__orthographic(JsonFormatter f, glTFOrthographic value)
{
    f.BeginMap();

    f.Key("xmag");
    f.Value(value.xmag);

    f.Key("ymag");
    f.Value(value.ymag);

    f.Key("zfar");
    f.Value(value.zfar);

    f.Key("znear");
    f.Value(value.znear);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_cameras__perspective(JsonFormatter f, glTFPerspective value)
{
    f.BeginMap();

    f.Key("aspectRatio");
    f.Value(value.aspectRatio);

    f.Key("yfov");
    f.Value(value.yfov);

    f.Key("zfar");
    f.Value(value.zfar);

    f.Key("znear");
    f.Value(value.znear);

    f.Key("extensions");
    value.extras.Serialize(f);

    f.Key("extras");
    value.extras.Serialize(f);


    f.EndMap();
}

public static void Serialize_gltf_extensionsUsed(JsonFormatter f, List<String> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

public static void Serialize_gltf_extensionsRequired(JsonFormatter f, List<String> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    f.Value(item);

    }
    f.EndList();
}

    } // class
} // namespace
