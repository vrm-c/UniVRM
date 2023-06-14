using System.Collections.Generic;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    public class VrmAnimationInstance : MonoBehaviour
    {
        public SkinnedMeshRenderer BoxMan;
        public (INormalizedPoseProvider, ITPoseProvider) ControlRig;
        public Dictionary<ExpressionKey, Transform> ExpressionMap = new Dictionary<ExpressionKey, Transform>();

        public void Initialize(IReadOnlyList<(ExpressionKey, Transform)> expressions)
        {
            var humanoid = gameObject.AddComponent<Humanoid>();
            if (humanoid.AssignBonesFromAnimator())
            {
                // require: transform is T-Pose 
                var provider = new InitRotationPoseProvider(transform, humanoid);
                ControlRig = (provider, provider);

                // create SkinnedMesh for bone visualize
                var animator = GetComponent<Animator>();
                BoxMan = SkeletonMeshUtility.CreateRenderer(animator);
                var material = new Material(Shader.Find("Standard"));
                BoxMan.sharedMaterial = material;
                var mesh = BoxMan.sharedMesh;
                mesh.name = "box-man";
            }

            foreach (var (preset, transform) in expressions)
            {
                ExpressionMap.Add(preset, transform);
            }
        }
    }
}
