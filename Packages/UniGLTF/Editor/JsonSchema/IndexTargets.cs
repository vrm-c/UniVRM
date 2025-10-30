using System.Collections.Generic;

namespace UniGLTF.JsonSchema
{
    /// <summary>
    /// JSON上の配列参照を列挙する
    /// </summary>
    public static class IndexTargets
    {
        public static Dictionary<string, string> Map = new Dictionary<string, string>{
            {".accessors[].bufferView", ".bufferViews"},
            {".accessors[].sparse.indices.bufferView", ".bufferViews"},
            {".accessors[].sparse.values.bufferView", ".bufferViews"},
            {".animations[].channels[].sampler", ".animations[{0}].samplers"},
            {".animations[].channels[].target.node", ".nodes"},
            {".animations[].samplers[].input", ".accessors"},
            {".animations[].samplers[].output", ".accessors"},
            {".bufferViews[].buffer", ".buffers"},
            {".images[].bufferView", ".bufferViews"},
            // {".materials[].extensions.KHR_materials_pbrSpecularGlossiness.diffuseTexture.index", ""},
            // {".materials[].extensions.KHR_materials_pbrSpecularGlossiness.specularGlossinessTexture.index", ""},
            {".materials[].pbrMetallicRoughness.baseColorTexture.index", ".textures"},
            {".materials[].pbrMetallicRoughness.metallicRoughnessTexture.index", ".textures"},
            {".materials[].normalTexture.index", ".textures"},
            {".materials[].occlusionTexture.index", ".textures"},
            {".materials[].emissiveTexture.index", ".textures"},
            // {".meshes[].primitives[].extensions.KHR_draco_mesh_compression.bufferView", ""},
            // {".meshes[].primitives[].extensions.KHR_draco_mesh_compression.attributes{}", ""},
            {".meshes[].primitives[].attributes{}", ".accessors"},
            {".meshes[].primitives[].indices", ".accessors"},
            {".meshes[].primitives[].material", ".materials"},
            {".meshes[].primitives[].targets[]{}", ".accessors"},
            {".nodes[].camera", ".cameras"},
            {".nodes[].children[]", ".nodes"},
            {".nodes[].skin", ".skins"},
            {".nodes[].mesh", ".meshes"},
            {".scene", ".scenes"},
            // {".scenes[].extensions.EXT_lights_image_based.light", ""},
            {".scenes[].nodes[]", ".nodes"},
            {".skins[].inverseBindMatrices", ".accessors"},
            {".skins[].skeleton", ".nodes"},
            {".skins[].joints[]", ".nodes"},
            {".textures[].sampler", ".samplers"},
            {".textures[].source", ".images"},
            // VRM
            {".extensions.VRM.humanoid.humanBones[].node", ".nodes"},
            {".extensions.VRM.firstPerson.firstPersonBone", ".nodes"},
            {".extensions.VRM.firstPerson.meshAnnotations[].mesh", ".meshes"},
            {".extensions.VRM.blendShapeMaster.blendShapeGroups[].binds[].mesh", ".meshes"},
            // {".extensions.VRM.blendShapeMaster.blendShapeGroups[].binds[].index", ".meshes[i].primitives[*].targets"},
            // {".extensions.VRM.blendShapeMaster.blendShapeGroups[].materialValues[].materialName", ".materials"},
            // {".extensions.VRM.blendShapeMaster.blendShapeGroups[].materialValues[].propertyName", ""},
            {".extensions.VRM.secondaryAnimation.boneGroups[].center", ".nodes"},
            {".extensions.VRM.secondaryAnimation.boneGroups[].bones[]", ".nodes"},
            {".extensions.VRM.secondaryAnimation.boneGroups[].colliderGroups[]", ".extensions.VRM.secondaryAnimation.colliderGroups"},
            {".extensions.VRM.secondaryAnimation.colliderGroups[].node", ".nodes"},
            {".extensions.VRM.materialProperties[].textureProperties{}", ".textures"},
        };
    }
}
