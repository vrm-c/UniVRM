using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRM
{
    public class VRMExporterValidator
    {
        // Allows you to enable and disable the wizard create button, so that the user can not click it.
        public bool IsValid
        {
            get
            {
                var hasError = m_validations.Any(x => !x.CanExport);
                return !hasError && !MetaHasError;
            }
        }

        bool MetaHasError = false;

        List<Validation> m_validations = new List<Validation>();
        public IEnumerable<Validation> Validations => m_validations;

        /// <summary>
        /// ボーン名の重複を確認
        /// </summary>
        /// <returns></returns>
        bool DuplicateBoneNameExists(GameObject ExportRoot)
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

        public static bool IsFileNameLengthTooLong(string fileName)
        {
            return fileName.Length > 64;
        }

        public static bool HasRotationOrScale(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>())
            {
                if (t.localRotation != Quaternion.identity)
                {
                    return true;
                }
                if (t.localScale != Vector3.one)
                {
                    return true;
                }
            }

            return false;
        }

        static Vector3 GetForward(Transform l, Transform r)
        {
            if (l == null || r == null)
            {
                return Vector3.zero;
            }
            var lr = (r.position - l.position).normalized;
            return Vector3.Cross(lr, Vector3.up);
        }

        static string Msg(VRMExporterWizardMessages key)
        {
            return M17N.Getter.Msg(key);
        }

        /// <summary>
        /// ExportDialogを表示する前に確認する。
        /// </summary>
        /// <param name="ExportRoot"></param>
        /// <param name="m_settings"></param>
        /// <returns></returns>
        public bool RootAndHumanoidCheck(GameObject ExportRoot, VRMExportSettings m_settings, IReadOnlyList<UniGLTF.MeshExportInfo> info)
        {
            //
            // root
            //
            if (ExportRoot == null)
            {
                Validation.Error(Msg(VRMExporterWizardMessages.ROOT_EXISTS)).DrawGUI();
                return false;
            }
            if (ExportRoot.transform.parent != null)
            {
                Validation.Error(Msg(VRMExporterWizardMessages.NO_PARENT)).DrawGUI();
                return false;
            }

            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            if (renderers.All(x => !x.EnableForExport()))
            {
                Validation.Error(Msg(VRMExporterWizardMessages.NO_ACTIVE_MESH)).DrawGUI();
                return false;
            }

            if (HasRotationOrScale(ExportRoot) || info.Any(x => x.ExportBlendShapeCount > 0 && !x.HasSkinning))
            {
                // 正規化必用
                if (m_settings.PoseFreeze)
                {
                    // する
                    EditorGUILayout.HelpBox("PoseFreeze checked. OK", MessageType.Info);
                }
                else
                {
                    // しない
                    Validation.Warning(Msg(VRMExporterWizardMessages.ROTATION_OR_SCALEING_INCLUDED_IN_NODE)).DrawGUI();
                }
            }
            else
            {
                // 不要
                if (m_settings.PoseFreeze)
                {
                    // する
                    Validation.Warning(Msg(VRMExporterWizardMessages.IS_POSE_FREEZE_DONE)).DrawGUI();
                }
                else
                {
                    // しない
                    EditorGUILayout.HelpBox("Root OK", MessageType.Info);
                }
            }

            //
            // animator
            //
            var animator = ExportRoot.GetComponent<Animator>();
            if (animator == null)
            {
                Validation.Error(Msg(VRMExporterWizardMessages.NO_ANIMATOR)).DrawGUI();
                return false;
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                Validation.Error(Msg(VRMExporterWizardMessages.NO_AVATAR_IN_ANIMATOR)).DrawGUI();
                return false;
            }
            if (!avatar.isValid)
            {
                Validation.Error(Msg(VRMExporterWizardMessages.AVATAR_IS_NOT_VALID)).DrawGUI();
                return false;
            }
            if (!avatar.isHuman)
            {
                Validation.Error(Msg(VRMExporterWizardMessages.AVATAR_IS_NOT_HUMANOID)).DrawGUI();
                return false;
            }
            {
                var l = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                var r = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                var f = GetForward(l, r);
                if (Vector3.Dot(f, Vector3.forward) < 0.8f)
                {
                    Validation.Error(Msg(VRMExporterWizardMessages.FACE_Z_POSITIVE_DIRECTION)).DrawGUI();
                    return false;
                }
            }
            var jaw = animator.GetBoneTransform(HumanBodyBones.Jaw);
            if (jaw != null)
            {
                Validation.Warning(Msg(VRMExporterWizardMessages.JAW_BONE_IS_INCLUDED)).DrawGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Animator OK", MessageType.Info);
            }

            return true;
        }

        /// <summary>
        /// エクスポート可能か検証する。
        /// </summary>
        /// <returns></returns>
        public void Validate(GameObject ExportRoot, VRMExportSettings m_settings, VRMMetaObject meta)
        {
            m_validations.Clear();
            if (ExportRoot == null)
            {
                return;
            }
            var proxy = ExportRoot.GetComponent<VRMBlendShapeProxy>();

            m_validations.AddRange(_Validate(ExportRoot, m_settings));
            m_validations.AddRange(VRMSpringBoneValidator.Validate(ExportRoot));
            var firstPerson = ExportRoot.GetComponent<VRMFirstPerson>();
            if (firstPerson != null)
            {
                m_validations.AddRange(firstPerson.Validate());
            }
            if (proxy != null)
            {
                m_validations.AddRange(proxy.Validate());
            }
            MetaHasError = meta.Validate().Any();
        }

        IEnumerable<Validation> _Validate(GameObject ExportRoot, VRMExportSettings m_settings)
        {
            if (ExportRoot == null)
            {
                yield break;
            }

            if (DuplicateBoneNameExists(ExportRoot))
            {
                yield return Validation.Warning(Msg(VRMExporterWizardMessages.DUPLICATE_BONE_NAME_EXISTS));
            }

            if (m_settings.ReduceBlendshape && ExportRoot.GetComponent<VRMBlendShapeProxy>() == null)
            {
                yield return Validation.Error(Msg(VRMExporterWizardMessages.NEEDS_VRM_BLENDSHAPE_PROXY));
            }

            var renderers = ExportRoot.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                for (int i = 0; i < r.sharedMaterials.Length; ++i)
                    if (r.sharedMaterials[i] == null)
                    {
                        yield return Validation.Error($"Renderer: {r.name}.Materials[{i}] is null. please fix it");
                    }
            }

            var materials = renderers.SelectMany(x => x.sharedMaterials).Where(x => x != null).Distinct();
            foreach (var material in materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.shader.name == "Standard")
                {
                    // standard
                    continue;
                }

                if (VRMMaterialExporter.UseUnlit(material.shader.name))
                {
                    // unlit
                    continue;
                }

                if (VRMMaterialExporter.VRMExtensionShaders.Contains(material.shader.name))
                {
                    // VRM supported
                    continue;
                }

                yield return Validation.Warning($"Material: {material.name}. Unknown Shader: \"{material.shader.name}\" is used. {Msg(VRMExporterWizardMessages.UNKNOWN_SHADER)}");
            }

            foreach (var material in materials)
            {
                if (IsFileNameLengthTooLong(material.name))
                    yield return Validation.Error(Msg(VRMExporterWizardMessages.FILENAME_TOO_LONG) + material.name);
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
                    yield return Validation.Error(Msg(VRMExporterWizardMessages.FILENAME_TOO_LONG) + textureName);
            }

            var vrmMeta = ExportRoot.GetComponent<VRMMeta>();
            if (vrmMeta != null && vrmMeta.Meta != null && vrmMeta.Meta.Thumbnail != null)
            {
                var thumbnailName = vrmMeta.Meta.Thumbnail.name;
                if (IsFileNameLengthTooLong(thumbnailName))
                    yield return Validation.Error(Msg(VRMExporterWizardMessages.FILENAME_TOO_LONG) + thumbnailName);
            }

            var meshFilters = ExportRoot.GetComponentsInChildren<MeshFilter>();
            var meshesName = meshFilters.Select(x => x.sharedMesh.name).Distinct();
            foreach (var meshName in meshesName)
            {
                if (IsFileNameLengthTooLong(meshName))
                    yield return Validation.Error(Msg(VRMExporterWizardMessages.FILENAME_TOO_LONG) + meshName);
            }

            var skinnedmeshRenderers = ExportRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            var skinnedmeshesName = skinnedmeshRenderers.Select(x => x.sharedMesh.name).Distinct();
            foreach (var skinnedmeshName in skinnedmeshesName)
            {
                if (IsFileNameLengthTooLong(skinnedmeshName))
                    yield return Validation.Error(Msg(VRMExporterWizardMessages.FILENAME_TOO_LONG) + skinnedmeshName);
            }
        }
    }
}
