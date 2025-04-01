using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
    public class MToon10ShowcaseEntryPoint : MonoBehaviour
    {
        [SerializeField] private MToon10ShowcaseDependencies dependencies;

        [FormerlySerializedAs("renderingShowcase")] [SerializeField]
        private AlphaModeShowcase alphaModeShowcase;

        [SerializeField] private LitShowcase litShowcase;
        [SerializeField] private ShadeShowcase shadeShowcase;
        [SerializeField] private CameraScroller cameraScroller;

        private static readonly int Columns = 4;
        private static readonly float SphereRadius = 0.3f;
        private static readonly float SphereSpacing = 0.2f;
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

            cameraScroller.Initialize(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0.0f, 5.0f, -5.0f), 0.1f);
        }

        private Transform CreateShowcase<T>(T[] entries, Material baseMaterial, Action<T, MToon10Context> setup)
            where T : class
        {
            const int columnCount = 4;

            var root = new GameObject(typeof(T).Name);
            root.transform.position = _nextOrigin;

            var label = Instantiate(dependencies.labelTextPrefab, root.transform);
            label.GetComponent<TextMesh>().text = typeof(T).Name;
            const float LabelHeight = 0.4f;

            var gridOrigin = new Vector3(SphereRadius, 0.0f, -(LabelHeight + LabelSpacing + SphereRadius));
            var gridSpacing = SphereRadius * 2 + SphereSpacing;
            for (var idx = 0; idx < entries.Length; idx++)
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.SetParent(root.transform);
                obj.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                var x = idx % columnCount * gridSpacing + gridOrigin.x;
                var z = -idx / columnCount * gridSpacing + gridOrigin.z;
                obj.transform.localPosition = new Vector3(x, 0.0f, z);
                var sphereScale = SphereRadius * 2;
                obj.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
                var mat = new Material(baseMaterial);
                obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
                setup(entries[idx], new MToon10Context(mat));
            }

            var rows = entries.Length / columnCount + (entries.Length % columnCount == 0 ? 0 : 1);
            var showcaseHeight = LabelHeight + LabelSpacing + gridSpacing * rows;
            _nextOrigin -= new Vector3(0.0f, 0.0f, showcaseHeight);

            return root.transform;
        }
    }
}