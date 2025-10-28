using System;
using System.Collections.Generic;
using UniJSON;
using System.Linq;

namespace UniGLTF {

    static public class GltfSerializer
    {


public static void Serialize(JsonFormatter f, glTF value)
{
    f.BeginMap();


    if(value.asset!=null){
        f.Key("asset");
        Serialize_gltf_asset(f, value.asset);
    }

    if(value.buffers!=null&&value.buffers.Count>=1){
        f.Key("buffers");
        Serialize_gltf_buffers(f, value.buffers);
    }

    if(value.bufferViews!=null&&value.bufferViews.Count>=1){
        f.Key("bufferViews");
        Serialize_gltf_bufferViews(f, value.bufferViews);
    }

    if(value.accessors!=null&&value.accessors.Count>=1){
        f.Key("accessors");
        Serialize_gltf_accessors(f, value.accessors);
    }

    if(value.textures!=null&&value.textures.Count>=1){
        f.Key("textures");
        Serialize_gltf_textures(f, value.textures);
    }

    if(value.samplers!=null&&value.samplers.Count>=1){
        f.Key("samplers");
        Serialize_gltf_samplers(f, value.samplers);
    }

    if(value.images!=null&&value.images.Count>=1){
        f.Key("images");
        Serialize_gltf_images(f, value.images);
    }

    if(value.materials!=null&&value.materials.Count>=1){
        f.Key("materials");
        Serialize_gltf_materials(f, value.materials);
    }

    if(value.meshes!=null&&value.meshes.Count>=1){
        f.Key("meshes");
        Serialize_gltf_meshes(f, value.meshes);
    }

    if(value.nodes!=null&&value.nodes.Count>=1){
        f.Key("nodes");
        Serialize_gltf_nodes(f, value.nodes);
    }

    if(value.skins!=null&&value.skins.Count>=1){
        f.Key("skins");
        Serialize_gltf_skins(f, value.skins);
    }

    if(value.scene>=0){
        f.Key("scene");
        f.Value(value.scene);
    }

    if(value.scenes!=null&&value.scenes.Count>=1){
        f.Key("scenes");
        Serialize_gltf_scenes(f, value.scenes);
    }

    if(value.animations!=null&&value.animations.Count>=1){
        f.Key("animations");
        Serialize_gltf_animations(f, value.animations);
    }

    if(value.cameras!=null&&value.cameras.Count>=1){
        f.Key("cameras");
        Serialize_gltf_cameras(f, value.cameras);
    }

    if(value.extensionsUsed!=null&&value.extensionsUsed.Count>=1){
        f.Key("extensionsUsed");
        Serialize_gltf_extensionsUsed(f, value.extensionsUsed);
    }

    if(value.extensionsRequired!=null&&value.extensionsRequired.Count>=1){
        f.Key("extensionsRequired");
        Serialize_gltf_extensionsRequired(f, value.extensionsRequired);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_asset(JsonFormatter f, glTFAssets value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.generator)){
        f.Key("generator");
        f.Value(value.generator);
    }

    if(!string.IsNullOrEmpty(value.version)){
        f.Key("version");
        f.Value(value.version);
    }

    if(!string.IsNullOrEmpty(value.copyright)){
        f.Key("copyright");
        f.Value(value.copyright);
    }

    if(!string.IsNullOrEmpty(value.minVersion)){
        f.Key("minVersion");
        f.Value(value.minVersion);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_buffers(JsonFormatter f, List<glTFBuffer> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_buffers_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_buffers_ITEM(JsonFormatter f, glTFBuffer value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.uri)){
        f.Key("uri");
        f.Value(value.uri);
    }

    if(value.byteLength>=1){
        f.Key("byteLength");
        f.Value(value.byteLength);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    f.EndMap();
}

public static void Serialize_gltf_bufferViews(JsonFormatter f, List<glTFBufferView> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_bufferViews_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_bufferViews_ITEM(JsonFormatter f, glTFBufferView value)
{
    f.BeginMap();


    if(value.buffer>=0){
        f.Key("buffer");
        f.Value(value.buffer);
    }

    if(value.byteOffset>=0){
        f.Key("byteOffset");
        f.Value(value.byteOffset);
    }

    if(value.byteLength>=1){
        f.Key("byteLength");
        f.Value(value.byteLength);
    }

    if(value.byteStride>=4&&false){
        f.Key("byteStride");
        f.Value(value.byteStride);
    }

    if(true&&value.target!=0){
        f.Key("target");
        f.Value((int)value.target);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    f.EndMap();
}

public static void Serialize_gltf_accessors(JsonFormatter f, List<glTFAccessor> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_accessors_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_accessors_ITEM(JsonFormatter f, glTFAccessor value)
{
    f.BeginMap();


    if(value.bufferView.HasValue){
        f.Key("bufferView");
        f.Value(value.bufferView.Value);
    }

    if(value.byteOffset.HasValue){
        f.Key("byteOffset");
        f.Value(value.byteOffset.Value);
    }

    if(!string.IsNullOrEmpty(value.type)){
        f.Key("type");
        f.Value(value.type);
    }

    if(true){
        f.Key("componentType");
        f.Value((int)value.componentType);
    }

    if(value.count>=1){
        f.Key("count");
        f.Value(value.count);
    }

    if(value.max!=null&&value.max.Length>=1){
        f.Key("max");
        Serialize_gltf_accessors__max(f, value.max);
    }

    if(value.min!=null&&value.min.Length>=1){
        f.Key("min");
        Serialize_gltf_accessors__min(f, value.min);
    }

    if(true){
        f.Key("normalized");
        f.Value(value.normalized);
    }

    if(value.sparse!=null){
        f.Key("sparse");
        Serialize_gltf_accessors__sparse(f, value.sparse);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

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


    if(value.count>=1){
        f.Key("count");
        f.Value(value.count);
    }

    if(value.indices!=null){
        f.Key("indices");
        Serialize_gltf_accessors__sparse_indices(f, value.indices);
    }

    if(value.values!=null){
        f.Key("values");
        Serialize_gltf_accessors__sparse_values(f, value.values);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_accessors__sparse_indices(JsonFormatter f, glTFSparseIndices value)
{
    f.BeginMap();


    if(value.bufferView>=0){
        f.Key("bufferView");
        f.Value(value.bufferView);
    }

    if(value.byteOffset>=0){
        f.Key("byteOffset");
        f.Value(value.byteOffset);
    }

    if(true){
        f.Key("componentType");
        f.Value((int)value.componentType);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_accessors__sparse_values(JsonFormatter f, glTFSparseValues value)
{
    f.BeginMap();


    if(value.bufferView>=0){
        f.Key("bufferView");
        f.Value(value.bufferView);
    }

    if(value.byteOffset>=0){
        f.Key("byteOffset");
        f.Value(value.byteOffset);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_textures(JsonFormatter f, List<glTFTexture> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_textures_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_textures_ITEM(JsonFormatter f, glTFTexture value)
{
    f.BeginMap();


    if(value.sampler>=0){
        f.Key("sampler");
        f.Value(value.sampler);
    }

    if(value.source.HasValue){
        f.Key("source");
        f.Value(value.source.Value);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    f.EndMap();
}

public static void Serialize_gltf_samplers(JsonFormatter f, List<glTFTextureSampler> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_samplers_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_samplers_ITEM(JsonFormatter f, glTFTextureSampler value)
{
    f.BeginMap();


    if(true){
        f.Key("magFilter");
        f.Value((int)value.magFilter);
    }

    if(true){
        f.Key("minFilter");
        f.Value((int)value.minFilter);
    }

    if(true){
        f.Key("wrapS");
        f.Value((int)value.wrapS);
    }

    if(true){
        f.Key("wrapT");
        f.Value((int)value.wrapT);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    f.EndMap();
}

public static void Serialize_gltf_images(JsonFormatter f, List<glTFImage> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_images_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_images_ITEM(JsonFormatter f, glTFImage value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(!string.IsNullOrEmpty(value.uri)){
        f.Key("uri");
        f.Value(value.uri);
    }

    if(value.bufferView>=0){
        f.Key("bufferView");
        f.Value(value.bufferView);
    }

    if(!string.IsNullOrEmpty(value.mimeType)){
        f.Key("mimeType");
        f.Value(value.mimeType);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_materials(JsonFormatter f, List<glTFMaterial> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_materials_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_materials_ITEM(JsonFormatter f, glTFMaterial value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(value.pbrMetallicRoughness!=null){
        f.Key("pbrMetallicRoughness");
        Serialize_gltf_materials__pbrMetallicRoughness(f, value.pbrMetallicRoughness);
    }

    if(value.normalTexture!=null){
        f.Key("normalTexture");
        Serialize_gltf_materials__normalTexture(f, value.normalTexture);
    }

    if(value.occlusionTexture!=null){
        f.Key("occlusionTexture");
        Serialize_gltf_materials__occlusionTexture(f, value.occlusionTexture);
    }

    if(value.emissiveTexture!=null){
        f.Key("emissiveTexture");
        Serialize_gltf_materials__emissiveTexture(f, value.emissiveTexture);
    }

    if(value.emissiveFactor!=null&&value.emissiveFactor.Length>=3){
        f.Key("emissiveFactor");
        Serialize_gltf_materials__emissiveFactor(f, value.emissiveFactor);
    }

    if(!string.IsNullOrEmpty(value.alphaMode)){
        f.Key("alphaMode");
        f.Value(value.alphaMode);
    }

    if(value.alphaCutoff>=0&&value.alphaMode=="MASK"){
        f.Key("alphaCutoff");
        f.Value(value.alphaCutoff);
    }

    if(true){
        f.Key("doubleSided");
        f.Value(value.doubleSided);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_materials__pbrMetallicRoughness(JsonFormatter f, glTFPbrMetallicRoughness value)
{
    f.BeginMap();


    if(value.baseColorTexture!=null){
        f.Key("baseColorTexture");
        Serialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(f, value.baseColorTexture);
    }

    if(value.baseColorFactor!=null&&value.baseColorFactor.Length>=4){
        f.Key("baseColorFactor");
        Serialize_gltf_materials__pbrMetallicRoughness_baseColorFactor(f, value.baseColorFactor);
    }

    if(value.metallicRoughnessTexture!=null){
        f.Key("metallicRoughnessTexture");
        Serialize_gltf_materials__pbrMetallicRoughness_metallicRoughnessTexture(f, value.metallicRoughnessTexture);
    }

    if(value.metallicFactor>=0){
        f.Key("metallicFactor");
        f.Value(value.metallicFactor);
    }

    if(value.roughnessFactor>=0){
        f.Key("roughnessFactor");
        f.Value(value.roughnessFactor);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(JsonFormatter f, glTFMaterialBaseColorTextureInfo value)
{
    f.BeginMap();


    if(value.index>=0){
        f.Key("index");
        f.Value(value.index);
    }

    if(value.texCoord>=0){
        f.Key("texCoord");
        f.Value(value.texCoord);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

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


    if(value.index>=0){
        f.Key("index");
        f.Value(value.index);
    }

    if(value.texCoord>=0){
        f.Key("texCoord");
        f.Value(value.texCoord);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_materials__normalTexture(JsonFormatter f, glTFMaterialNormalTextureInfo value)
{
    f.BeginMap();


    if(true){
        f.Key("scale");
        f.Value(value.scale);
    }

    if(value.index>=0){
        f.Key("index");
        f.Value(value.index);
    }

    if(value.texCoord>=0){
        f.Key("texCoord");
        f.Value(value.texCoord);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_materials__occlusionTexture(JsonFormatter f, glTFMaterialOcclusionTextureInfo value)
{
    f.BeginMap();


    if(value.strength>=0){
        f.Key("strength");
        f.Value(value.strength);
    }

    if(value.index>=0){
        f.Key("index");
        f.Value(value.index);
    }

    if(value.texCoord>=0){
        f.Key("texCoord");
        f.Value(value.texCoord);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_materials__emissiveTexture(JsonFormatter f, glTFMaterialEmissiveTextureInfo value)
{
    f.BeginMap();


    if(value.index>=0){
        f.Key("index");
        f.Value(value.index);
    }

    if(value.texCoord>=0){
        f.Key("texCoord");
        f.Value(value.texCoord);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

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
    Serialize_gltf_meshes_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes_ITEM(JsonFormatter f, glTFMesh value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(value.primitives!=null&&value.primitives.Count>=1){
        f.Key("primitives");
        Serialize_gltf_meshes__primitives(f, value.primitives);
    }

    if(value.weights!=null&&value.weights.Length>=1){
        f.Key("weights");
        Serialize_gltf_meshes__weights(f, value.weights);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_meshes__primitives(JsonFormatter f, List<glTFPrimitives> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_meshes__primitives_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes__primitives_ITEM(JsonFormatter f, glTFPrimitives value)
{
    f.BeginMap();


    if(true){
        f.Key("mode");
        f.Value(value.mode);
    }

    if(value.indices>=0){
        f.Key("indices");
        f.Value(value.indices);
    }

    if(value.attributes!=null){
        f.Key("attributes");
        Serialize_gltf_meshes__primitives__attributes(f, value.attributes);
    }

    if(value.material.HasValidIndex()){
        f.Key("material");
        f.Value(value.material.Value);
    }

    if(value.targets!=null&&value.targets.Count>=1){
        f.Key("targets");
        Serialize_gltf_meshes__primitives__targets(f, value.targets);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_meshes__primitives__attributes(JsonFormatter f, glTFAttributes value)
{
    f.BeginMap();


    if(value.POSITION>=0){
        f.Key("POSITION");
        f.Value(value.POSITION);
    }

    if(value.NORMAL>=0){
        f.Key("NORMAL");
        f.Value(value.NORMAL);
    }

    if(value.TANGENT>=0){
        f.Key("TANGENT");
        f.Value(value.TANGENT);
    }

    if(value.TEXCOORD_0>=0){
        f.Key("TEXCOORD_0");
        f.Value(value.TEXCOORD_0);
    }

    if(value.TEXCOORD_1>=0){
        f.Key("TEXCOORD_1");
        f.Value(value.TEXCOORD_1);
    }

    if(value.COLOR_0>=0){
        f.Key("COLOR_0");
        f.Value(value.COLOR_0);
    }

    if(value.JOINTS_0>=0){
        f.Key("JOINTS_0");
        f.Value(value.JOINTS_0);
    }

    if(value.WEIGHTS_0>=0){
        f.Key("WEIGHTS_0");
        f.Value(value.WEIGHTS_0);
    }

    f.EndMap();
}

public static void Serialize_gltf_meshes__primitives__targets(JsonFormatter f, List<gltfMorphTarget> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_meshes__primitives__targets_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_meshes__primitives__targets_ITEM(JsonFormatter f, gltfMorphTarget value)
{
    f.BeginMap();


    if(value.POSITION>=0){
        f.Key("POSITION");
        f.Value(value.POSITION);
    }

    if(value.NORMAL>=0){
        f.Key("NORMAL");
        f.Value(value.NORMAL);
    }

    if(value.TANGENT>=0){
        f.Key("TANGENT");
        f.Value(value.TANGENT);
    }

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
    Serialize_gltf_nodes_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_nodes_ITEM(JsonFormatter f, glTFNode value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(value.children!=null&&value.children.Length>=1){
        f.Key("children");
        Serialize_gltf_nodes__children(f, value.children);
    }

    if(value.matrix!=null&&value.matrix.Length>=16){
        f.Key("matrix");
        Serialize_gltf_nodes__matrix(f, value.matrix);
    }

    if(value.translation!=null&&value.translation.Length>=3&&!value.translation.SequenceEqual(new float[]{0, 0, 0})){
        f.Key("translation");
        Serialize_gltf_nodes__translation(f, value.translation);
    }

    if(value.rotation!=null&&value.rotation.Length>=4&&!value.rotation.SequenceEqual(new float[]{0, 0, 0, 1})){
        f.Key("rotation");
        Serialize_gltf_nodes__rotation(f, value.rotation);
    }

    if(value.scale!=null&&value.scale.Length>=3&&!value.scale.SequenceEqual(new float[]{1, 1, 1})){
        f.Key("scale");
        Serialize_gltf_nodes__scale(f, value.scale);
    }

    if(value.mesh>=0){
        f.Key("mesh");
        f.Value(value.mesh);
    }

    if(value.skin>=0){
        f.Key("skin");
        f.Value(value.skin);
    }

    if(value.weights!=null&&value.weights.Length>=1){
        f.Key("weights");
        Serialize_gltf_nodes__weights(f, value.weights);
    }

    if(value.camera>=0){
        f.Key("camera");
        f.Value(value.camera);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

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
    Serialize_gltf_skins_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_skins_ITEM(JsonFormatter f, glTFSkin value)
{
    f.BeginMap();


    if(value.inverseBindMatrices>=0){
        f.Key("inverseBindMatrices");
        f.Value(value.inverseBindMatrices);
    }

    if(value.joints!=null&&value.joints.Length>=1){
        f.Key("joints");
        Serialize_gltf_skins__joints(f, value.joints);
    }

    if(value.skeleton>=0){
        f.Key("skeleton");
        f.Value(value.skeleton);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

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
    Serialize_gltf_scenes_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_scenes_ITEM(JsonFormatter f, gltfScene value)
{
    f.BeginMap();


    if(value.nodes!=null&&value.nodes.Length>=1){
        f.Key("nodes");
        Serialize_gltf_scenes__nodes(f, value.nodes);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

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
    Serialize_gltf_animations_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations_ITEM(JsonFormatter f, glTFAnimation value)
{
    f.BeginMap();


    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(value.channels!=null&&value.channels.Count>=1){
        f.Key("channels");
        Serialize_gltf_animations__channels(f, value.channels);
    }

    if(value.samplers!=null&&value.samplers.Count>=1){
        f.Key("samplers");
        Serialize_gltf_animations__samplers(f, value.samplers);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_animations__channels(JsonFormatter f, List<glTFAnimationChannel> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_animations__channels_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations__channels_ITEM(JsonFormatter f, glTFAnimationChannel value)
{
    f.BeginMap();


    if(value.sampler>=0){
        f.Key("sampler");
        f.Value(value.sampler);
    }

    if(value.target!=null){
        f.Key("target");
        Serialize_gltf_animations__channels__target(f, value.target);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_animations__channels__target(JsonFormatter f, glTFAnimationTarget value)
{
    f.BeginMap();


    if(value.node>=0){
        f.Key("node");
        f.Value(value.node);
    }

    if(!string.IsNullOrEmpty(value.path)){
        f.Key("path");
        f.Value(value.path);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_animations__samplers(JsonFormatter f, List<glTFAnimationSampler> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_animations__samplers_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_animations__samplers_ITEM(JsonFormatter f, glTFAnimationSampler value)
{
    f.BeginMap();


    if(value.input>=0){
        f.Key("input");
        f.Value(value.input);
    }

    if(!string.IsNullOrEmpty(value.interpolation)){
        f.Key("interpolation");
        f.Value(value.interpolation);
    }

    if(value.output>=0){
        f.Key("output");
        f.Value(value.output);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_cameras(JsonFormatter f, List<glTFCamera> value)
{
    f.BeginList();

    foreach(var item in value)
    {
    Serialize_gltf_cameras_ITEM(f, item);

    }
    f.EndList();
}

public static void Serialize_gltf_cameras_ITEM(JsonFormatter f, glTFCamera value)
{
    f.BeginMap();


    if(value.orthographic!=null){
        f.Key("orthographic");
        Serialize_gltf_cameras__orthographic(f, value.orthographic);
    }

    if(value.perspective!=null){
        f.Key("perspective");
        Serialize_gltf_cameras__perspective(f, value.perspective);
    }

    if(true){
        f.Key("type");
        f.Value(value.type.ToString().ToLower());
    }

    if(!string.IsNullOrEmpty(value.name)){
        f.Key("name");
        f.Value(value.name);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_cameras__orthographic(JsonFormatter f, glTFOrthographic value)
{
    f.BeginMap();


    if(true){
        f.Key("xmag");
        f.Value(value.xmag);
    }

    if(true){
        f.Key("ymag");
        f.Value(value.ymag);
    }

    if(value.zfar>0){
        f.Key("zfar");
        f.Value(value.zfar);
    }

    if(value.znear>=0){
        f.Key("znear");
        f.Value(value.znear);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

    f.EndMap();
}

public static void Serialize_gltf_cameras__perspective(JsonFormatter f, glTFPerspective value)
{
    f.BeginMap();


    if(value.aspectRatio>0){
        f.Key("aspectRatio");
        f.Value(value.aspectRatio);
    }

    if(value.yfov>0){
        f.Key("yfov");
        f.Value(value.yfov);
    }

    if(value.zfar>0){
        f.Key("zfar");
        f.Value(value.zfar);
    }

    if(value.znear>0){
        f.Key("znear");
        f.Value(value.znear);
    }

    if(value.extensions!=null){
        f.Key("extensions");
        value.extensions.Serialize(f);
    }

    if(value.extras!=null){
        f.Key("extras");
        value.extras.Serialize(f);
    }

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
