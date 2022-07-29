using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRM.Sample.BlendShapeMenu
{
    public static class BlendShapeMenu
    {
        static string[] NAMES = new string[]
        {
            "BrowInnerUp",
            "BrowDownLeft",
            "BrowDownRight",
            "BrowOuterUpLeft",
            "BrowOuterUpRight",
            "EyeLookUpLeft",
            "EyeLookUpRight",
            "EyeLookDownLeft",
            "EyeLookDownRight",
            "EyeLookInLeft",
            "EyeLookInRight",
            "EyeLookOutLeft",
            "EyeLookOutRight",
            "EyeBlinkLeft",
            "EyeBlinkRight",
            "EyeSquintRight",
            "EyeSquintLeft",
            "EyeWideLeft",
            "EyeWideRight",
            "CheekPuff",
            "CheekSquintLeft",
            "CheekSquintRight",
            "NoseSneerLeft",
            "NoseSneerRight",
            "JawOpen",
            "JawForward",
            "JawLeft",
            "JawRight",
            "MouthFunnel",
            "MouthPucker",
            "MouthLeft",
            "MouthRight",
            "MouthRollUpper",
            "MouthRollLower",
            "MouthShrugUpper",
            "MouthShrugLower",
            "MouthClose",
            "MouthSmileLeft",
            "MouthSmileRight",
            "MouthFrownLeft",
            "MouthFrownRight",
            "MouthDimpleLeft",
            "MouthDimpleRight",
            "MouthUpperUpLeft",
            "MouthUpperUpRight",
            "MouthLowerDownLeft",
            "MouthLowerDownRight",
            "MouthPressLeft",
            "MouthPressRight",
            "MouthStretchLeft",
            "MouthStretchRight",
            "TongueOut",
        };

        /// <summary>
        /// fullpath C:/path/to/project/Assets/some.preset to Assets/some.preset
        /// </summary>
        /// <param name="src"></param>
        static string ToUnityPath(string src)
        {
            if (!src.StartsWith(Application.dataPath))
            {
                // not in Assets
                throw new System.Exception("not in Assets");
            }

            return "Assets" + src.Substring(Application.dataPath.Length);
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/BlendShapeAvatar/Add ARKit FaceTracking BlendShapes")]
        public static void AddARKitFaceTrackingBlendShapes(MenuCommand command)
        {
            // Debug.Log(command.context);
            var avatar = command.context as BlendShapeAvatar;
            if (avatar == null)
            {
                Debug.LogError("no context");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(avatar);

            var dir = EditorUtility.SaveFolderPanel("blendshape folder", assetPath, "");
            if (string.IsNullOrEmpty(dir))
            {
                // cancel
                return;
            }

            var assetDir = ToUnityPath(dir);
            var sb = new StringBuilder();
            foreach (var name in NAMES)
            {
                if (avatar.Clips.Find(x => x != null && x.Preset == BlendShapePreset.Unknown && x.BlendShapeName == name))
                {
                    // already exists
                    continue;
                }

                // new blendshape clip
                var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                clip.Preset = BlendShapePreset.Unknown;
                clip.name = name;
                clip.BlendShapeName = name;

                // write blendshape clip
                var clipPath = $"{assetDir}/{name}.asset";
                sb.AppendLine($"{clipPath}");
                AssetDatabase.CreateAsset(clip, clipPath);

                // add to avatar
                avatar.Clips.Add(clip);
            }

            Debug.Log("Create\n" + sb.ToString());
        }

        [MenuItem("CONTEXT/BlendShapeAvatar/Assign all BlendShapes in a folder")]
        static void AssginAllBlendShapesInAFolder(MenuCommand command)
        {
            // Debug.Log(command.context);
            var avatar = command.context as BlendShapeAvatar;
            if (avatar == null)
            {
                Debug.LogError("no context");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(avatar);

            var dir = EditorUtility.SaveFolderPanel("blendshape folder", assetPath, "");
            if (string.IsNullOrEmpty(dir))
            {
                // cancel
                return;
            }

            var assetDir = ToUnityPath(dir);
            var sb = new StringBuilder();
            foreach (var f in Directory.EnumerateFiles(dir))
            {
                var extension = Path.GetExtension(f).ToLower();
                if (extension != ".asset")
                {
                    // not asset
                    continue;
                }

                var clipPath = ToUnityPath(f);
                if (clipPath == null)
                {
                    // not BlendShapeClip
                    continue;
                }

                var clip = AssetDatabase.LoadAssetAtPath<BlendShapeClip>(clipPath);
                if (avatar.Clips.Contains(clip))
                {
                    // already exists
                    continue;
                }

                sb.AppendLine(clipPath);
                avatar.Clips.Add(clip);
            }

            Debug.Log($"Assign\n" + sb.ToString());
        }
#endif
    }
}
