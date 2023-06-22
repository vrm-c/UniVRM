using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VrmAnimation : IMotion
    {
        private readonly VrmAnimationInstance m_instance;

        (INormalizedPoseProvider, ITPoseProvider) IMotion.ControlRig => m_instance.ControlRig;
        IDictionary<ExpressionKey, Transform> IMotion.ExpressionMap => m_instance.ExpressionMap;

        public VrmAnimation(VrmAnimationInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }
        }

        public void ShowBoxMan(bool enable)
        {
            if (m_instance.BoxMan != null)
            {
                m_instance.BoxMan.enabled = enable;
            }
        }

        public void Dispose()
        {
            if (m_instance.BoxMan != null)
            {
                GameObject.Destroy(m_instance.BoxMan.gameObject);
            }
        }

        public static async Task<VrmAnimation> LoadVrmAnimationFromPathAsync(string path)
        {
            using GltfData data = new AutoGltfFileParser(path).Parse();
            using var loader = new VrmAnimationImporter(data);
            var instance = await loader.LoadAsync(new ImmediateCaller());
            return new VrmAnimation(instance.GetComponent<VrmAnimationInstance>());
        }
    }
}
