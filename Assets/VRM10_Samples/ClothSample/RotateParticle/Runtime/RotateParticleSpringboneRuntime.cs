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
        RotateParticleSystem _system;
        Action<RotateParticleSystem, Vrm10Instance> _onInit;

        public RotateParticleSpringboneRuntime(Action<RotateParticleSystem, Vrm10Instance> onInit = null)
        {
            _onInit = onInit;
        }

        public void Dispose()
        {
        }

        public async Task InitializeAsync(Vrm10Instance instance, IAwaitCaller awaitCaller)
        {
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
            _system.Env.DragForce = 0.6f;
            _system.Env.Stiffness = 0.07f;

            if (_onInit != null)
            {
                _onInit(_system, instance);
            }

            foreach (var warp in instance.GetComponentsInChildren<Warp>())
            {
                _system._warps.Add(warp);
            }

            foreach (var cloth in instance.GetComponentsInChildren<RectCloth>())
            {
                _system._cloths.Add(cloth);
            }

            foreach (var g in instance.SpringBone.ColliderGroups)
            {
                foreach (var vrmCollider in g.Colliders)
                {
                    _system.AddColliderIfNotExists(g.name, vrmCollider);
                }
            }

            await awaitCaller.NextFrame();
            _system.Initialize();
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