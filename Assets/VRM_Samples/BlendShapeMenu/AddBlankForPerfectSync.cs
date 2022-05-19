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

        [MenuItem("CONTEXT/BlendShapeAvatar/AddBlankForPerfectSync")]
        public static void AddBlankForPerfectSync(MenuCommand command)
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

            if (!dir.StartsWith(Application.dataPath))
            {
                // not in Assets
                EditorUtility.DisplayDialog("error", "folder is not in Assets", "ok");
                return;
            }

            var assetDir = "Assets" + dir.Substring(Application.dataPath.Length);
            var sb = new StringBuilder();
            foreach (var name in NAMES)
            {
                if (avatar.Clips.Find(x => x.Preset == BlendShapePreset.Unknown && x.BlendShapeName == name))
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

            Debug.Log(sb.ToString());
        }
    }
}
