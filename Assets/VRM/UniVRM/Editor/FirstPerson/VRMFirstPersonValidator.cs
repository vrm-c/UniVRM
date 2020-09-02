using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRM
{
    public static class VRMFirstPersonValidator
    {
        public static IEnumerable<Validation> Validate(this VRMFirstPerson self)
        {
            var hierarchy = self.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < self.Renderers.Count; ++i)
            {
                var r = self.Renderers[i];
                if (r.Renderer == null)
                {
                    yield return Validation.Error($"[VRMFirstPerson]{self.name}.Renderers[{i}].Renderer is null");
                }
                if (!hierarchy.Contains(r.Renderer.transform))
                {
                    yield return Validation.Error($"[VRMFirstPerson]{self.name}.Renderers[{i}].Renderer is out of hierarchy");
                }
                if (!r.Renderer.EnableForExport())
                {
                    yield return Validation.Error($"[VRMFirstPerson]{self.name}.Renderers[{i}].Renderer is not active");
                }
            }
            yield break;
        }
    }
}
