#define USE_ROTATEPARTICLE_JOB
using System;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UniVRM10;


namespace RotateParticle
{
    public class RotateParticleSpringboneRuntime : IVrm10SpringBoneRuntime
    {
        Action<Vrm10Instance> _onInit;
        IRotateParticleSystem _system;

        public RotateParticleSpringboneRuntime(Action<Vrm10Instance> onInit = null)
        {
            _onInit = onInit;
        }

        public void Dispose()
        {
            if (_system != null)
            {
                _system.Dispose();
                _system = null;
            }
        }

        public async Task InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            if (_onInit != null)
            {
                _onInit(vrm);
            }

#if USE_ROTATEPARTICLE_JOB
            _system = new Jobs.RotateParticleJobSystem();
#else
            _system = new RotateParticleSystem();
#endif           
            await _system.InitializeAsync(vrm, awaitCaller);
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