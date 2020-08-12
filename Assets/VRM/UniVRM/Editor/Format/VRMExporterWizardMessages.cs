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
            public string ROOT_WITHOUT_ROTATION_AND_SCALING;
            public string PREFAV_CANNOT_EXPORT;
        }

        public static readonly Dictionary<Languages, LangMessages> M17N = new Dictionary<Languages, LangMessages>
        {
            {Languages.ja, new LangMessages{
                ROOT_EXISTS = "ExportRootをセットしてください",
                NO_PARENT = "ExportRootに親はオブジェクトは持てません",
                ROOT_WITHOUT_ROTATION_AND_SCALING = "ExportRootに回転・拡大縮小は持てません。子階層で回転・拡大縮小してください",
                PREFAV_CANNOT_EXPORT = "シーンに出していない Prefab はエクスポートできません(細かい挙動が違い、想定外の動作をところがあるため)。シーンに展開してからエクスポートしてください",
            }
            },
            {Languages.en, new LangMessages{
                ROOT_EXISTS = "Set ExportRoot",
                NO_PARENT = "ExportRoot can not has parent",
                ROOT_WITHOUT_ROTATION_AND_SCALING = "ExportRoot can not has rotation or scaling. Please transform in child",
                PREFAV_CANNOT_EXPORT = "Asset prefab can not export. Because, asset prefab has different behaviour with scene gameobject. Please put the prefab into the scene",
            }
            },
        };
    }
}
