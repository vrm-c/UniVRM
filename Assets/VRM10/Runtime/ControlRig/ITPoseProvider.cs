using System;
using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;

namespace UniVRM10
{
    public interface ITPoseProvider
    {
        /// <summary>
        /// TPose 時のモデルルートを基準とした world 回転と位置を返す。
        /// 該当する bone が無いときは null。
        /// </summary>
        EuclideanTransform? GetWorldTransform(HumanBodyBones bone);
    }

    public static class ITPoseProviderExtensions
    {
        static HumanBodyBones GetParent(ITPoseProvider self, HumanBodyBones bone)
        {
            var current = bone;
            // 無限ループ防止のため念のためカウンター付き
            for (int i = 0; i < 100; ++i)
            {
                var vrmBone = Vrm10HumanoidBoneSpecification.ConvertFromUnityBone(current);
                var def = Vrm10HumanoidBoneSpecification.GetDefine(vrmBone);
                var parentBone = Vrm10HumanoidBoneSpecification.ConvertToUnityBone(def.ParentBone.Value);
                if (self.GetWorldTransform(parentBone).HasValue)
                {
                    return parentBone;
                }
                current = parentBone;
            }

            // Hips 以外は必ず親ボーンが有るはずなのでここには来ない
            throw new Exception("parent not found");
        }

        public static IEnumerable<(HumanBodyBones Bone, HumanBodyBones Parent)> EnumerateBoneParentPairs(this ITPoseProvider self)
        {
            foreach (var bone in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }
                if (!self.GetWorldTransform(bone).HasValue)
                {
                    continue;
                }

                if (bone == HumanBodyBones.Hips)
                {
                    yield return (bone, HumanBodyBones.LastBone);
                }
                else
                {
                    yield return (bone, GetParent(self, bone));
                }
            }
        }
    }
}
