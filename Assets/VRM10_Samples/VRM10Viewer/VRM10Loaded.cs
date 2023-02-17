using System;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Utils;
using UnityEngine;
using VRMShaders;
using static UniVRM10.Vrm10;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10Loaded : IDisposable
    {
        RuntimeGltfInstance m_instance;
        Vrm10Instance m_controller;

        public IControlRigSetter ControlRig => m_controller?.Runtime.ControlRig;

        VRM10AIUEO m_lipSync;
        bool m_enableLipSyncValue;
        public bool EnableLipSyncValue
        {
            set
            {
                if (m_enableLipSyncValue == value) return;
                m_enableLipSyncValue = value;
                if (m_lipSync != null)
                {
                    m_lipSync.enabled = m_enableLipSyncValue;
                }
            }
        }

        VRM10AutoExpression m_autoExpression;
        bool m_enableAutoExpressionValue;
        public bool EnableAutoExpressionValue
        {
            set
            {
                if (m_enableAutoExpressionValue == value) return;
                m_enableAutoExpressionValue = value;
                if (m_autoExpression != null)
                {
                    m_autoExpression.enabled = m_enableAutoExpressionValue;
                }
            }
        }

        VRM10Blinker m_blink;
        bool m_enableBlinkValue;
        public bool EnableBlinkValue
        {
            set
            {
                if (m_blink == value) return;
                m_enableBlinkValue = value;
                if (m_blink != null)
                {
                    m_blink.enabled = m_enableBlinkValue;
                }
            }
        }

        public VRM10Loaded(RuntimeGltfInstance instance, Transform lookAtTarget)
        {
            m_instance = instance;

            m_controller = instance.GetComponent<Vrm10Instance>();
            if (m_controller != null)
            {
                // VRM
                m_controller.UpdateType = Vrm10Instance.UpdateTypes.LateUpdate; // after HumanPoseTransfer's setPose
                {
                    m_lipSync = instance.gameObject.AddComponent<VRM10AIUEO>();
                    m_blink = instance.gameObject.AddComponent<VRM10Blinker>();
                    m_autoExpression = instance.gameObject.AddComponent<VRM10AutoExpression>();

                    m_controller.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.SpecifiedTransform;
                    m_controller.LookAtTarget = lookAtTarget;
                }
            }

            var animation = instance.GetComponent<Animation>();
            if (animation && animation.clip != null)
            {
                // GLTF animation
                animation.Play(animation.clip.name);
            }
        }

        public void Dispose()
        {
            // destroy GameObject
            GameObject.Destroy(m_instance.gameObject);
        }

        static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpGltfMaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInGltfMaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpVrm10MaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInVrm10MaterialDescriptorGenerator();
            }
        }

        public static async Task<VRM10Loaded> LoadAsync(string path, bool useAsync, bool useUrp,
            VrmMetaInformationCallback metaCallback, CancellationToken cancellationToken, Transform lookAtTarget)
        {
            Debug.LogFormat("{0}", path);
            var vrm10Instance = await Vrm10.LoadPathAsync(path,
                canLoadVrm0X: true,
                showMeshes: false,
                awaitCaller: useAsync ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller(),
                materialGenerator: GetVrmMaterialDescriptorGenerator(useUrp),
                vrmMetaInformationCallback: metaCallback,
                ct: cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(vrm10Instance.gameObject);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (vrm10Instance == null)
            {
                Debug.LogWarning("LoadPathAsync is null");
                return null;
            }

            var instance = vrm10Instance.GetComponent<RuntimeGltfInstance>();
            instance.ShowMeshes();
            instance.EnableUpdateWhenOffscreen();
            return new VRM10Loaded(instance, lookAtTarget);
        }

        /// <summary>
        /// from v0.103
        /// </summary>
        /// <param name="src"></param>
        public void UpdateControlRigExplicit(Animator src)
        {
            var controlRig = m_controller.Runtime.ControlRig;

            foreach (HumanBodyBones bone in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }

                // var controlRigBone = controlRig.GetBoneTransform(bone);
                // if (controlRigBone == null)
                // {
                //     continue;
                // }

                var bvhBone = src.GetBoneTransform(bone);
                if (bvhBone != null)
                {
                    // set normalized pose
                    controlRig.SetNormalizedLocalRotation(bone, bvhBone.localRotation);
                }

                if (bone == HumanBodyBones.Hips)
                {
                    controlRig.SetRootPosition(bvhBone.localPosition);
                }
            }
        }

        public void TPoseControlRig()
        {
            var controlRig = m_controller.Runtime.ControlRig;
            controlRig.EnforceTPose();
        }
    }
}
