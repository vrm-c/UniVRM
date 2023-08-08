using System;
using System.Collections.Generic;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    public class VrmAnimationInstance : MonoBehaviour
    {
        public SkinnedMeshRenderer BoxMan;
        public (INormalizedPoseProvider, ITPoseProvider) ControlRig;
        public Dictionary<ExpressionKey, Func<float>> ExpressionMap = new();

        public float preset_happy;
        public float preset_angry;
        public float preset_sad;
        public float preset_relaxed;
        public float preset_surprised;
        public float preset_aa;
        public float preset_ih;
        public float preset_ou;
        public float preset_ee;
        public float preset_oh;
        public float preset_blink;
        public float preset_blinkleft;
        public float preset_blinkright;
        // public float preset_lookup;
        // public float preset_lookdown;
        // public float preset_lookleft;
        // public float preset_lookright;
        public float preset_neutral;

        public void Initialize(IEnumerable<ExpressionKey> keys)
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

            foreach (var key in keys)
            {
                switch (key.Preset)
                {
                    case ExpressionPreset.happy: ExpressionMap.Add(key, () => preset_happy); break;
                    case ExpressionPreset.angry: ExpressionMap.Add(key, () => preset_angry); break;
                    case ExpressionPreset.sad: ExpressionMap.Add(key, () => preset_sad); break;
                    case ExpressionPreset.relaxed: ExpressionMap.Add(key, () => preset_relaxed); break;
                    case ExpressionPreset.surprised: ExpressionMap.Add(key, () => preset_surprised); break;
                    case ExpressionPreset.aa: ExpressionMap.Add(key, () => preset_aa); break;
                    case ExpressionPreset.ih: ExpressionMap.Add(key, () => preset_ih); break;
                    case ExpressionPreset.ou: ExpressionMap.Add(key, () => preset_ou); break;
                    case ExpressionPreset.ee: ExpressionMap.Add(key, () => preset_ee); break;
                    case ExpressionPreset.oh: ExpressionMap.Add(key, () => preset_oh); break;
                    case ExpressionPreset.blink: ExpressionMap.Add(key, () => preset_blink); break;
                    case ExpressionPreset.blinkLeft: ExpressionMap.Add(key, () => preset_blinkleft); break;
                    case ExpressionPreset.blinkRight: ExpressionMap.Add(key, () => preset_blinkright); break;
                    // case ExpressionPreset.lookUp: ExpressionMap.Add(key, () => preset_lookUp); break;
                    // case ExpressionPreset.lookDown: ExpressionMap.Add(key, () => preset_lookDown); break;
                    // case ExpressionPreset.lookLeft: ExpressionMap.Add(key, () => preset_lookLeft); break;
                    // case ExpressionPreset.lookRight: ExpressionMap.Add(key, () => preset_lookRight); break;
                    case ExpressionPreset.neutral: ExpressionMap.Add(key, () => preset_neutral); break;
                }
            }
        }
    }
}
