// #define USE_ROTATEPARTICLE_JOB
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
        Vrm10Instance _vrm;
        Action<Vrm10Instance> _onInit;
        IRotateParticleSystem _system;
        bool _building = false;

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
            _building = true;
            _vrm = vrm;

            if (_system != null)
            {
                _system.Dispose();
            }

            if (_onInit != null)
            {
                _onInit(vrm);
                _onInit = null;
            }

#if USE_ROTATEPARTICLE_JOB
            _system = new Jobs.RotateParticleJobSystem();
#else
            _system = new RotateParticleSystem();
#endif           
            await _system.InitializeAsync(vrm, awaitCaller);

            _building = false;
        }

        public void Process()
        {
            if (_system == null) return;
            _system.Process(Time.deltaTime);
        }

        public bool ReconstructSpringBone()
        {
            if (_vrm == null)
            {
                return false;
            }
            if (_building)
            {
                return false;
            }
            var task = InitializeAsync(_vrm, new ImmediateCaller());
            return true;
        }

        public void RestoreInitialTransform()
        {
            if (_system == null) return;
            _system.ResetInitialRotation();
        }

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            if (_system == null) return;
            _system.SetJointLevel(joint, jointSettings);
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
            if (_system == null) return;
        }

        public void DrawGizmos()
        {
            if (_system == null) return;
            _system.DrawGizmos();
        }
    }
}