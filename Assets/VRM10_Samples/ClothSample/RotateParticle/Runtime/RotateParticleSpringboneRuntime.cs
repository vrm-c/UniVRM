using System;
using System.Threading.Tasks;
using RotateParticle.Components;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UniVRM10;


namespace RotateParticle
{
    public class RotateParticleSpringboneRuntime : IVrm10SpringBoneRuntime
    {
        Vrm10Instance _vrm;
        Action<IRotateParticleSystem, Vrm10Instance> _onInit;


        IRotateParticleSystem _system;

        public RotateParticleSpringboneRuntime(Action<IRotateParticleSystem, Vrm10Instance> onInit = null)
        {
            _onInit = onInit;
        }

        public void Dispose()
        {
        }

        public async Task InitializeAsync(Vrm10Instance instance, IAwaitCaller awaitCaller)
        {
            _vrm = instance;

            var animator = instance.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("no animator");
                return;
            }

            var avatar = animator.avatar;
            if (!avatar.isHuman)
            {
                Debug.LogWarning("not humanoid");
                return;
            }

            _system = new RotateParticleSystem();

            if (_onInit != null)
            {
                _onInit(_system, instance);
            }

            var warps = instance.GetComponentsInChildren<Warp>();
            var cloths = instance.GetComponentsInChildren<RectCloth>();
            await awaitCaller.NextFrame();
            _system.Initialize(warps, cloths);
        }

        public void Process()
        {
            if (_system == null) return;
            _system.Process(Time.deltaTime);
        }

        public bool ReconstructSpringBone()
        {
            return false;
        }

        public void RestoreInitialTransform()
        {
            if (_system == null) return;
            _system.ResetInitialRotation();
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
        }

        public void DrawGizmos()
        {
            if (_system == null) return;
            _system.DrawGizmos();
        }
    }
}