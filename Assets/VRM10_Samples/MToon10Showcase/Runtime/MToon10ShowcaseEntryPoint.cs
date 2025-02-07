using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
    public class MToon10ShowcaseEntryPoint : MonoBehaviour
    {
        [SerializeField] private MToon10ShowcaseDependencies dependencies;
        [SerializeField] private LitShowcase litShowcase;
        [SerializeField] private ShadeShowcase shadeShowcase;

        private void Start()
        {
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
        }

        private Transform CreateShowcase<T>(T[] entries, Material baseMaterial, Action<T, MToon10Context> setup) where T : class
        {
            const int columnCount = 4;

            var root = new GameObject(typeof(T).Name);
            for (var idx = 0; idx < entries.Length; idx++)
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.SetParent(root.transform);
                obj.transform.localPosition = new Vector3(idx % columnCount, 0.5f, idx / columnCount);
                obj.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                var mat = new Material(baseMaterial);
                obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
                setup(entries[idx], new MToon10Context(mat));
            }

            return root.transform;
        }
    }
}