using System;
using System.Collections.Generic;
using System.Linq;
using MeshUtility;
using UnityEngine;

namespace VRM
{
    public static class VRMFirstPersonValidator
    {
        public static Transform[] Hierarchy;
        static Action extended;

        public static bool IsValid(this VRMFirstPerson.RendererFirstPersonFlags r, string name, out Validation validation)
        {
            if (r.Renderer == null)
            {
                validation = Validation.Error($"{name}.Renderer is null", extended);
                return false;
            }

            if (!Hierarchy.Contains(r.Renderer.transform))
            {
                validation = Validation.Error($"{name}.Renderer is out of hierarchy", extended);
                return false;
            }

            if (!r.Renderer.EnableForExport())
            {
                validation = Validation.Error($"{name}.Renderer is not active", extended);
                return false;
            }

            validation = default;
            return true;
        }

        public static IEnumerable<Validation> Validate(this VRMFirstPerson self, GameObject _)
        {
            Hierarchy = self.GetComponentsInChildren<Transform>(true);

            extended = () =>
            {
                if (GUILayout.Button("reset VRMFirstPerson.Renderers"))
                {
                    self.TraverseRenderers();
                }
            };

            for (int i = 0; i < self.Renderers.Count; ++i)
            {
                if (!IsValid(self.Renderers[i], $"[VRMFirstPerson]{self.name}.Renderers[{i}]", out Validation v))
                {
                    yield return v;
                }
            }
        }
    }
}
