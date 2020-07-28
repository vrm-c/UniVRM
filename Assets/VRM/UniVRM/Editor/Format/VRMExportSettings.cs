using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public class VRMExportSettings
    {
        /// <summary>
        /// エクスポート対象
        /// </summary>
        public GameObject Source;

        #region Meta
        /// <summary>
        /// エクスポート名
        /// </summary>
        public string Title;

        /// <summary>
        /// エクスポートバージョン(エクスポートするModelのバージョン)
        /// </summary>
        public string Version;

        /// <summary>
        /// 作者
        /// </summary>
        public string Author;

        /// <summary>
        /// 作者連絡先
        /// </summary>
        public string ContactInformation;

        /// <summary>
        /// 作品引用
        /// </summary>
        public string Reference;
        #endregion

        #region Settings
        /// <summary>
        /// エクスポート時に強制的にT-Pose化する
        /// </summary>
        [Tooltip("Option")]
        public bool ForceTPose = false;

        /// <summary>
        /// エクスポート時にヒエラルキーの正規化を実施する
        /// </summary>
        [Tooltip("Require only first time")]
        public bool PoseFreeze = true;

        /// <summary>
        /// エクスポート時に新しいJsonSerializerを使う
        /// </summary>
        [Tooltip("Use new JSON serializer")]
        public bool UseExperimentalExporter = false;

        /// <summary>
        /// BlendShapeのシリアライズにSparseAccessorを使う
        /// </summary>
        [Tooltip("Use sparse accessor for blendshape. This may reduce vrm size")]
        public bool UseSparseAccessor = false;

        /// <summary>
        /// BlendShapeのPositionのみをエクスポートする
        /// </summary>
        [Tooltip("UniVRM-0.54 or later can load it. Otherwise fail to load")]
        public bool OnlyBlendshapePosition = false;

        /// <summary>
        /// エクスポート時にBlendShapeClipから参照されないBlendShapeを削除する
        /// </summary>
        [Tooltip("Remove blendshape that is not used from BlendShapeClip")]
        public bool ReduceBlendshape = false;

        /// <summary>
        /// skip if BlendShapeClip.Preset == Unknown
        /// </summary>
        [Tooltip("Remove blendShapeClip that preset is Unknown")]
        public bool ReduceBlendshapeClip = false;

        /// <summary>
        /// 頂点カラーを削除する
        /// </summary>
        [Tooltip("Remove vertex color")]
        public bool RemoveVertexColor = false;
        #endregion

        public static bool IsFileNameLengthTooLong(string fileName)
        {
            return fileName.Length > 64;
        }

        public struct Validation
        {
            /// <summary>
            /// エクスポート可能か否か。
            /// true のメッセージは警告
            /// false のメッセージはエラー
            /// </summary>
            public readonly bool CanExport;
            public readonly String Message;

            Validation(bool canExport, string message)
            {
                CanExport = canExport;
                Message = message;
            }

            public static Validation Error(string msg)
            {
                return new Validation(false, msg);
            }

            public static Validation Warning(string msg)
            {
                return new Validation(true, msg);
            }
        }

        /// <summary>
        /// ボーン名の重複を確認
        /// </summary>
        /// <returns></returns>
        bool DuplicateBoneNameExists()
        {
            var bones = Source.transform.Traverse().ToArray();
            var duplicates = bones
                .GroupBy(p => p.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            return (duplicates.Any());
        }

        /// <summary>
        /// エクスポート可能か検証する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Validation> Validate()
        {
            if (Source == null)
            {
                yield return Validation.Error("Require source");
                yield break;
            }

            if (Source.transform.position != Vector3.zero ||
                Source.transform.rotation != Quaternion.identity ||
                Source.transform.localScale != Vector3.one)
            {
                // EditorUtility.DisplayDialog("Error", "The Root transform should have Default translation, rotation and scale.", "ok");
                yield return Validation.Warning("The Root translation, rotation and scale will be dropped.");
            }

            var animator = Source.GetComponent<Animator>();
            if (animator == null)
            {
                yield return Validation.Error("Require animator. ");
            }
            else if (animator.avatar == null)
            {
                yield return Validation.Error("Require animator.avatar. ");
            }
            else if (!animator.avatar.isValid)
            {
                yield return Validation.Error("Animator.avatar is not valid. ");
            }
            else if (!animator.avatar.isHuman)
            {
                yield return Validation.Error("Animator.avatar is not humanoid. Please change model's AnimationType to humanoid. ");
            }

            var jaw = animator.GetBoneTransform(HumanBodyBones.Jaw);
            if (jaw != null)
            {
                yield return Validation.Warning("Jaw bone is included. It may not be what you intended. Please check the humanoid avatar setting screen");
            }

            if (DuplicateBoneNameExists())
            {
                yield return Validation.Warning("There is a bone with the same name in the hierarchy. If exported, these bones will be automatically renamed.");
            }

            if (string.IsNullOrEmpty(Title))
            {
                yield return Validation.Error("Require Title. ");
            }
            if (string.IsNullOrEmpty(Version))
            {
                yield return Validation.Error("Require Version. ");
            }
            if (string.IsNullOrEmpty(Author))
            {
                yield return Validation.Error("Require Author. ");
            }

            if (ReduceBlendshape && Source.GetComponent<VRMBlendShapeProxy>() == null)
            {
                yield return Validation.Error("ReduceBlendshapeSize needs VRMBlendShapeProxy. You need to convert to VRM once.");
            }

            var vertexColor = Source.GetComponentsInChildren<SkinnedMeshRenderer>().Any(x => x.sharedMesh.colors.Length > 0);
            if (vertexColor)
            {
                yield return Validation.Warning("This model contains vertex color");
            }

            var renderers = Source.GetComponentsInChildren<Renderer>();
            if (renderers.All(x => !x.gameObject.activeInHierarchy))
            {
                yield return Validation.Error("No active mesh");
            }

            var materials = renderers.SelectMany(x => x.sharedMaterials).Distinct();
            foreach (var material in materials)
            {
                if (material.shader.name == "Standard")
                {
                    // standard
                    continue;
                }

                if (MaterialExporter.UseUnlit(material.shader.name))
                {
                    // unlit
                    continue;
                }

                if (VRMMaterialExporter.VRMExtensionShaders.Contains(material.shader.name))
                {
                    // VRM supported
                    continue;
                }

                yield return Validation.Warning(string.Format("unknown material '{0}' is used. this will export as `Standard` fallback", material.shader.name));
            }

            foreach (var material in materials)
            {
                if (IsFileNameLengthTooLong(material.name))
                    yield return Validation.Error(string.Format("FileName '{0}' is too long. ", material.name));
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
                    yield return Validation.Error(string.Format("FileName '{0}' is too long. ", textureName));
            }

            var vrmMeta = Source.GetComponent<VRMMeta>();
            if (vrmMeta != null && vrmMeta.Meta != null && vrmMeta.Meta.Thumbnail != null)
            {
                var thumbnailName = vrmMeta.Meta.Thumbnail.name;
                if (IsFileNameLengthTooLong(thumbnailName))
                    yield return Validation.Error(string.Format("FileName '{0}' is too long. ", thumbnailName));
            }

            var meshFilters = Source.GetComponentsInChildren<MeshFilter>();
            var meshesName = meshFilters.Select(x => x.sharedMesh.name).Distinct();
            foreach (var meshName in meshesName)
            {
                if (IsFileNameLengthTooLong(meshName))
                    yield return Validation.Error(string.Format("FileName '{0}' is too long. ", meshName));
            }

            var skinnedmeshRenderers = Source.GetComponentsInChildren<SkinnedMeshRenderer>();
            var skinnedmeshesName = skinnedmeshRenderers.Select(x => x.sharedMesh.name).Distinct();
            foreach (var skinnedmeshName in skinnedmeshesName)
            {
                if (IsFileNameLengthTooLong(skinnedmeshName))
                    yield return Validation.Error(string.Format("FileName '{0}' is too long. ", skinnedmeshName));
            }
        }

        /// <summary>
        /// 対象のモデルからMeta情報を取得し、エクスポート設定を初期する
        /// </summary>
        /// <param name="go"></param>
        public void InitializeFrom(GameObject go)
        {
            if (Source == go) return;
            Source = go;

            //
            // initialize
            //
            var desc = Source == null ? null : go.GetComponent<VRMHumanoidDescription>();
            if (desc == null)
            {
                // 初回のVRMエクスポートとみなす
                ForceTPose = false; // option
                PoseFreeze = true;
            }
            else
            {
                // すでに正規化済みとみなす
                ForceTPose = false;
                PoseFreeze = false;
            }

            //
            // Meta
            //
            var meta = Source == null ? null : go.GetComponent<VRMMeta>();
            if (meta != null && meta.Meta != null)
            {
                Title = meta.Meta.Title;
                Version = string.IsNullOrEmpty(meta.Meta.Version) ? "0.0" : meta.Meta.Version;
                Author = meta.Meta.Author;
                ContactInformation = meta.Meta.ContactInformation;
                Reference = meta.Meta.Reference;
            }
            else
            {
                Title = go.name;
                Version = "0.0";
            }
        }
    }
}