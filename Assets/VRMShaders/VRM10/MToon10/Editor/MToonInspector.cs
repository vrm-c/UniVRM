using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using VRMShaders.VRM10.MToon10.Runtime;

namespace VRMShaders.VRM10.MToon10.Editor
{
    public sealed class MToonInspector : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var props = MToon10Properties.UnityShaderLabNames
                .ToDictionary(x => x.Key, x => FindProperty(x.Value, properties));
            var materials = materialEditor.targets.Select(x => x as Material).ToArray();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox("This shader is for VRM 1.0.", MessageType.Info);

            using (new LabelScope("Editor Settings"))
            {
                PopupEnum<MToon10EditorEditMode>("Edit Mode", props[MToon10Prop.EditorEditMode], materialEditor);
            }
            var editMode = (MToon10EditorEditMode)(int)props[MToon10Prop.EditorEditMode].floatValue;
            var isAdvancedEditMode = editMode == MToon10EditorEditMode.Advanced;

            using (new LabelScope("Rendering"))
            {
                PopupEnum<MToon10AlphaMode>("Alpha Mode", props[MToon10Prop.AlphaMode], materialEditor);
                var alphaMode = (MToon10AlphaMode)(int)props[MToon10Prop.AlphaMode].floatValue;

                if (isAdvancedEditMode && alphaMode == MToon10AlphaMode.Transparent)
                {
                    PopupEnum<MToon10TransparentWithZWriteMode>(
                        "Transparent With ZWrite Mode",
                        props[MToon10Prop.TransparentWithZWrite],
                        materialEditor
                    );
                }

                if (alphaMode == MToon10AlphaMode.Cutout)
                {
                    materialEditor.ShaderProperty(props[MToon10Prop.AlphaCutoff], "Cutoff");
                }

                PopupEnum<MToon10DoubleSidedMode>("Double Sided", props[MToon10Prop.DoubleSided], materialEditor);

                if (isAdvancedEditMode)
                {
                    materialEditor.ShaderProperty(props[MToon10Prop.RenderQueueOffsetNumber], "RenderQueue Offset");
                }
            }

            using (new LabelScope("Lighting"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Lit Color, Alpha", "Lit (RGB), Alpha (A)"),
                    props[MToon10Prop.BaseColorTexture],
                    props[MToon10Prop.BaseColorFactor]
                );
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Shade Color", "Shade (RGB)"),
                    props[MToon10Prop.ShadeColorTexture],
                    props[MToon10Prop.ShadeColorFactor]
                );
                if (isAdvancedEditMode)
                {
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Normal Map", "Normal Map (RGB)"),
                        props[MToon10Prop.NormalTexture],
                        props[MToon10Prop.NormalTextureScale]
                    );
                }
                EditorGUILayout.Space();

                if (isAdvancedEditMode)
                {
                    materialEditor.ShaderProperty(props[MToon10Prop.ShadingToonyFactor], "Shading Toony");
                    materialEditor.ShaderProperty(props[MToon10Prop.ShadingShiftFactor], "Shading Shift");
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Additive Shading Shift", "Shading Shift (R)"),
                        props[MToon10Prop.ShadingShiftTexture],
                        props[MToon10Prop.ShadingShiftTextureScale]
                    );
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Presets");
                    if (GUILayout.Button("Default"))
                    {
                        props[MToon10Prop.ShadingToonyFactor].floatValue = 0.95f;
                        props[MToon10Prop.ShadingShiftFactor].floatValue = -0.05f;
                        props[MToon10Prop.ShadingShiftTexture].textureValue = null;
                    }
                    if (GUILayout.Button("Lambert"))
                    {
                        props[MToon10Prop.ShadingToonyFactor].floatValue = 0.5f;
                        props[MToon10Prop.ShadingShiftFactor].floatValue = -0.5f;
                        props[MToon10Prop.ShadingShiftTexture].textureValue = null;
                    }
                    if (GUILayout.Button("Cartoon"))
                    {
                        props[MToon10Prop.ShadingToonyFactor].floatValue = 1.0f;
                        props[MToon10Prop.ShadingShiftFactor].floatValue = 0.0f;
                        props[MToon10Prop.ShadingShiftTexture].textureValue = null;
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.BeginVertical(GUI.skin.box);
                    materialEditor.ShaderProperty(props[MToon10Prop.ShadingToonyFactor], "Shading Toony");
                    materialEditor.ShaderProperty(props[MToon10Prop.ShadingShiftFactor], "Shading Shift");
                    GUILayout.EndVertical();
                }

                if (props[MToon10Prop.ShadingShiftTexture].textureValue == null)
                {
                    var toony = props[MToon10Prop.ShadingToonyFactor].floatValue;
                    var shift = props[MToon10Prop.ShadingShiftFactor].floatValue;
                    if (toony - shift < 1.0f - 0.001f)
                    {
                        EditorGUILayout.HelpBox("The lit area includes non-lit area.", MessageType.Warning);
                    }
                }
            }

            if (isAdvancedEditMode)
            {
                using (new LabelScope("Global Illumination"))
                {
                    materialEditor.ShaderProperty(props[MToon10Prop.GiEqualizationFactor], "GI Equalization");
                }
            }

            using (new LabelScope("Emission"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Emission", "Emission (RGB)"),
                    props[MToon10Prop.EmissiveTexture],
                    props[MToon10Prop.EmissiveFactor]
                );
            }

            using (new LabelScope("Rim Lighting"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Rim Color", "Rim Color (RGB)"),
                    props[MToon10Prop.RimMultiplyTexture]
                );
                if (isAdvancedEditMode)
                {
                    materialEditor.ShaderProperty(
                        props[MToon10Prop.RimLightingMixFactor],
                        new GUIContent("Rim LightingMix")
                    );
                    EditorGUILayout.Space();

                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Matcap Rim", "Matcap Rim (RGB)"),
                        props[MToon10Prop.MatcapTexture]
                    );
                }
                EditorGUILayout.Space();

                materialEditor.ShaderProperty(
                    props[MToon10Prop.ParametricRimColorFactor],
                    new GUIContent("Parametric Rim Color")
                );
                materialEditor.ShaderProperty(
                    props[MToon10Prop.ParametricRimFresnelPowerFactor],
                    new GUIContent("Parametric Rim Fresnel Power")
                );
                materialEditor.ShaderProperty(
                    props[MToon10Prop.ParametricRimLiftFactor],
                    new GUIContent("Parametric Rim Lift")
                );
            }

            using (new LabelScope("Outline"))
            {
                PopupEnum<MToon10OutlineMode>("Outline Mode", props[MToon10Prop.OutlineWidthMode], materialEditor);
                var hasOutline = (MToon10OutlineMode)(int)props[MToon10Prop.OutlineWidthMode].floatValue != MToon10OutlineMode.None;

                if (hasOutline)
                {
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Outline Width", "Outline Width (G) [meter]"),
                        props[MToon10Prop.OutlineWidthMultiplyTexture],
                        props[MToon10Prop.OutlineWidthFactor]
                    );
                    materialEditor.ShaderProperty(
                        props[MToon10Prop.OutlineColorFactor],
                        new GUIContent("Outline Color")
                    );
                    if (isAdvancedEditMode)
                    {
                        materialEditor.ShaderProperty(
                            props[MToon10Prop.OutlineLightingMixFactor],
                            new GUIContent("Outline LightingMix")
                        );
                    }
                }
            }

            if (isAdvancedEditMode)
            {
                using (new LabelScope("UV Animation"))
                {
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Mask", "Mask (B)"),
                        props[MToon10Prop.UvAnimationMaskTexture]
                    );
                    materialEditor.ShaderProperty(
                        props[MToon10Prop.UvAnimationScrollXSpeedFactor],
                        new GUIContent("Translate X")
                    );
                    materialEditor.ShaderProperty(
                        props[MToon10Prop.UvAnimationScrollYSpeedFactor],
                        new GUIContent("Translate Y")
                    );
                    materialEditor.ShaderProperty(
                        props[MToon10Prop.UvAnimationRotationSpeedFactor],
                        new GUIContent("Rotation")
                    );
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Validate(materials);
            }

            if (isAdvancedEditMode && materials.Length == 1)
            {
                var mat = materials[0];
                using (new LabelScope("Debug"))
                {
                    EditorGUILayout.LabelField("RenderQueue", mat.renderQueue.ToString());
                    EditorGUILayout.LabelField("Cull", ((CullMode)props[MToon10Prop.UnityCullMode].floatValue).ToString());
                    EditorGUILayout.LabelField("SrcBlend", ((BlendMode)props[MToon10Prop.UnitySrcBlend].floatValue).ToString());
                    EditorGUILayout.LabelField("DstBlend", ((BlendMode)props[MToon10Prop.UnityDstBlend].floatValue).ToString());
                    EditorGUILayout.LabelField("ZWrite", ((UnityZWriteMode)props[MToon10Prop.UnityZWrite].floatValue).ToString());
                    EditorGUILayout.LabelField("AlphaToMask", ((UnityAlphaToMaskMode)props[MToon10Prop.UnityAlphaToMask].floatValue).ToString());
                    EditorGUILayout.LabelField("Enabled Keywords", string.Join("\n", mat.shaderKeywords), EditorStyles.textArea);
                }
            }

            // base.OnGUI(materialEditor, properties);
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
            var ret = EditorGUILayout.Popup(name, (int)property.floatValue, Enum.GetNames(typeof(T)));
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                editor.RegisterPropertyChangeUndo($"Change {name}");
                property.floatValue = ret;
            }

            EditorGUI.showMixedValue = false;
            return changed;
        }

    }
}