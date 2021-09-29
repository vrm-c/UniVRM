using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;
using UniVRM10.FastSpringBones.NativeWrappers;
using UniVRM10.FastSpringBones.Registries;

namespace UniVRM10.FastSpringBones.Components
{
    /// <summary>
    /// VRMSpringBoneColliderGroupに対応したクラス
    /// バッファの作成も行う
    /// </summary>
    public sealed unsafe class FastSpringBoneColliderGroup : MonoBehaviour
    {
        private NativeTransform _nativeTransform;
        private NativeColliderGroup _nativeColliderGroup;

        public BlittableColliderGroup* ColliderGroupPtr => _nativeColliderGroup.GetUnsafePtr();

        public void Initialize(TransformRegistry transformRegistry, BlittableCollider[] colliders)
        {
            _nativeTransform = new NativeTransform(transformRegistry, TransformSynchronizationType.PullOnly, transform);
            _nativeColliderGroup = new NativeColliderGroup(colliders, _nativeTransform);
        }

        private void OnDestroy()
        {
            _nativeTransform?.Dispose();
            _nativeColliderGroup?.Dispose();
        }
    }
}
