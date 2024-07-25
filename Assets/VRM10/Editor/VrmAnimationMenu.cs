using System;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UniHumanoid;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    internal static class VrmAnimationMenu
    {
        public const string MENU_NAME = "Convert BVH to VRM-Animation...";

        public static void BvhToVrmAnimationMenu()
        {
            var path = EditorUtility.OpenFilePanel("select bvh", null, "bvh");
            if (!string.IsNullOrEmpty(path))
            {
                var bytes = BvhToVrmAnimation(path);
                var dst = EditorUtility.SaveFilePanel("write vrma",
                        Path.GetDirectoryName(path),
                        Path.GetFileNameWithoutExtension(path),
                        "vrma");
                if (!string.IsNullOrEmpty(dst))
                {
                    File.WriteAllBytes(dst, bytes);
                }
            }
        }

        static Transform GetParentBone(Dictionary<HumanBodyBones, Transform> map, Vrm10HumanoidBones bone)
        {
            while (true)
            {
                if (bone == Vrm10HumanoidBones.Hips)
                {
                    break;
                }
                var parentBone = Vrm10HumanoidBoneSpecification.GetDefine(bone).ParentBone.Value;
                var unityParentBone = Vrm10HumanoidBoneSpecification.ConvertToUnityBone(parentBone);
                if (map.TryGetValue(unityParentBone, out var found))
                {
                    return found;
                }
                bone = parentBone;
            }

            // hips has no parent
            return null;
        }

        /// 使用する BVH は次の条件を満たす必要があります。
        ///
        /// * レストポーズが TPose であること
        ///
        /// また、BVH には HumanBone の割り当てが記述されておらず、
        /// 大きさに関しても単位の規定がありません。
        ///
        /// bvh.Load 関数の中で HumanBone の名前ベースの割り当て処理と、
        /// メートルサイズへのスケーリング(cm to meter など)をしています。
        ///
        /// bvh のボーン割り当てを追加する場合は、
        /// bvh.Parse 内の Skeleton.Estimate 関数を参照してください。
        ///
        static byte[] BvhToVrmAnimation(string path)
        {
            var bvh = new BvhImporterContext();
            bvh.Parse(path, File.ReadAllText(path));
            bvh.Load();

            var data = new ExportingGltfData();
            using var exporter = new VrmAnimationExporter(
                        data, new GltfExportSettings());
            exporter.Prepare(bvh.Root.gameObject);

            exporter.Export((VrmAnimationExporter vrma) =>
            {
                //
                // setup
                //
                var map = new Dictionary<HumanBodyBones, Transform>();
                var animator = bvh.Root.GetComponentOrThrow<Animator>();
                foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (bone == HumanBodyBones.LastBone)
                    {
                        continue;
                    }
                    var t = animator.GetBoneTransform(bone);
                    if (t == null)
                    {
                        continue;
                    }
                    map.Add(bone, t);
                }

                vrma.SetPositionBoneAndParent(map[HumanBodyBones.Hips], bvh.Root.transform);

                foreach (var kv in map)
                {
                    var vrmBone = Vrm10HumanoidBoneSpecification.ConvertFromUnityBone(kv.Key);
                    var parent = GetParentBone(map, vrmBone) ?? bvh.Root.transform;
                    vrma.AddRotationBoneAndParent(kv.Key, kv.Value, parent);
                }

                //
                // get data
                //
                var animation = bvh.Root.gameObject.GetComponentOrThrow<Animation>();
                var clip = animation.clip;
                var state = animation[clip.name];

                var time = default(TimeSpan);
                for (int i = 0; i < bvh.Bvh.FrameCount; ++i, time += bvh.Bvh.FrameTime)
                {
                    state.time = (float)time.TotalSeconds;
                    animation.Sample();
                    vrma.AddFrame(time);
                }

            });

            return data.ToGlbBytes();
        }
    }
}
