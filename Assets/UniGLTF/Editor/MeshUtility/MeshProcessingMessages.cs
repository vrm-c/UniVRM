using UniGLTF.M17N;

namespace UniGLTF.MeshUtility
{
    public enum MeshProcessingMessages
    {
        [LangMsg(Languages.ja, "ターゲットオブジェクト")]
        [LangMsg(Languages.en, "TargetObject")]
        TARGET_OBJECT,

        [LangMsg(Languages.ja, "BlendShapeを含むメッシュは分割されます")]
        [LangMsg(Languages.en, "Meshes containing BlendShape will be split")]
        MESH_SEPARATOR,

        [LangMsg(Languages.ja, "メッシュを統合します。BlendShapeを含むメッシュは独立して統合されます")]
        [LangMsg(Languages.en, "Generate a single mesh. Meshes w/ BlendShape will be grouped into another one")]
        MESH_INTEGRATOR,

        [LangMsg(Languages.ja, "静的メッシュを一つに統合します")]
        [LangMsg(Languages.en, "Integrate static meshes into one")]
        STATIC_MESH_INTEGRATOR,

        [LangMsg(Languages.ja, "ボーン(Erase Rootのヒエラルキー)に関連するメッシュを削除します")]
        [LangMsg(Languages.en, "Eliminate meshes associated with the bones in EraseRoot hierarchy")]
        BONE_MESH_ERASER,

        [LangMsg(Languages.ja, "Skinned Meshを選んでください")]
        [LangMsg(Languages.en, "Select a skinned mesh")]
        SELECT_SKINNED_MESH,

        [LangMsg(Languages.ja, "Erase Rootを選んでください")]
        [LangMsg(Languages.en, "Select a erase root")]
        SELECT_ERASE_ROOT,

        [LangMsg(Languages.ja, "GameObjectを選んでください")]
        [LangMsg(Languages.en, "Select a GameObject first")]
        NO_GAMEOBJECT_SELECTED,

        [LangMsg(Languages.ja, "GameObjectにスキンメッシュが含まれていません")]
        [LangMsg(Languages.en, "No skinned mesh is contained")]
        NO_SKINNED_MESH,

        [LangMsg(Languages.ja, "GameObjectに静的メッシュが含まれていません")]
        [LangMsg(Languages.en, "No static mesh is contained")]
        NO_STATIC_MESH,

        [LangMsg(Languages.ja, "GameObjectにスキンメッシュ・静的メッシュが含まれていません")]
        [LangMsg(Languages.en, "Skinned/Static mesh is not contained")]
        NO_MESH,

        [LangMsg(Languages.ja, "ターゲットオブジェクトはVRMモデルです。`VRM0-> MeshIntegrator`を使ってください")]
        [LangMsg(Languages.en, "Target object is VRM model, use `VRM0 -> MeshIntegrator` instead")]
        VRM_DETECTED,
    }
}