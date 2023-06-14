using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VrmAnimation : IMotion
    {
        private readonly VrmAnimationInstance m_instance;

        (INormalizedPoseProvider, ITPoseProvider) IMotion.ControlRig => m_instance.ControlRig;
        public IDictionary<ExpressionKey, Transform> ExpressionMap { get; } = new Dictionary<ExpressionKey, Transform>();

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
            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new VrmAnimationImporter(data))
            {
                loader.InvertAxis = Axes.X;
                var instance = await loader.LoadAsync(new ImmediateCaller());

                var animationInstance = instance.gameObject.AddComponent<UniVRM10.VrmAnimationInstance>();
                animationInstance.Initialize();

                var animation = new VrmAnimation(animationInstance);
                foreach (var (preset, transform) in loader.GetExpressions())
                {
                    animation.ExpressionMap.Add(preset, transform);
                }
                return animation;
            }
        }
    }
}
