using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.Components;
using VRMShaders;
using Object = UnityEngine.Object;

namespace VRM
{
    /// <summary>
    /// 指定されたGameObject内にあるSpringBoneをFastSpringBoneに差し替えるユーティリティ
    /// </summary>
    public static class FastSpringBoneReplacer
    {
        public static async Task ReplaceAsync(GameObject gameObject, IAwaitCaller awaitCaller = null, CancellationToken token = default)
        {
            var service = FastSpringBoneService.Instance;
            var springBones = gameObject.GetComponentsInChildren<VRMSpringBone>();
            var disposer = gameObject.AddComponent<FastSpringBoneDisposer>();

            // VRMSpringBoneで動いた後の状態がFastSpringBoneの初期状態にならないようにするためawait UniTask.Yield()する前にVRMSpringBoneをdisableにしておく
            foreach (var springBone in springBones)
            {
                springBone.enabled = false;
            };

            if (awaitCaller != null)
            {
                await awaitCaller.NextFrame();
                token.ThrowIfCancellationRequested();
            }

            var vrmColliderGroups = gameObject.GetComponentsInChildren<VRMSpringBoneColliderGroup>();
            var colliderGroupDictionary = new Dictionary<VRMSpringBoneColliderGroup, FastSpringBoneColliderGroup>();

            // Colliderを差し替える
            foreach (var vrmColliderGroup in vrmColliderGroups)
            {
                if (awaitCaller != null)
                {
                    await awaitCaller.NextFrame();
                    token.ThrowIfCancellationRequested();
                }

                var fastSpringBoneCollider = vrmColliderGroup.gameObject.AddComponent<FastSpringBoneColliderGroup>();
                fastSpringBoneCollider.Initialize(
                    service.TransformRegistry,
                    vrmColliderGroup.Colliders
                        .Select(data => new BlittableCollider(data.Offset, data.Radius))
                        .ToArray()
                );
                colliderGroupDictionary[vrmColliderGroup] = fastSpringBoneCollider;
            }

            var springRootBones =
            (
                from springBone in springBones
                from rootBone in springBone.RootBones
                select (springBone, rootBone)
            ).ToList();

            for (var i = 0; i < springRootBones.Count; i++)
            {
                var current = springRootBones[i];

                // 他のRootBoneのどれかが、自分の親（もしくは同じTransform）なら自分自身を削除する
                if (springRootBones
                    .Where(other => other != current)
                    .Any(other => current.rootBone.IsChildOf(other.rootBone)))
                {
                    springRootBones.RemoveAt(i);
                    --i;
                }
            }

            if (awaitCaller != null)
            {
                await awaitCaller.NextFrame();
                token.ThrowIfCancellationRequested();
            }
            token.ThrowIfCancellationRequested();

            foreach (var (vrmSpringBone, rootBoneTransform) in springRootBones)
            {
                // FastSpringRootBoneに差し替える
                var fastSpringRootBone = 
                    new FastSpringRootBone(
                        service.TransformRegistry,
                        rootBoneTransform, 
                        service.RootBoneRegistry,
                        service.ColliderGroupRegistry);
                disposer.Add(fastSpringRootBone);

                var colliderGroups =
                    vrmSpringBone.ColliderGroups != null
                        ? vrmSpringBone.ColliderGroups.Select(group => colliderGroupDictionary[@group]).ToArray()
                        : Array.Empty<FastSpringBoneColliderGroup>();

                fastSpringRootBone.Initialize(
                    vrmSpringBone.m_gravityPower,
                    vrmSpringBone.m_gravityDir,
                    vrmSpringBone.m_dragForce,
                    vrmSpringBone.m_stiffnessForce,
                    colliderGroups,
                    vrmSpringBone.m_hitRadius,
                    vrmSpringBone.m_center
                );

                Object.Destroy(vrmSpringBone);
                
                if (awaitCaller != null)
                {
                    await awaitCaller.NextFrame();
                    token.ThrowIfCancellationRequested();
                }
                token.ThrowIfCancellationRequested();
            }

            // Colliderを削除
            foreach (var vrmSpringBoneColliderGroup in vrmColliderGroups)
            {
                Object.Destroy(vrmSpringBoneColliderGroup);
            }
        }
    }
}
