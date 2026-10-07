using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// glTF 準拠 PBR ShaderGraph (UniGltfPbr) 用の Custom ShaderGUI。
    ///
    /// - 見た目は URP/Lit に近づけ、CoreEditorUtils.DrawHeaderFoldout でセクション分けする。
    /// - 不透明/透明・Alpha Clip・Render Face の切替は副作用 (blend/zwrite/renderQueue/keyword) を伴うため、
    ///   生プロパティ (_Surface / _Blend / _SrcBlend ...) を直接露出せず、
    ///   <see cref="UniGltfPbrContext"/> の SurfaceType / IsAlphaClipEnabled / CullMode + Validate() に
    ///   委譲して自動適用・隠蔽する。
    ///
    /// アタッチ: UniGltfPbr.shadergraph の各 Target の Custom Editor GUI に "UniGLTF.UniGltfPbrShaderGUI" を設定。
    /// </summary>
    public class UniGltfPbrShaderGUI : ShaderGUI
    {
        private static class Styles
        {
            public static readonly GUIContent SurfaceType = new GUIContent("Surface Type", "Opaque / Transparent の切替。関連する Blend / ZWrite / RenderQueue は自動設定されます。");
            public static readonly GUIContent RenderFace = new GUIContent("Render Face", "描画する面。Front(片面 / doubleSided=false) または Both(doubleSided=true)。");
            public static readonly GUIContent AlphaClip = new GUIContent("Alpha Clipping", "Alpha が閾値未満のピクセルを破棄します (glTF MASK)。");
            public static readonly GUIContent Threshold = new GUIContent("Threshold", "Alpha Clipping の閾値 (alphaCutoff)。");

            public static readonly GUIContent BaseMap = new GUIContent("Base Map", "baseColorTexture (sRGB) と baseColorFactor。");
            public static readonly GUIContent Metallic = new GUIContent("Metallic", "metallicFactor。");
            public static readonly GUIContent Roughness = new GUIContent("Roughness", "roughnessFactor。");
            public static readonly GUIContent MetallicRoughnessMap = new GUIContent("Metallic Roughness Map", "metallicRoughnessTexture (Linear, G:Roughness B:Metallic)。");
            public static readonly GUIContent OcclusionMap = new GUIContent("Occlusion Map", "occlusionTexture (Linear, R:Occlusion)。");
            public static readonly GUIContent OcclusionStrength = new GUIContent("Occlusion Strength", "occlusionTexture.strength。");
            public static readonly GUIContent NormalMap = new GUIContent("Normal Map", "normalTexture と normalScale。");
            public static readonly GUIContent Emission = new GUIContent("Emission", "emissiveFactor (HDR) と emissiveTexture (sRGB)。");
            public static readonly GUIContent EmissiveStrength = new GUIContent("Emissive Strength", "KHR_materials_emissive_strength。");
        }

        // Render Face popup。ラベルは「描画する面」、CullMode は「カリングする面」で逆になる点に注意:
        //   Front を描画 = 背面カリング = CullMode.Back (glTF: doubleSided=false)
        //   Both        = CullMode.Off        (glTF: doubleSided=true)
        // 裏面のみ描画 (CullMode.Front) は glTF 非対応のため選択肢に用意しない。
        private static readonly string[] RenderFaceNames = { "Front", "Both" };
        private static readonly CullMode[] RenderFaceCull = { CullMode.Back, CullMode.Off };

        // foldout state (editor セッション保持)
        private static bool s_surfaceOptions = true;
        private static bool s_surfaceInputs = true;
        private static bool s_advanced = false;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var baseColorFactor = FindProperty("_baseColorFactor", properties, false);
            var baseColorTexture = FindProperty("_baseColorTexture", properties, false);
            var metallicFactor = FindProperty("_metallicFactor", properties, false);
            var roughnessFactor = FindProperty("_roughnessFactor", properties, false);
            var metallicRoughnessTexture = FindProperty("_metallicRoughnessTexture", properties, false);
            var occlusionTexture = FindProperty("_occlusionTexture", properties, false);
            var occlusionStrength = FindProperty("_occlusionStrength", properties, false);
            var normalTexture = FindProperty("_normalTexture", properties, false);
            var normalScale = FindProperty("_normalScale", properties, false);
            var emissiveFactor = FindProperty("_emissiveFactor", properties, false);
            var emissiveTexture = FindProperty("_emissiveTexture", properties, false);
            var emissiveStrength = FindProperty("_emissiveStrength", properties, false);
            var alphaCutoff = FindProperty("_alphaCutoff", properties, false);

            s_surfaceOptions = CoreEditorUtils.DrawHeaderFoldout("Surface Options", s_surfaceOptions);
            if (s_surfaceOptions)
            {
                EditorGUI.indentLevel++;
                DrawSurfaceOptions(materialEditor, alphaCutoff);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            s_surfaceInputs = CoreEditorUtils.DrawHeaderFoldout("Surface Inputs", s_surfaceInputs);
            if (s_surfaceInputs)
            {
                EditorGUI.indentLevel++;
                DrawSurfaceInputs(
                    materialEditor,
                    baseColorTexture, baseColorFactor,
                    metallicFactor, roughnessFactor, metallicRoughnessTexture,
                    occlusionTexture, occlusionStrength,
                    normalTexture, normalScale,
                    emissiveTexture, emissiveFactor, emissiveStrength);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            s_advanced = CoreEditorUtils.DrawHeaderFoldout("Advanced Options", s_advanced);
            if (s_advanced)
            {
                EditorGUI.indentLevel++;
                DrawAdvanced(materialEditor);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawSurfaceOptions(MaterialEditor materialEditor, MaterialProperty alphaCutoff)
        {
            var materials = materialEditor.targets.OfType<Material>().ToArray();
            if (materials.Length == 0) return;

            // Surface Type (Opaque / Transparent)
            {
                var current = new UniGltfPbrContext(materials[0]).SurfaceType;
                EditorGUI.showMixedValue = materials.Any(m => new UniGltfPbrContext(m).SurfaceType != current);
                EditorGUI.BeginChangeCheck();
                var next = (UniGltfPbrSurfaceType)EditorGUILayout.EnumPopup(Styles.SurfaceType, current);
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Surface Type");
                    foreach (var m in materials)
                    {
                        var context = new UniGltfPbrContext(m) { SurfaceType = next };
                        context.Validate();
                    }
                }
                EditorGUI.showMixedValue = false;
            }

            // Render Face (cull)
            {
                var currentCull = new UniGltfPbrContext(materials[0]).CullMode;
                var currentIndex = Mathf.Max(0, System.Array.IndexOf(RenderFaceCull, currentCull));
                EditorGUI.showMixedValue = materials.Any(m => new UniGltfPbrContext(m).CullMode != currentCull);
                EditorGUI.BeginChangeCheck();
                var nextIndex = EditorGUILayout.Popup(Styles.RenderFace, currentIndex, RenderFaceNames);
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Render Face");
                    foreach (var m in materials)
                    {
                        var context = new UniGltfPbrContext(m) { CullMode = RenderFaceCull[nextIndex] };
                        context.Validate();
                    }
                }
                EditorGUI.showMixedValue = false;
            }

            // Alpha Clipping
            var alphaClipEnabled = new UniGltfPbrContext(materials[0]).IsAlphaClipEnabled;
            {
                EditorGUI.showMixedValue = materials.Any(m => new UniGltfPbrContext(m).IsAlphaClipEnabled != alphaClipEnabled);
                EditorGUI.BeginChangeCheck();
                var next = EditorGUILayout.Toggle(Styles.AlphaClip, alphaClipEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Alpha Clipping");
                    foreach (var m in materials)
                    {
                        var context = new UniGltfPbrContext(m) { IsAlphaClipEnabled = next };
                        context.Validate();
                    }
                    alphaClipEnabled = next;
                }
                EditorGUI.showMixedValue = false;
            }

            // Threshold は Alpha Clipping 有効時のみ表示 (副作用のない単純プロパティ)
            if (alphaClipEnabled && alphaCutoff != null)
            {
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(alphaCutoff, Styles.Threshold);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawSurfaceInputs(
            MaterialEditor materialEditor,
            MaterialProperty baseColorTexture, MaterialProperty baseColorFactor,
            MaterialProperty metallicFactor, MaterialProperty roughnessFactor, MaterialProperty metallicRoughnessTexture,
            MaterialProperty occlusionTexture, MaterialProperty occlusionStrength,
            MaterialProperty normalTexture, MaterialProperty normalScale,
            MaterialProperty emissiveTexture, MaterialProperty emissiveFactor, MaterialProperty emissiveStrength)
        {
            // NOTE: シェーダは全テクスチャに Tiling/Offset (ScaleOffset) を持つため、
            //       テクスチャ割当時に各マップの TextureScaleOffsetProperty を表示する。
            if (baseColorTexture != null && baseColorFactor != null)
            {
                materialEditor.TexturePropertySingleLine(Styles.BaseMap, baseColorTexture, baseColorFactor);
                DrawScaleOffset(materialEditor, baseColorTexture);
            }

            if (metallicFactor != null) materialEditor.ShaderProperty(metallicFactor, Styles.Metallic);
            if (roughnessFactor != null) materialEditor.ShaderProperty(roughnessFactor, Styles.Roughness);
            if (metallicRoughnessTexture != null)
            {
                materialEditor.TexturePropertySingleLine(Styles.MetallicRoughnessMap, metallicRoughnessTexture);
                DrawScaleOffset(materialEditor, metallicRoughnessTexture);
            }

            if (occlusionTexture != null)
            {
                var occExtra = occlusionTexture.textureValue != null ? occlusionStrength : null;
                materialEditor.TexturePropertySingleLine(Styles.OcclusionMap, occlusionTexture, occExtra);
                DrawScaleOffset(materialEditor, occlusionTexture);
            }

            if (normalTexture != null)
            {
                var normalExtra = normalTexture.textureValue != null ? normalScale : null;
                materialEditor.TexturePropertySingleLine(Styles.NormalMap, normalTexture, normalExtra);
                DrawScaleOffset(materialEditor, normalTexture);
            }

            if (emissiveTexture != null && emissiveFactor != null)
            {
                materialEditor.TexturePropertyWithHDRColor(Styles.Emission, emissiveTexture, emissiveFactor, false);
                if (emissiveStrength != null)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(emissiveStrength, Styles.EmissiveStrength);
                    EditorGUI.indentLevel--;
                }
                DrawScaleOffset(materialEditor, emissiveTexture);
            }
        }

        /// <summary>
        /// テクスチャが割り当てられている時のみ Tiling/Offset を表示する。
        /// </summary>
        private static void DrawScaleOffset(MaterialEditor materialEditor, MaterialProperty textureProperty)
        {
            if (textureProperty.textureValue == null) return;
            materialEditor.TextureScaleOffsetProperty(textureProperty);
            GUILayout.Space(8);
        }

        private static void DrawAdvanced(MaterialEditor materialEditor)
        {
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
            materialEditor.RenderQueueField();
        }
    }
}
