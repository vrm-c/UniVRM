using System;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public class VrmAnimationExporter : gltfExporter
    {
        public VrmAnimationExporter(
                ExportingGltfData data,
                GltfExportSettings settings)
        : base(data, settings) { }

        // private (INormalizedPoseProvider, ITPoseProvider) m_controlRig;
        readonly List<float> m_times = new();

        class PositionExporter
        {
            public List<Vector3> Values = new();
            public Transform Node;
            readonly Transform m_root;

            public PositionExporter(Transform bone, Transform root)
            {
                Node = bone;
                m_root = root;
            }

            public void Add()
            {
                Values.Add(m_root.worldToLocalMatrix.MultiplyPoint(Node.position));
            }
        }
        PositionExporter m_position;

        class RotationExporter
        {
            public List<Quaternion> Values = new();
            readonly Transform Node;
            public Transform m_parent;

            public RotationExporter(Transform bone, Transform parent)
            {
                Node = bone;
                m_parent = parent;
            }

            public void Add()
            {
                Values.Add(Quaternion.Inverse(m_parent.rotation) * Node.rotation);
            }
        }
        readonly Dictionary<HumanBodyBones, RotationExporter> m_rotations = new();

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

        private void AddFrame(TimeSpan time)
        {
            m_times.Add((float)time.TotalSeconds);
            m_position.Add();
            foreach (var kv in m_rotations)
            {
                kv.Value.Add();
            }
        }

        public void Export(BvhImporterContext bvh)
        {
            base.Export(new RuntimeTextureSerializer());

            //
            // setup
            //
            var map = new Dictionary<HumanBodyBones, Transform>();
            var animator = bvh.Root.GetComponent<Animator>();
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

            m_position = new PositionExporter(map[HumanBodyBones.Hips],
                    bvh.Root.transform);

            foreach (var kv in map)
            {
                var vrmBone = Vrm10HumanoidBoneSpecification.ConvertFromUnityBone(kv.Key);
                var parent = GetParentBone(map, vrmBone) ?? bvh.Root.transform;
                m_rotations.Add(kv.Key, new RotationExporter(kv.Value, parent));
            }

            //
            // get data
            //
            var animation = bvh.Root.gameObject.GetComponent<Animation>();
            var clip = animation.clip;
            var state = animation[clip.name];

            var time = default(TimeSpan);
            for (int i = 0; i < bvh.Bvh.FrameCount; ++i, time += bvh.Bvh.FrameTime)
            {
                state.time = (float)time.TotalSeconds;
                animation.Sample();
                AddFrame(time);
            }

            //
            // export
            // 
            var gltfAnimation = new glTFAnimation
            {
            };
            _data.Gltf.animations.Add(gltfAnimation);

            // time values
            var input = _data.ExtendBufferAndGetAccessorIndex(m_times.ToArray());

            {
                var output = _data.ExtendBufferAndGetAccessorIndex(m_position.Values.ToArray());
                var sampler = gltfAnimation.samplers.Count;
                gltfAnimation.samplers.Add(new glTFAnimationSampler
                {
                    input = input,
                    output = output,
                    interpolation = "LINEAR",
                });

                gltfAnimation.channels.Add(new glTFAnimationChannel
                {
                    sampler = sampler,
                    target = new glTFAnimationTarget
                    {
                        node = Nodes.IndexOf(m_position.Node),
                        path = "translation",
                    },
                });
            }

            foreach (var kv in m_rotations)
            {
                var output = _data.ExtendBufferAndGetAccessorIndex(kv.Value.Values.ToArray());
                var sampler = gltfAnimation.samplers.Count;
                gltfAnimation.samplers.Add(new glTFAnimationSampler
                {
                    input = input,
                    output = output,
                    interpolation = "LINEAR",
                });

                gltfAnimation.channels.Add(new glTFAnimationChannel
                {
                    sampler = sampler,
                    target = new glTFAnimationTarget
                    {
                        node = Nodes.IndexOf(m_position.Node),
                        path = "rotation",
                    },
                });
            }
        }

        public static byte[] BvhToVrmAnimation(string path)
        {
            var bvh = new BvhImporterContext();
            bvh.Parse(path, File.ReadAllText(path));
            bvh.Load();

            var data = new ExportingGltfData();
            using (var exporter = new VrmAnimationExporter(
                        data, new GltfExportSettings()))
            {
                exporter.Prepare(bvh.Root.gameObject);
                exporter.Export(bvh);
                return data.ToGlbBytes();
            }
        }
    }
}
