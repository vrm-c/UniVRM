using System;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
    public class MToon10ShowcaseEntryPoint : MonoBehaviour
    {
        [SerializeField] private CameraScroller cameraScroller;
        [SerializeField] private MToon10ShowcaseDependencies dependencies;

        [SerializeField] private AlphaModeShowcase alphaModeShowcase;
        [SerializeField] private RenderQueueOffsetShowcase renderQueueOffsetShowcase;
        [SerializeField] private LitShowcase litShowcase;
        [SerializeField] private ShadeShowcase shadeShowcase;
        [SerializeField] private NormalMapShowcase normalMapShowcase;
        [SerializeField] private ShadingToonyShowcase shadingToonyShowcase;
        [SerializeField] private ShadingShiftShowcase shadingShiftShowcase;
        [SerializeField] private GIEqualizationShowcase giEqualizationShowcase;
        [SerializeField] private EmissionShowcase emissionShowcase;
        [SerializeField] private RimLightingShowcase rimLightShowcase;
        [SerializeField] private MatcapShowcase matcapShowcase;
        [SerializeField] private ParametricRimShowcase parametricRimShowcase;
        [SerializeField] private OutlineShowcase outlineShowcase;
        [SerializeField] private UVAnimationShowcase uvAnimationShowcase;

        private static readonly int Columns = 4;
        private static readonly float PrimitiveRadius = 0.3f;
        private static readonly float PrimitiveSpacing = 0.2f;
        private static readonly float LabelSpacing = 0.1f;

        private Vector3 _nextOrigin = new(-2.0f, 1.0f, 2.0f);

        private void Start()
        {
            CreateShowcase(alphaModeShowcase.entries, alphaModeShowcase.baseMaterial, (entry, context) =>
            {
                context.AlphaMode = entry.alphaMode;
                context.TransparentWithZWriteMode = entry.transparentWithZWriteMode;
                context.AlphaCutoff = entry.alphaCutoff;
                context.DoubleSidedMode = entry.doubleSidedMode;
                context.Validate();
            });
            CreateShowcase(renderQueueOffsetShowcase.entries, renderQueueOffsetShowcase.baseMaterial, (entry, context) =>
            {
                context.AlphaMode = MToon10AlphaMode.Transparent;
                context.BaseColorFactorSrgb = entry.litColor;
                context.RenderQueueOffsetNumber = entry.renderQueueOffset;
            });
            CreateShowcase(litShowcase.entries, litShowcase.baseMaterial, (entry, context) =>
            {
                context.BaseColorFactorSrgb = entry.litColor;
                context.BaseColorTexture = entry.litTexture;
            });
            CreateShowcase(shadeShowcase.entries, shadeShowcase.baseMaterial, (entry, context) =>
            {
                context.ShadeColorFactorSrgb = entry.shadeColor;
                context.ShadeColorTexture = entry.shadeTexture;
            });
            CreateShowcase(normalMapShowcase.entries, normalMapShowcase.baseMaterial, (entry, context) =>
            {
                context.NormalTextureScale = entry.normalTextureScale;
                context.NormalTexture = entry.normalTexture;
            });
            CreateShowcase(shadingToonyShowcase.entries, shadingToonyShowcase.baseMaterial, (entry, context) =>
            {
                context.ShadingToonyFactor = entry.shadingToonyFactor;
                context.ShadingShiftFactor = entry.shadingShiftFactor;
            });
            CreateShowcase(shadingShiftShowcase.entries, shadingShiftShowcase.baseMaterial, (entry, context) =>
            {
                context.ShadingShiftTextureScale = entry.shadingShiftTextureScale;
                context.ShadingShiftTexture = entry.shadingShiftTexture;
            }, primitiveType: PrimitiveType.Quad);
            CreateShowcase(giEqualizationShowcase.entries, giEqualizationShowcase.baseMaterial, (entry, context) =>
            {
                context.GiEqualizationFactor = entry.giEqualizationFactor;
            });
            CreateShowcase(emissionShowcase.entries, emissionShowcase.baseMaterial, (entry, context) =>
            {
                context.EmissiveTexture = entry.emissiveTexture;
                context.EmissiveFactorLinear = entry.emissiveFactorLinear;
            });
            CreateShowcase(rimLightShowcase.entries, rimLightShowcase.baseMaterial, (entry, context) =>
            {
                context.RimMultiplyTexture = entry.rimMultiplyTexture;
                context.RimLightingMixFactor = entry.rimLightingMixFactor;
            });
            CreateShowcase(matcapShowcase.entries, matcapShowcase.baseMaterial, (entry, context) =>
            {
                context.MatcapTexture = entry.matcapTexture;
                context.MatcapColorFactorSrgb = entry.matcapColorFactor;
            });
            CreateShowcase(parametricRimShowcase.entries, parametricRimShowcase.baseMaterial, (entry, context) =>
            {
                context.ParametricRimColorFactorSrgb = entry.parametricRimColor;
                context.ParametricRimFresnelPowerFactor = entry.parametricRimFresnelPowerFactor;
                context.ParametricRimLiftFactor = entry.parametricRimLiftFactor;
            });
            CreateShowcase(outlineShowcase.entries, outlineShowcase.baseMaterial, (entry, context) =>
            {
                context.OutlineWidthMode = entry.outlineWidthMode;
                context.OutlineWidthFactor = entry.outlineWidthFactor;
                context.OutlineWidthMultiplyTexture = entry.outlineWidthMultiplyTexture;
                context.OutlineColorFactorSrgb = entry.outlineColorFactor;
                context.OutlineLightingMixFactor = entry.outlineLightingMixFactor;
            });
            CreateShowcase(uvAnimationShowcase.entries, uvAnimationShowcase.baseMaterial, (entry, context) =>
            {
                context.UvAnimationMaskTexture = entry.uvAnimationMaskTexture;
                context.UvAnimationScrollXSpeedFactor = entry.uvAnimationScrollXSpeedFactor;
                context.UvAnimationScrollYSpeedFactor = entry.uvAnimationScrollYSpeedFactor;
                context.UvAnimationRotationSpeedFactor = entry.uvAnimationRotationSpeedFactor;
            }, PrimitiveType.Quad);
            
            FillFloor();

            cameraScroller.Initialize(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0.0f, 5.0f, _nextOrigin.z), 0.5f);
        }

        private Transform CreateShowcase<T>(T[] entries, Material baseMaterial, Action<T, MToon10Context> setup, PrimitiveType primitiveType = PrimitiveType.Sphere)
            where T : class
        {
            const int columnCount = 4;

            var root = new GameObject(typeof(T).Name);
            root.transform.position = _nextOrigin;

            var label = Instantiate(dependencies.labelTextPrefab, root.transform);
            label.GetComponent<TextMesh>().text = typeof(T).Name;
            const float LabelHeight = 0.4f;

            var gridOrigin = new Vector3(PrimitiveRadius, 0.0f, -(LabelHeight + LabelSpacing + PrimitiveRadius));
            var gridSpacing = PrimitiveRadius * 2 + PrimitiveSpacing;
            for (var idx = 0; idx < entries.Length; idx++)
            {
                var obj = GameObject.CreatePrimitive(primitiveType);
                obj.transform.SetParent(root.transform);
                obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                var x = idx % columnCount * gridSpacing + gridOrigin.x;
                var z = -idx / columnCount * gridSpacing + gridOrigin.z;
                obj.transform.localPosition = new Vector3(x, 0.0f, z);
                var scale = PrimitiveRadius * 2;
                obj.transform.localScale = new Vector3(scale, scale, scale);
                var mat = new Material(baseMaterial);
                obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
                var context = new MToon10Context(mat);
                setup(entries[idx], context);
                context.Validate();
            }

            var rows = entries.Length / columnCount + (entries.Length % columnCount == 0 ? 0 : 1);
            var showcaseHeight = LabelHeight + LabelSpacing + gridSpacing * rows;
            _nextOrigin -= new Vector3(0.0f, 0.0f, showcaseHeight);

            return root.transform;
        }

        private void FillFloor()
        {
            var floors = new GameObject("Floors");
            var nextFloorOrigin = Vector3.zero;
            var floorLength = 10f * dependencies.floorPrefab.transform.localScale.y;
            var viewportOffset = Camera.main!.orthographicSize;
            while (nextFloorOrigin.z + floorLength / 2f > _nextOrigin.z - viewportOffset)
            {
                var floor = Instantiate(dependencies.floorPrefab, floors.transform);
                floor.transform.position = nextFloorOrigin;
                nextFloorOrigin -= new Vector3(0.0f, 0.0f, floorLength);
            }
        }
    }
}