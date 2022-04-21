using System;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.EditorSettingsValidator
{
    [InitializeOnLoad]
    public sealed class UnityEditorSettingsValidatorWindow : EditorWindow
    {
        private static readonly UnityEditorSettingsValidator Validator = new UnityEditorSettingsValidator();

        static UnityEditorSettingsValidatorWindow()
        {
#if !UNITY_WEBGL
            EditorApplication.update += Validate;
#endif
        }

        private static void Validate()
        {
            if (!Validator.IsValid())
            {
                var window = GetWindow<UnityEditorSettingsValidatorWindow>(utility: true);
                window.minSize = new Vector2(320, 300);
            }
        }

        private void OnProjectChange()
        {
#if !UNITY_WEBGL
            Validate();
#endif
        }

        private void OnGUI()
        {
            if (Validator.IsValid())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(Messages.ThankYou.Msg(), new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                });

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(Messages.YouCanCloseThisWindow.Msg());
                if (GUILayout.Button(Messages.CloseWindowButton.Msg()))
                {
                    Close();
                }

                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(5);
                GUILayout.Label(Messages.RecommendedProjectSettingsForUniGltfAndUniVrm.Msg());
                GUILayout.Space(5);
                GUILayout.EndVertical();

                GUILayout.Space(10);

                foreach (var validator in Validator.Validators)
                {
                    GUILayout.Label($"{validator.HeaderDescription} ({Messages.CurrentValue.Msg()} = {validator.CurrentValueDescription})");
                    var oldEnabled = GUI.enabled;
                    GUI.enabled = !validator.IsValid;
                    if (GUILayout.Button($"{Messages.UseRecommended.Msg()} {validator.RecommendedValueDescription}"))
                    {
                        validator.Validate();
                    }

                    GUI.enabled = oldEnabled;

                    GUILayout.Space(5);
                }

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical(GUI.skin.box);
                if (GUILayout.Button(Messages.AcceptAllButton.Msg()))
                {
                    foreach (var validator in Validator.Validators)
                    {
                        if (!validator.IsValid)
                        {
                            validator.Validate();
                        }
                    }
                }

                GUILayout.EndVertical();
            }
        }
    }
}
