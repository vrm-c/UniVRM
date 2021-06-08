using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRMShaders.VRM10.MToon10.Editor
{
    public sealed class MToonInspector : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var props = PropExtensions.PropertyNames
                .ToDictionary(x => x.Key, x => FindProperty(x.Value, properties));
            var materials = materialEditor.targets.Select(x => x as Material).ToArray();

            EditorGUI.BeginChangeCheck();

            using (new LabelScope("Rendering"))
            {
                PopupEnum<AlphaMode>("Alpha Mode", props[Prop.AlphaMode], materialEditor);
                var alphaMode = (AlphaMode) (int) props[Prop.AlphaMode].floatValue;

                if (alphaMode == AlphaMode.Transparent)
                {
                    PopupEnum<TransparentWithZWriteMode>(
                        "Transparent With ZWrite Mode",
                        props[Prop.TransparentWithZWrite],
                        materialEditor
                    );
                }

                if (alphaMode == AlphaMode.Cutout)
                {
                    materialEditor.ShaderProperty(props[Prop.AlphaCutoff], "Cutoff");
                }

                PopupEnum<DoubleSidedMode>("Double Sided", props[Prop.DoubleSided], materialEditor);

                materialEditor.ShaderProperty(props[Prop.RenderQueueOffsetNumber], "RenderQueue Offset");
            }

            using (new LabelScope("Lighting"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Lit Color, Alpha", "Lit (RGB), Alpha (A)"),
                    props[Prop.BaseColorTexture],
                    props[Prop.BaseColorFactor]
                );
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Shade Color", "Shade (RGB)"),
                    props[Prop.ShadeColorTexture],
                    props[Prop.ShadeColorFactor]
                );
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Normal Map", "Normal Map (RGB)"),
                    props[Prop.NormalTexture],
                    props[Prop.NormalTextureScale]
                );
                materialEditor.ShaderProperty(props[Prop.ShadingShiftFactor], "Shading Shift");
                materialEditor.ShaderProperty(props[Prop.ShadingToonyFactor], "Shading Toony");
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Additive Shading Shift", "Shading Shift (R)"),
                    props[Prop.ShadingShiftTexture],
                    props[Prop.ShadingShiftTextureScale]
                );
            }

            using (new LabelScope("Global Illumination"))
            {
                materialEditor.ShaderProperty(props[Prop.GiEqualizationFactor], "GI Equalization");
            }

            using (new LabelScope("Emission"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Emission", "Emission (RGB)"),
                    props[Prop.EmissiveTexture],
                    props[Prop.EmissiveFactor]
                );
            }

            using (new LabelScope("Rim Lighting"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Rim Color", "Rim Color (RGB)"),
                    props[Prop.RimMultiplyTexture]
                );
                materialEditor.ShaderProperty(
                    props[Prop.RimLightingMixFactor],
                    new GUIContent("Rim LightingMix")
                );
                EditorGUILayout.Space();

                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Matcap Rim", "Matcap Rim (RGB)"),
                    props[Prop.MatcapTexture]
                );
                EditorGUILayout.Space();

                materialEditor.ShaderProperty(
                    props[Prop.ParametricRimColorFactor],
                    new GUIContent("Parametric Rim Color")
                );
                materialEditor.ShaderProperty(
                    props[Prop.ParametricRimFresnelPowerFactor],
                    new GUIContent("Parametric Rim Fresnel Power")
                );
                materialEditor.ShaderProperty(
                    props[Prop.ParametricRimLiftFactor],
                    new GUIContent("Parametric Rim Lift")
                );
            }

            using (new LabelScope("Outline"))
            {
                PopupEnum<OutlineMode>("Outline Mode", props[Prop.OutlineWidthMode], materialEditor);
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Outline Width", "Outline Width (G) [meter]"),
                    props[Prop.OutlineWidthMultiplyTexture],
                    props[Prop.OutlineWidthFactor]
                );
                materialEditor.ShaderProperty(
                    props[Prop.OutlineColorFactor],
                    new GUIContent("Outline Color")
                );
                materialEditor.ShaderProperty(
                    props[Prop.OutlineLightingMixFactor],
                    new GUIContent("Outline LightingMix")
                );
            }

            using (new LabelScope("UV Animation"))
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Mask", "Mask (B)"),
                    props[Prop.UvAnimationMaskTexture]
                );
                materialEditor.ShaderProperty(
                    props[Prop.UvAnimationScrollXSpeedFactor],
                    new GUIContent("Translate X")
                );
                materialEditor.ShaderProperty(
                    props[Prop.UvAnimationScrollYSpeedFactor],
                    new GUIContent("Translate Y")
                );
                materialEditor.ShaderProperty(
                    props[Prop.UvAnimationRotationSpeedFactor],
                    new GUIContent("Rotation")
                );
            }

            if (EditorGUI.EndChangeCheck())
            {
                Validate(materials);
            }

            using (new LabelScope("Debug"))
            {
                if (materials.Length == 1)
                {
                    var mat = materials[0];

                    EditorGUILayout.LabelField("RenderQueue", mat.renderQueue.ToString());
                    EditorGUILayout.LabelField("Cull", ((CullMode) props[Prop.UnityCullMode].floatValue).ToString());
                    EditorGUILayout.LabelField("SrcBlend", ((BlendMode) props[Prop.UnitySrcBlend].floatValue).ToString());
                    EditorGUILayout.LabelField("DstBlend", ((BlendMode) props[Prop.UnityDstBlend].floatValue).ToString());
                    EditorGUILayout.LabelField("ZWrite", ((UnityZWriteMode) props[Prop.UnityZWrite].floatValue).ToString());
                    EditorGUILayout.LabelField("AlphaToMask", ((UnityAlphaToMaskMode) props[Prop.UnityAlphaToMask].floatValue).ToString());
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