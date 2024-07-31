using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public class VrmAnimationExporter : gltfExporter
    {
        public VrmAnimationExporter(
                ExportingGltfData data,
                GltfExportSettings settings)
        : base(data, settings)
        {
            settings.InverseAxis = Axes.X;
        }

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
                var p = m_root.worldToLocalMatrix.MultiplyPoint(Node.position);
                // reverse-X
                Values.Add(new Vector3(-p.x, p.y, p.z));
            }
        }
        PositionExporter m_position;
        public void SetPositionBoneAndParent(Transform bone, Transform parent)
        {
            m_position = new PositionExporter(bone, parent);
        }

        class RotationExporter
        {
            public List<Quaternion> Values = new();
            public readonly Transform Node;
            public Transform m_parent;

            public RotationExporter(Transform bone, Transform parent)
            {
                Node = bone;
                m_parent = parent;
            }

            public void Add()
            {
                var q = Quaternion.Inverse(m_parent.rotation) * Node.rotation;
                // reverse-X
                Values.Add(new Quaternion(q.x, -q.y, -q.z, q.w));
            }
        }
        readonly Dictionary<HumanBodyBones, RotationExporter> m_rotations = new();
        public void AddRotationBoneAndParent(HumanBodyBones bone, Transform transform, Transform parent)
        {
            m_rotations.Add(bone, new RotationExporter(transform, parent));
        }

        public void AddFrame(TimeSpan time)
        {
            m_times.Add((float)time.TotalSeconds);
            m_position.Add();
            foreach (var kv in m_rotations)
            {
                kv.Value.Add();
            }
        }

        public void Export(Action<VrmAnimationExporter> addFrames)
        {
            base.Export();

            addFrames(this);

            //
            // export
            // 
            var gltfAnimation = new glTFAnimation
            {
            };
            _data.Gltf.animations.Add(gltfAnimation);

            // this.Nodes には 右手左手変換後のコピーが入っている
            // 代替として名前で逆引きする
            var names = Nodes.Select(x => x.name).ToList();

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
                        node = names.IndexOf(m_position.Node.name),
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
                        node = names.IndexOf(kv.Value.Node.name),
                        path = "rotation",
                    },
                });
            }

            // VRMC_vrm_animation
            var vrmAnimation = VrmAnimationUtil.Create(m_rotations.ToDictionary(kv => kv.Key, kv => kv.Value.Node), names);
            UniGLTF.Extensions.VRMC_vrm_animation.GltfSerializer.SerializeTo(
                    ref _data.Gltf.extensions
                    , vrmAnimation);
        }
    }
}
