using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRMShaders.VRM10.MToon10.Editor
{
    public class MToonInspector : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var props = PropExtensions.PropertyNames
                .ToDictionary(x => x.Key, x => FindProperty(x.Value, properties));
            var materials = materialEditor.targets.Select(x => x as Material).ToArray();

            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
                if (PopupEnum<AlphaMode>("Alpha Mode", props[Prop.AlphaMode], materialEditor))
                {
                    Validate(materials);
                }

                if (PopupEnum<TransparentWithZWriteMode>("Transparent With ZWrite Mode", props[Prop.TransparentWithZWrite], materialEditor))
                {
                    Validate(materials);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            base.OnGUI(materialEditor, properties);
        }

        private static void Validate(Material[] materials)
        {
            foreach (var material in materials)
            {
                new MToonValidator(material).Validate();
            }
        }

        private static bool PopupEnum<T>(string name, MaterialProperty property, MaterialEditor editor) where T : struct
        {
            EditorGUI.showMixedValue = property.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            var ret = EditorGUILayout.Popup(name, (int) property.floatValue, Enum.GetNames(typeof(T)));
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                editor.RegisterPropertyChangeUndo("EnumPopUp");
                property.floatValue = ret;
            }

            EditorGUI.showMixedValue = false;
            return changed;
        }

    }
}