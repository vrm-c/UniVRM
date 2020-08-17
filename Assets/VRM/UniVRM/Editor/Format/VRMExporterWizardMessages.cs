using System.Collections.Generic;

namespace VRM
{
    /// <summary>
    /// エクスポートダイアログ用の簡易なメッセージカタログ
    /// </summary>
    public static class VRMExporterWizardMessages
    {
        public enum Languages
        {
            ja,
            en,
        }

        public struct LangMessages
        {
            public string ROOT_EXISTS;
            public string NO_PARENT;
            public string ROOT_WITHOUT_ROTATION_AND_SCALING_CHANGED;
            public string PREFAB_CANNOT_EXPORT;
            public string ROTATION_OR_SCALEING_INCLUDED_IN_NODE;
            public string IS_POSE_FREEZE_DONE;
            public string NO_ANIMATOR;
            public string FACE_Z_POSITIVE_DIRECTION;
            public string NO_AVATAR_IN_ANIMATOR;
            public string AVATAR_IS_NOT_VALID;
            public string AVATAR_IS_NOT_HUMANOID;
            public string JAW_BONE_IS_INCLUDED;
            public string DUPLICATE_BONE_NAME_EXISTS;
            public string NEEDS_VRM_BLENDSHAPE_PROXY;
            public string VERTEX_COLOR_IS_INCLUDED;
            public string NO_ACTIVE_MESH;
            public string UNKNOWN_SHADER;
            public string FILENAME_TOO_LONG;
        }

        public static readonly Dictionary<Languages, LangMessages> M17N = new Dictionary<Languages, LangMessages>
        {
            {Languages.ja, new LangMessages{
                ROOT_EXISTS ="ExportRootをセットしてください",
                NO_PARENT = "ExportRootに親はオブジェクトは持てません",
                ROOT_WITHOUT_ROTATION_AND_SCALING_CHANGED = "ExportRootに回転・拡大縮小は持てません。子階層で回転・拡大縮小してください",
                PREFAB_CANNOT_EXPORT = "シーンに出していない Prefab はエクスポートできません(細かい挙動が違い、想定外の動作をところがあるため)。シーンに展開してからエクスポートしてください",
                ROTATION_OR_SCALEING_INCLUDED_IN_NODE = "回転・拡大縮小を持つノードが含まれています。正規化が必用です。Setting の PoseFreeze を有効にしてください",
                IS_POSE_FREEZE_DONE = "正規化済みです。Setting の PoseFreeze は不要です",
                NO_ANIMATOR = "ExportRootに Animator がありません",
                FACE_Z_POSITIVE_DIRECTION = "Z+ 向きにしてください",
                NO_AVATAR_IN_ANIMATOR = "ExportRootの Animator に Avatar がありません",
                AVATAR_IS_NOT_VALID = "ExportRootの Animator.Avatar が不正です",
                AVATAR_IS_NOT_HUMANOID = "ExportRootの Animator.Avatar がヒューマノイドではありません。FBX importer の Rig で設定してください",
                JAW_BONE_IS_INCLUDED = "humanoid設定に顎が含まれている。FBX importer の rig 設定に戻って設定を解除することをおすすめします",
                DUPLICATE_BONE_NAME_EXISTS = "ヒエラルキーの中に同じ名前のGameObjectが含まれている。 エクスポートした場合に自動でリネームする",
                NEEDS_VRM_BLENDSHAPE_PROXY = "VRMBlendShapeProxyが必要です。先にVRMフォーマットに変換してください",
                VERTEX_COLOR_IS_INCLUDED = "ヒエラルキーに含まれる mesh に頂点カラーが含まれている",
                NO_ACTIVE_MESH = "ヒエラルキーに active なメッシュが含まれていない",
                UNKNOWN_SHADER = "Standard, Unlit, MToon 以外のマテリアルは、Standard になります",
                FILENAME_TOO_LONG = "名前が長すぎる。リネームしてください： ",
            }
            },
            {Languages.en, new LangMessages{
                ROOT_EXISTS = "Please set up a ExportRoot for model export",
                NO_PARENT = "ExportRoot must be topmost parent",
                ROOT_WITHOUT_ROTATION_AND_SCALING_CHANGED = "ExportRoot's rotation and scaling are not allowed to change. Please set up rotation and scaling in child node",
                PREFAB_CANNOT_EXPORT = "Prefab Asset cannot be exported. Prefab Asset has different behaviour with Scene GameObject. Please put the prefab into the scene",
                ROTATION_OR_SCALEING_INCLUDED_IN_NODE = " Normalization is required. There are nodes (child GameObject) where rotation and scaling are not default. Please enable PoseFreeze",
                IS_POSE_FREEZE_DONE = "Normalization has been done. PoseFreeze is not required",
                NO_ANIMATOR = "No Animator in ExportRoot",
                FACE_Z_POSITIVE_DIRECTION = "The model needs to face the positive Z-axis",
                NO_AVATAR_IN_ANIMATOR = "No Avatar in ExportRoot's Animator",
                AVATAR_IS_NOT_VALID = "Animator.avatar in ExportRoot is not valid",
                AVATAR_IS_NOT_HUMANOID = "Animator.avatar is not humanoid. Please change model's AnimationType to humanoid",
                JAW_BONE_IS_INCLUDED = "Jaw bone is included. It may not what you intended. Please check the humanoid avatar setting screen",
                DUPLICATE_BONE_NAME_EXISTS = "There are bones with the same name in the hierarchy. They will be automatically renamed after export",
                NEEDS_VRM_BLENDSHAPE_PROXY = "VRMBlendShapeProxy is required. Please convert to VRM format first",
                VERTEX_COLOR_IS_INCLUDED = "This model contains vertex color",
                NO_ACTIVE_MESH = "No active mesh",
                UNKNOWN_SHADER = "It will export as `Standard` fallback",
                FILENAME_TOO_LONG = "FileName is too long: ",
            }
            },
        };
    }
}
