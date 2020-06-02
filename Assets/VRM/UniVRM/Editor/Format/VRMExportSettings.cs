using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public class VRMExportSettings
    {
        public GameObject Source;

        public string Title;

        public string Version;

        public string Author;

        public string ContactInformation;

        public string Reference;

        public bool ForceTPose = true;

        public bool PoseFreeze = true;

        public bool UseExperimentalExporter = false;

        public bool ReduceBlendshapeSize = false;

        public struct Validation
        {
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

        //ここで重複ボーン名のチェックをする
        bool DuplicateBoneNameExists()
        {
            var bones = Source.transform.Traverse().ToArray();
            var duplicates = bones
                .GroupBy(p => p.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            return (duplicates.Any());
        }

        public IEnumerable<Validation> Validate()
        {
            if (Source == null)
            {
                yield return Validation.Error("Require source");
                yield break;
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
                yield return Validation.Error("Find duplicate Bone names. Please check model's bone names. ");
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

            if (ReduceBlendshapeSize && Source.GetComponent<VRMBlendShapeProxy>() == null)
            {
                yield return Validation.Error("ReduceBlendshapeSize is need VRMBlendShapeProxy, you need to convert to VRM once.");
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
        }

        public void InitializeFrom(GameObject go)
        {
            if (Source == go) return;
            Source = go;

            var desc = Source == null ? null : go.GetComponent<VRMHumanoidDescription>();
            if (desc == null)
            {
                ForceTPose = true;
                PoseFreeze = true;
            }
            else
            {
                ForceTPose = false;
                PoseFreeze = false;
            }

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