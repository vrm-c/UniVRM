using System.Collections.Generic;
using System.Linq;
using MeshUtility.M17N;
using UnityEngine;

namespace MeshUtility.Validators
{
    public static class HierarchyValidator
    {
        public enum ExportValidatorMessages
        {
            [LangMsg(Languages.ja, "ExportRootをセットしてください")]
            [LangMsg(Languages.en, "Please set up a ExportRoot for model export")]
            ROOT_EXISTS,

            [LangMsg(Languages.ja, "ExportRootに親はオブジェクトは持てません")]
            [LangMsg(Languages.en, "ExportRoot must be topmost parent")]
            NO_PARENT,

            [LangMsg(Languages.ja, "ヒエラルキーに active なメッシュが含まれていない")]
            [LangMsg(Languages.en, "No active mesh")]
            NO_ACTIVE_MESH,

            [LangMsg(Languages.ja, "ヒエラルキーの中に同じ名前のGameObjectが含まれている。 エクスポートした場合に自動でリネームする")]
            [LangMsg(Languages.en, "There are bones with the same name in the hierarchy. They will be automatically renamed after export")]
            DUPLICATE_BONE_NAME_EXISTS,
        }

        /// <summary>
        /// ボーン名の重複を確認
        /// </summary>
        /// <returns></returns>
        static bool DuplicateNodeNameExists(GameObject ExportRoot)
        {
            if (ExportRoot == null)
            {
                return false;
            }
            var bones = ExportRoot.transform.GetComponentsInChildren<Transform>();
            var duplicates = bones
                .GroupBy(p => p.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            return (duplicates.Any());
        }

        public static IEnumerable<Validation> Validate(GameObject ExportRoot)
        {
            if (ExportRoot == null)
            {
                yield return Validation.Critical(ExportValidatorMessages.ROOT_EXISTS.Msg());
                yield break;
            }

            if (ExportRoot.transform.parent != null)
            {
                yield return Validation.Critical(ExportValidatorMessages.NO_PARENT.Msg());
                yield break;
            }

            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            if (renderers.All(x => !x.EnableForExport()))
            {
                yield return Validation.Critical(ExportValidatorMessages.NO_ACTIVE_MESH.Msg());
                yield break;
            }

            if (DuplicateNodeNameExists(ExportRoot))
            {
                yield return Validation.Warning(ExportValidatorMessages.DUPLICATE_BONE_NAME_EXISTS.Msg());
            }
        }
    }
}
