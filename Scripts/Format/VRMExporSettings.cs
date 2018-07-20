using System;
using System.Collections.Generic;
using UnityEngine;


namespace VRM
{
    [Serializable]
    public class VRMExportSettings
    {
        public GameObject Source;

        public string Title;

        public string Author;

        public bool ForceTPose = true;

        public bool PoseFreeze = true;

        public IEnumerable<string> CanExport()
        {
            if (Source == null)
            {
                yield return "Require source";
                yield break;
            }

            var animator = Source.GetComponent<Animator>();
            if (animator == null)
            {
                yield return "Require animator. ";
            }
            else if (animator.avatar == null)
            {
                yield return "Require animator.avatar. ";
            }
            else if (!animator.avatar.isValid)
            {
                yield return "Animator.avatar is not valid. ";
            }
            else if (!animator.avatar.isHuman)
            {
                yield return "Animator.avatar is not humanoid. Please change model's AnimationType to humanoid. ";
            }

            if (string.IsNullOrEmpty(Title))
            {
                yield return "Require Title. ";
            }

            if (string.IsNullOrEmpty(Author))
            {
                yield return "Require Author. ";
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
                Author = meta.Meta.Author;
            }
            else
            {
                Title = go.name;
                //Author = "";
            }
        }
    }
}
