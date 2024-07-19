using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MToon
{
    public class MToonInspector : ShaderGUI
    {
        private const float RoundsToDegree = 360f;
        private const float RoundsToRadian = (float) Math.PI * 2f;

        private static bool isAdvancedLightingPanelFoldout = false;
        private static EditorRotationUnit editorRotationUnit = EditorRotationUnit.Rounds;
        
        private MaterialProperty _version;
        private MaterialProperty _blendMode;
        private MaterialProperty _bumpMap;
        private MaterialProperty _bumpScale;
        private MaterialProperty _color;
        private MaterialProperty _cullMode;
//        private MaterialProperty _outlineCullMode;
        private MaterialProperty _cutoff;

        private MaterialProperty _debugMode;
        private MaterialProperty _emissionColor;
        private MaterialProperty _emissionMap;
        private MaterialProperty _lightColorAttenuation;
        private MaterialProperty _indirectLightIntensity;
        private MaterialProperty _mainTex;
        private MaterialProperty _outlineColor;
        private MaterialProperty _outlineColorMode;
        private MaterialProperty _outlineLightingMix;
        private MaterialProperty _outlineWidth;
        private MaterialProperty _outlineScaledMaxDistance;
        private MaterialProperty _outlineWidthMode;
        private MaterialProperty _outlineWidthTexture;
        private MaterialProperty _receiveShadowRate;
        private MaterialProperty _receiveShadowTexture;
        private MaterialProperty _shadingGradeRate;
        private MaterialProperty _shadingGradeTexture;
        private MaterialProperty _shadeColor;
        private MaterialProperty _shadeShift;
        private MaterialProperty _shadeTexture;
        private MaterialProperty _shadeToony;
        private MaterialProperty _sphereAdd;
        private MaterialProperty _rimColor;
        private MaterialProperty _rimTexture;
        private MaterialProperty _rimLightingMix;
        private MaterialProperty _rimFresnelPower;
        private MaterialProperty _rimLift;
        private MaterialProperty _uvAnimMaskTexture;
        private MaterialProperty _uvAnimScrollX;
        private MaterialProperty _uvAnimScrollY;
        private MaterialProperty _uvAnimRotation;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _version = FindProperty(Utils.PropVersion, properties);
            _debugMode = FindProperty(Utils.PropDebugMode, properties);
            _outlineWidthMode = FindProperty(Utils.PropOutlineWidthMode, properties);
            _outlineColorMode = FindProperty(Utils.PropOutlineColorMode, properties);
            _blendMode = FindProperty(Utils.PropBlendMode, properties);
            _cullMode = FindProperty(Utils.PropCullMode, properties);
//            _outlineCullMode = FindProperty(Utils.PropOutlineCullMode, properties);
            _cutoff = FindProperty(Utils.PropCutoff, properties);
            _color = FindProperty(Utils.PropColor, properties);
            _shadeColor = FindProperty(Utils.PropShadeColor, properties);
            _mainTex = FindProperty(Utils.PropMainTex, properties);
            _shadeTexture = FindProperty(Utils.PropShadeTexture, properties);
            _bumpScale = FindProperty(Utils.PropBumpScale, properties);
            _bumpMap = FindProperty(Utils.PropBumpMap, properties);
            _receiveShadowRate = FindProperty(Utils.PropReceiveShadowRate, properties);
            _receiveShadowTexture = FindProperty(Utils.PropReceiveShadowTexture, properties);
            _shadingGradeRate = FindProperty(Utils.PropShadingGradeRate, properties);
            _shadingGradeTexture = FindProperty(Utils.PropShadingGradeTexture, properties);
            _shadeShift = FindProperty(Utils.PropShadeShift, properties);
            _shadeToony = FindProperty(Utils.PropShadeToony, properties);
            _lightColorAttenuation = FindProperty(Utils.PropLightColorAttenuation, properties);
            _indirectLightIntensity = FindProperty(Utils.PropIndirectLightIntensity, properties);
            _rimColor = FindProperty(Utils.PropRimColor, properties);
            _rimTexture = FindProperty(Utils.PropRimTexture, properties);
            _rimLightingMix = FindProperty(Utils.PropRimLightingMix, properties);
            _rimFresnelPower = FindProperty(Utils.PropRimFresnelPower, properties);
            _rimLift = FindProperty(Utils.PropRimLift, properties);
            _sphereAdd = FindProperty(Utils.PropSphereAdd, properties);
            _emissionColor = FindProperty(Utils.PropEmissionColor, properties);
            _emissionMap = FindProperty(Utils.PropEmissionMap, properties);
            _outlineWidthTexture = FindProperty(Utils.PropOutlineWidthTexture, properties);
            _outlineWidth = FindProperty(Utils.PropOutlineWidth, properties);
            _outlineScaledMaxDistance = FindProperty(Utils.PropOutlineScaledMaxDistance, properties);
            _outlineColor = FindProperty(Utils.PropOutlineColor, properties);
            _outlineLightingMix = FindProperty(Utils.PropOutlineLightingMix, properties);
            _uvAnimMaskTexture = FindProperty(Utils.PropUvAnimMaskTexture, properties);
            _uvAnimScrollX = FindProperty(Utils.PropUvAnimScrollX, properties);
            _uvAnimScrollY = FindProperty(Utils.PropUvAnimScrollY, properties);
            _uvAnimRotation = FindProperty(Utils.PropUvAnimRotation, properties);
            var materials = materialEditor.targets.Select(x => x as Material).ToArray();
            Draw(materialEditor, materials);
        }

        private void Draw(MaterialEditor materialEditor, Material[] materials)
        {
            EditorGUI.BeginChangeCheck();
            {
                _version.floatValue = Utils.VersionNumber;
                
                EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
                    if (PopupEnum<RenderMode>("Rendering Type", _blendMode, materialEditor))
                    {
                        ModeChanged(materials, isBlendModeChangedByUser: true);
                    }

                    if ((RenderMode) _blendMode.floatValue == RenderMode.TransparentWithZWrite)
                    {
                        EditorGUILayout.HelpBox("TransparentWithZWrite mode can cause problems with rendering.", MessageType.Warning);
                    }

                    if (PopupEnum<CullMode>("Cull Mode", _cullMode, materialEditor))
                    {
                        ModeChanged(materials);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Texture", EditorStyles.boldLabel);
                    {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Lit Color, Alpha", "Lit (RGB), Alpha (A)"),
                            _mainTex, _color);

                        materialEditor.TexturePropertySingleLine(new GUIContent("Shade Color", "Shade (RGB)"), _shadeTexture,
                            _shadeColor);
                    }
                    var bm = (RenderMode) _blendMode.floatValue;
                    if (bm == RenderMode.Cutout)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Alpha", EditorStyles.boldLabel);
                        {
                            materialEditor.ShaderProperty(_cutoff, "Cutoff");
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    {
                        materialEditor.ShaderProperty(_shadeToony,
                            new GUIContent("Shading Toony",
                                "0.0 is Lambert. Higher value get toony shading."));

                        // Normal
                        EditorGUI.BeginChangeCheck();
                        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map [Normal]", "Normal Map (RGB)"),
                            _bumpMap,
                            _bumpScale);
                        if (EditorGUI.EndChangeCheck())
                        {
                            materialEditor.RegisterPropertyChangeUndo("BumpEnabledDisabled");
                            ModeChanged(materials);
                        }
                    }
                    EditorGUILayout.Space();

                    EditorGUI.indentLevel++;
                    {
                        isAdvancedLightingPanelFoldout = EditorGUILayout.Foldout(isAdvancedLightingPanelFoldout, "Advanced Settings", EditorStyles.boldFont);

                        if (isAdvancedLightingPanelFoldout)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox(
                                "The default settings are suitable for Advanced Settings if you want to toony result.",
                                MessageType.Info);
                            if (GUILayout.Button("Use Default"))
                            {
                                _shadeShift.floatValue = 0;
                                _receiveShadowTexture.textureValue = null;
                                _receiveShadowRate.floatValue = 1;
                                _shadingGradeTexture.textureValue = null;
                                _shadingGradeRate.floatValue = 1;
                                _lightColorAttenuation.floatValue = 0;
                                _indirectLightIntensity.floatValue = 0.1f;
                            }
                            EditorGUILayout.EndHorizontal();
                            
                            materialEditor.ShaderProperty(_shadeShift,
                                new GUIContent("Shading Shift",
                                    "Zero is Default. Negative value increase lit area. Positive value increase shade area."));
                            materialEditor.TexturePropertySingleLine(
                                new GUIContent("Shadow Receive Multiplier",
                                    "Texture (R) * Rate. White is Default. Black attenuates shadows."),
                                _receiveShadowTexture,
                                _receiveShadowRate);
                            materialEditor.TexturePropertySingleLine(
                                new GUIContent("Lit & Shade Mixing Multiplier",
                                    "Texture (R) * Rate. Compatible with UTS2 ShadingGradeMap. White is Default. Black amplifies shade."),
                                _shadingGradeTexture,
                                _shadingGradeRate);
                            materialEditor.ShaderProperty(_lightColorAttenuation, "LightColor Attenuation");
                            materialEditor.ShaderProperty(_indirectLightIntensity, "GI Intensity");
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    TextureWithHdrColor(materialEditor, "Emission", "Emission (RGB)",
                        _emissionMap, _emissionColor);
                    
                    materialEditor.TexturePropertySingleLine(new GUIContent("MatCap", "MatCap Texture (RGB)"),
                        _sphereAdd);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                    
                EditorGUILayout.LabelField("Rim", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    TextureWithHdrColor(materialEditor, "Color", "Rim Color (RGB)",
                        _rimTexture, _rimColor);
                    
                    materialEditor.DefaultShaderProperty(_rimLightingMix, "Lighting Mix");

                    materialEditor.ShaderProperty(_rimFresnelPower,
                        new GUIContent("Fresnel Power",
                            "If you increase this value, you get sharpness rim light."));

                    materialEditor.ShaderProperty(_rimLift,
                        new GUIContent("Lift",
                            "If you increase this value, you can lift rim light."));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();


                EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    // Outline
                    EditorGUILayout.LabelField("Width", EditorStyles.boldLabel);
                    {
                        if (PopupEnum<OutlineWidthMode>("Mode", _outlineWidthMode, materialEditor))
                        {
                            ModeChanged(materials);
                        }
                        
                        if ((RenderMode) _blendMode.floatValue == RenderMode.Transparent &&
                            (OutlineWidthMode) _outlineWidthMode.floatValue != OutlineWidthMode.None)
                        {
                            EditorGUILayout.HelpBox("Outline with Transparent material cause problem with rendering.", MessageType.Warning);
                        }

                        var widthMode = (OutlineWidthMode) _outlineWidthMode.floatValue;
                        if (widthMode != OutlineWidthMode.None)
                        {
                            materialEditor.TexturePropertySingleLine(
                                new GUIContent("Width", "Outline Width Texture (RGB)"),
                                _outlineWidthTexture, _outlineWidth);
                        }

                        if (widthMode == OutlineWidthMode.ScreenCoordinates)
                        {
                            materialEditor.ShaderProperty(_outlineScaledMaxDistance, "Width Scaled Max Distance");
                        }
                    }
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
                    {
                        var widthMode = (OutlineWidthMode) _outlineWidthMode.floatValue;
                        if (widthMode != OutlineWidthMode.None)
                        {
                            EditorGUI.BeginChangeCheck();

                            if (PopupEnum<OutlineColorMode>("Mode", _outlineColorMode, materialEditor))
                            {
                                ModeChanged(materials);
                            }

                            var colorMode = (OutlineColorMode) _outlineColorMode.floatValue;

                            materialEditor.ShaderProperty(_outlineColor, "Color");
                            if (colorMode == OutlineColorMode.MixedLighting)
                                materialEditor.DefaultShaderProperty(_outlineLightingMix, "Lighting Mix");
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                
                EditorGUILayout.LabelField("UV Coordinates", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    // UV
                    EditorGUILayout.LabelField("Scale & Offset", EditorStyles.boldLabel);
                    {
                        materialEditor.TextureScaleOffsetProperty(_mainTex);
                    }
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Auto Animation", EditorStyles.boldLabel);
                    {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Mask", "Auto Animation Mask Texture (R)"), _uvAnimMaskTexture);
                        materialEditor.ShaderProperty(_uvAnimScrollX, "Scroll X (per second)");
                        materialEditor.ShaderProperty(_uvAnimScrollY, "Scroll Y (per second)");

                        {
                            var control = EditorGUILayout.GetControlRect(hasLabel: true);
                            const int popupMargin = 5;
                            const int popupWidth = 80;

                            var floatControl = new Rect(control);
                            floatControl.width -= popupMargin + popupWidth;
                            var popupControl = new Rect(control);
                            popupControl.x = floatControl.x + floatControl.width + popupMargin;
                            popupControl.width = popupWidth;
                            
                            EditorGUI.BeginChangeCheck();
                            var inspectorRotationValue = GetInspectorRotationValue(editorRotationUnit, _uvAnimRotation.floatValue);
                            inspectorRotationValue = EditorGUI.FloatField(floatControl, "Rotation value (per second)", inspectorRotationValue);
                            if (EditorGUI.EndChangeCheck())
                            {
                                materialEditor.RegisterPropertyChangeUndo("UvAnimRotationValueChanged");
                                _uvAnimRotation.floatValue = GetRawRotationValue(editorRotationUnit, inspectorRotationValue);
                            }
                            editorRotationUnit = (EditorRotationUnit) EditorGUI.EnumPopup(popupControl, editorRotationUnit);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                
                
                EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Debugging Options", EditorStyles.boldLabel);
                    {
                        if (PopupEnum<DebugMode>("Visualize", _debugMode, materialEditor))
                        {
                            ModeChanged(materials);
                        }
                    }
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
                    {
#if UNITY_5_6_OR_NEWER
//                    materialEditor.EnableInstancingField();
                        materialEditor.DoubleSidedGIField();
#endif
                        EditorGUI.BeginChangeCheck();
                        materialEditor.RenderQueueField();
                        if (EditorGUI.EndChangeCheck())
                        {
                            ModeChanged(materials);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUI.EndChangeCheck();
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            Utils.ValidateProperties(material, isBlendModeChangedByUser: true);
        }

        private static void ModeChanged(Material[] materials, bool isBlendModeChangedByUser = false)
        {
            foreach (var material in materials)
            {
                Utils.ValidateProperties(material, isBlendModeChangedByUser);
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

        private static void TextureWithHdrColor(MaterialEditor materialEditor, string label, string description,
            MaterialProperty texProp, MaterialProperty colorProp)
        {
            materialEditor.TexturePropertyWithHDRColor(new GUIContent(label, description),
                texProp,
                colorProp,
#if UNITY_2018_1_OR_NEWER
#else
                new ColorPickerHDRConfig(minBrightness: 0, maxBrightness: 10, minExposureValue: -10,
                    maxExposureValue: 10),
#endif
                showAlpha: false);
            
        }

        private static float GetRawRotationValue(EditorRotationUnit unit, float inspectorValue)
        {
            switch (unit)
            {
                case EditorRotationUnit.Rounds:
                    return inspectorValue;
                case EditorRotationUnit.Degrees:
                    return inspectorValue / RoundsToDegree;
                case EditorRotationUnit.Radians:
                    return inspectorValue / RoundsToRadian;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static float GetInspectorRotationValue(EditorRotationUnit unit, float rawValue)
        {
            switch (unit)
            {
                case EditorRotationUnit.Rounds:
                    return rawValue;
                case EditorRotationUnit.Degrees:
                    return rawValue * RoundsToDegree;
                case EditorRotationUnit.Radians:
                    return rawValue * RoundsToRadian;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}