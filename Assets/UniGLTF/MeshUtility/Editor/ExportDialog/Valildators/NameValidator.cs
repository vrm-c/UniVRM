using System.Collections.Generic;
using System.Linq;
using MeshUtility.M17N;
using UnityEditor;
using UnityEngine;

namespace MeshUtility.Validators
{
    public class NameValidator
    {
        public enum ValidationMessages
        {
            [LangMsg(Languages.ja, "名前が長すぎる。リネームしてください： ")]
            [LangMsg(Languages.en, "FileName is too long: ")]
            FILENAME_TOO_LONG,
        }

        public static bool IsFileNameLengthTooLong(string fileName)
        {
            return fileName.Length > 64;
        }

        public static IEnumerable<Validation> Validate(GameObject ExportRoot)
        {
            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            var materials = renderers.SelectMany(x => x.sharedMaterials).Where(x => x != null).Distinct();
            foreach (var material in materials)
            {
                if (IsFileNameLengthTooLong(material.name))
                    yield return Validation.Error(ValidationMessages.FILENAME_TOO_LONG.Msg() + material.name);
            }

            var textureNameList = new List<string>();
            foreach (var material in materials)
            {
                var shader = material.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        if ((material.GetTexture(ShaderUtil.GetPropertyName(shader, i)) != null))
                        {
                            var textureName = material.GetTexture(ShaderUtil.GetPropertyName(shader, i)).name;
                            if (!textureNameList.Contains(textureName))
                                textureNameList.Add(textureName);
                        }
                    }
                }
            }

            foreach (var textureName in textureNameList)
            {
                if (IsFileNameLengthTooLong(textureName))
                    yield return Validation.Error(ValidationMessages.FILENAME_TOO_LONG.Msg() + textureName);
            }

            var meshFilters = ExportRoot.GetComponentsInChildren<MeshFilter>();
            var meshesName = meshFilters.Select(x => x.sharedMesh.name).Distinct();
            foreach (var meshName in meshesName)
            {
                if (IsFileNameLengthTooLong(meshName))
                    yield return Validation.Error(ValidationMessages.FILENAME_TOO_LONG.Msg() + meshName);
            }

            var skinnedmeshRenderers = ExportRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            var skinnedmeshesName = skinnedmeshRenderers.Select(x => x.sharedMesh.name).Distinct();
            foreach (var skinnedmeshName in skinnedmeshesName)
            {
                if (IsFileNameLengthTooLong(skinnedmeshName))
                    yield return Validation.Error(ValidationMessages.FILENAME_TOO_LONG.Msg() + skinnedmeshName);
            }

        }
    }
}
