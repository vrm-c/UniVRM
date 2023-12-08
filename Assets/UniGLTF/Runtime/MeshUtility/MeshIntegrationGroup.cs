using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrationGroup
    {
        public string Name;

        public enum MeshIntegrationTypes
        {
            Both,
            FirstPersonOnly,
            ThirdPersonOnly,
            Auto,
        }

        public MeshIntegrationTypes IntegrationType = default;
        public List<Renderer> Renderers = new List<Renderer>();

        public MeshIntegrationGroup CopyInstantiate(GameObject go, GameObject instance)
        {
            var copy = new MeshIntegrationGroup
            {
                Name = Name
            };
            foreach (var r in Renderers)
            {
                var relative = r.transform.RelativePathFrom(go.transform);
                if (r is SkinnedMeshRenderer smr)
                {
                    copy.Renderers.Add(instance.transform.GetFromPath(relative).GetComponent<SkinnedMeshRenderer>());
                }
                else if (r is MeshRenderer mr)
                {
                    copy.Renderers.Add(instance.transform.GetFromPath(relative).GetComponent<MeshRenderer>());
                }
            }
            return copy;
        }
    }
}