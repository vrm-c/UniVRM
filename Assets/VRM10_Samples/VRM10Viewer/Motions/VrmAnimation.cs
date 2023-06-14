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
        private RuntimeGltfInstance m_instance;
        public SkinnedMeshRenderer m_boxMan;
        public SkinnedMeshRenderer BoxMan => m_boxMan;
        private (INormalizedPoseProvider, ITPoseProvider) m_controlRig;
        (INormalizedPoseProvider, ITPoseProvider) IMotion.ControlRig => m_controlRig;
        public IDictionary<ExpressionKey, Transform> ExpressionMap { get; } = new Dictionary<ExpressionKey, Transform>();

        public VrmAnimation(RuntimeGltfInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }

            var humanoid = instance.gameObject.AddComponent<Humanoid>();
            if (humanoid.AssignBonesFromAnimator())
            {
                var provider = new InitRotationPoseProvider(instance.transform, humanoid);
                m_controlRig = (provider, provider);

                // create SkinnedMesh for bone visualize
                var animator = instance.GetComponent<Animator>();
                m_boxMan = SkeletonMeshUtility.CreateRenderer(animator);
                var material = new Material(Shader.Find("Standard"));
                BoxMan.sharedMaterial = material;
                var mesh = BoxMan.sharedMesh;
                mesh.name = "box-man";
            }
        }

        public void ShowBoxMan(bool enable)
        {
            if (m_boxMan != null)
            {
                m_boxMan.enabled = enable;
            }
        }

        public void Dispose()
        {
            if (m_boxMan != null)
            {
                GameObject.Destroy(m_boxMan.gameObject);
            }
        }

        public static async Task<VrmAnimation> LoadVrmAnimationFromPathAsync(string path)
        {
            using (GltfData data = new AutoGltfFileParser(path).Parse())
            using (var loader = new VrmAnimationImporter(data))
            {
                loader.InvertAxis = Axes.X;
                var instance = await loader.LoadAsync(new ImmediateCaller());

                var animation = new VrmAnimation(instance);
                foreach (var (preset, transform) in loader.GetExpressions())
                {
                    animation.ExpressionMap.Add(preset, transform);
                }
                return animation;
            }
        }
    }
}
