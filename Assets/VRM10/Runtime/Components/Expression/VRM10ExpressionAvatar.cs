using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using MeshUtility;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniVRM10
{
    [CreateAssetMenu(menuName = "VRM10/ExpressionAvatar")]
    public sealed class VRM10ExpressionAvatar : ScriptableObject
    {
        public const string ExtractKey = "ExpressionAvatar";

        [SerializeField]
        public List<VRM10Expression> Clips = new List<VRM10Expression>();

        /// <summary>
        /// NullのClipを削除して詰める
        /// </summary>
        public void RemoveNullClip()
        {
            if (Clips == null)
            {
                return;
            }
            for (int i = Clips.Count - 1; i >= 0; --i)
            {
                if (Clips[i] == null)
                {
                    Clips.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Restore")]
        void Restore()
        {
            var assetPath = UnityPath.FromAsset(this);
            if (assetPath.IsNull)
            {
                return;
            }


            foreach (var x in assetPath.Parent.ChildFiles)
            {
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<VRM10Expression>(x.Value);
                if (clip == null) continue;

                if (!Clips.Contains(clip))
                {
                    Clips.Add(clip);
                }

                Debug.LogFormat("{0}", clip.name);
            }
            Clips = Clips.OrderBy(x => ExpressionKey.CreateFromClip(x)).ToList();
        }

        static public VRM10Expression CreateExpression(string path)
        {
            //Debug.LogFormat("{0}", path);
            var clip = ScriptableObject.CreateInstance<VRM10Expression>();
            clip.ExpressionName = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.ImportAsset(path);
            return clip;
            //Clips.Add(clip);
            //EditorUtility.SetDirty(this);
            //AssetDatabase.SaveAssets();
        }
#endif

        /// <summary>
        /// Unknown以外で存在しないものを全て作る
        /// </summary>
        public void CreateDefaultPreset()
        {
            foreach (var preset in ((UniGLTF.Extensions.VRMC_vrm.ExpressionPreset[])Enum.GetValues(typeof(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset)))
                .Where(x => x != UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
                )
            {
                CreateDefaultPreset(preset);
            }
        }

        void CreateDefaultPreset(UniGLTF.Extensions.VRMC_vrm.ExpressionPreset preset)
        {
            var clip = GetClip(new ExpressionKey(preset));
            if (clip != null) return;
            clip = ScriptableObject.CreateInstance<VRM10Expression>();
            clip.name = preset.ToString();
            clip.ExpressionName = preset.ToString();
            clip.Preset = preset;
            Clips.Add(clip);
        }

        public void SetClip(ExpressionKey key, VRM10Expression clip)
        {
            int index = -1;
            try
            {
                index = Clips.FindIndex(x => key.Match(x));
            }
            catch (Exception)
            {

            }
            if (index == -1)
            {
                Clips.Add(clip);
            }
            else
            {
                Clips[index] = clip;
            }
        }

        public VRM10Expression GetClip(ExpressionKey key)
        {
            if (Clips == null) return null;
            return Clips.FirstOrDefault(x => key.Match(x));
        }
    }
}
