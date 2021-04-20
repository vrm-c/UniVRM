using System.Collections.Generic;
using System.Linq;
using UniGLTF.M17N;
using UnityEngine;

namespace UniGLTF
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

            [LangMsg(Languages.ja, "ヒエラルキーに active なメッシュが含まれていません")]
            [LangMsg(Languages.en, "No active mesh")]
            NO_ACTIVE_MESH,

            [LangMsg(Languages.ja, "ヒエラルキーの中に同じ名前のGameObjectが含まれている。 エクスポートした場合に自動でリネームします")]
            [LangMsg(Languages.en, "There are bones with the same name in the hierarchy. They will be automatically renamed after export")]
            DUPLICATE_BONE_NAME_EXISTS,

            [LangMsg(Languages.ja, "SkinnedMeshRenderer.bones に重複する内容がある。エクスポートした場合に、bones の重複を取り除き、boneweights, bindposes を調整します")]
            [LangMsg(Languages.en, "There are duplicated bones in SkinnedMeshRenderer.bones. They will be exported as unique bones. boneweights and bindposes will also be adjusted")]
            NO_UNIQUE_JOINTS,
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

        static bool HasNoUniqueJoints(Renderer r)
        {
            if (r is SkinnedMeshRenderer skin)
            {
                if (skin.bones != null && skin.bones.Length != skin.bones.Distinct().Count())
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<Validation> ValidateRoot(GameObject ExportRoot)
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

            if (renderers.Any(x => HasNoUniqueJoints(x)))
            {
                yield return Validation.Warning(ExportValidatorMessages.NO_UNIQUE_JOINTS.Msg());
            }

            if (DuplicateNodeNameExists(ExportRoot))
            {
                yield return Validation.Warning(ExportValidatorMessages.DUPLICATE_BONE_NAME_EXISTS.Msg());
            }
        }
    }
}
