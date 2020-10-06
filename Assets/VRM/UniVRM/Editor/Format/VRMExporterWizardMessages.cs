using VRM.M17N;

namespace VRM
{
    public enum VRMExporterWizardMessages
    {
        [LangMsg(Languages.ja, "ExportRootをセットしてください")]
        [LangMsg(Languages.en, "Please set up a ExportRoot for model export")]
        ROOT_EXISTS,


        [LangMsg(Languages.ja, "ExportRootに親はオブジェクトは持てません")]
        [LangMsg(Languages.en, "ExportRoot must be topmost parent")]
        NO_PARENT,

        [LangMsg(Languages.ja, "ExportRootに回転・拡大縮小は持てません。子階層で回転・拡大縮小してください")]
        [LangMsg(Languages.en, "ExportRoot's rotation and scaling are not allowed to change. Please set up rotation and scaling in child node")]
        ROOT_WITHOUT_ROTATION_AND_SCALING_CHANGED,

        [LangMsg(Languages.ja, "シーンに出していない Prefab はエクスポートできません(細かい挙動が違い、想定外の動作をところがあるため)。シーンに展開してからエクスポートしてください")]
        [LangMsg(Languages.en, "Prefab Asset cannot be exported. Prefab Asset has different behaviour with Scene GameObject. Please put the prefab into the scene")]
        PREFAB_CANNOT_EXPORT,

        [LangMsg(Languages.ja, "回転・拡大縮小もしくはWeightの無いBlendShapeが含まれています。正規化が必用です。Setting の PoseFreeze を有効にしてください")]
        [LangMsg(Languages.en, " Normalization is required. There are nodes (child GameObject) where rotation and scaling or blendshape without bone weight are not default. Please enable PoseFreeze")]
        ROTATION_OR_SCALEING_INCLUDED_IN_NODE,

        [LangMsg(Languages.ja, "正規化済みです。Setting の PoseFreeze は不要です")]
        [LangMsg(Languages.en, "Normalization has been done. PoseFreeze is not required")]
        IS_POSE_FREEZE_DONE,

        [LangMsg(Languages.ja, "ExportRootに Animator がありません")]
        [LangMsg(Languages.en, "No Animator in ExportRoot")]
        NO_ANIMATOR,

        [LangMsg(Languages.ja, "Z+ 向きにしてください")]
        [LangMsg(Languages.en, "The model needs to face the positive Z-axis")]
        FACE_Z_POSITIVE_DIRECTION,

        [LangMsg(Languages.ja, "ExportRootの Animator に Avatar がありません")]
        [LangMsg(Languages.en, "No Avatar in ExportRoot's Animator")]
        NO_AVATAR_IN_ANIMATOR,

        [LangMsg(Languages.ja, "ExportRootの Animator.Avatar が不正です")]
        [LangMsg(Languages.en, "Animator.avatar in ExportRoot is not valid")]
        AVATAR_IS_NOT_VALID,

        [LangMsg(Languages.ja, "ExportRootの Animator.Avatar がヒューマノイドではありません。FBX importer の Rig で設定してください")]
        [LangMsg(Languages.en, "Animator.avatar is not humanoid. Please change model's AnimationType to humanoid")]
        AVATAR_IS_NOT_HUMANOID,

        [LangMsg(Languages.ja, "humanoid設定に顎が含まれている。FBX importer の rig 設定で顎ボーンの割り当てを確認できます")]
        [LangMsg(Languages.en, "Jaw bone is included. It may not what you intended. Please check the humanoid avatar setting screen")]
        JAW_BONE_IS_INCLUDED,

        [LangMsg(Languages.ja, "ヒエラルキーの中に同じ名前のGameObjectが含まれている。 エクスポートした場合に自動でリネームする")]
        [LangMsg(Languages.en, "There are bones with the same name in the hierarchy. They will be automatically renamed after export")]
        DUPLICATE_BONE_NAME_EXISTS,

        [LangMsg(Languages.ja, "VRMBlendShapeProxyが必要です。先にVRMフォーマットに変換してください")]
        [LangMsg(Languages.en, "VRMBlendShapeProxy is required. Please convert to VRM format first")]
        NEEDS_VRM_BLENDSHAPE_PROXY,

        [LangMsg(Languages.en, "This model contains vertex color")]
        [LangMsg(Languages.ja, "ヒエラルキーに含まれる mesh に頂点カラーが含まれている")]
        VERTEX_COLOR_IS_INCLUDED,

        [LangMsg(Languages.ja, "ヒエラルキーに active なメッシュが含まれていない")]
        [LangMsg(Languages.en, "No active mesh")]
        NO_ACTIVE_MESH,

        [LangMsg(Languages.ja, "Standard, Unlit, MToon 以外のマテリアルは、Standard になります")]
        [LangMsg(Languages.en, "It will export as `Standard` fallback")]
        UNKNOWN_SHADER,

        [LangMsg(Languages.ja, "名前が長すぎる。リネームしてください： ")]
        [LangMsg(Languages.en, "FileName is too long: ")]
        FILENAME_TOO_LONG,
    }
}
