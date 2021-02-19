using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using VrmLib.Bvh;

namespace VrmLib
{
    public static class ModelExtensionsForBvh
    {
        static float ToRad(float src)
        {
            return src / 180.0f * MathFWrap.PI;
        }

        static BvhNode GetNode(BvhNode root, string path)
        {
            var splitted = path.Split('/');

            var it = splitted.Select(x => x).GetEnumerator();
            var current = root;
            if (splitted[0] == path)
            {
                return current;
            }
            it.MoveNext();
            while (it.MoveNext())
            {
                current = current.Children.First(x => x.Name == it.Current);
            }

            return current;
        }

        public static Model CreateFromBvh(BvhNode node)
        {
            // add nodes
            var model = new Model(Coordinates.Vrm1);
            model.Root.Name = "__bvh_root__";

            AddBvhNodeRecursive(model, model.Root, node);

            return model;
        }

        static void AddBvhNodeRecursive(Model model, Node parent, BvhNode node)
        {
            var newNode = new Node(node.Name)
            {
                HumanoidBone = node.Bone,
            };

            model.Nodes.Add(newNode);
            parent.Add(newNode);
            newNode.Translation = node.SkeletonLocalPosition;

            foreach (var child in node.Children)
            {
                AddBvhNodeRecursive(model, newNode, child);
            }
        }

        class BvhNodeCurves
        {
            public Bvh.ChannelCurve LocalPositionX;
            public Bvh.ChannelCurve LocalPositionY;
            public Bvh.ChannelCurve LocalPositionZ;

            public Bvh.ChannelCurve EulerX;
            public Bvh.ChannelCurve EulerY;
            public Bvh.ChannelCurve EulerZ;

            public void Set(string prop, Bvh.ChannelCurve curve)
            {
                switch (prop)
                {
                    case "localPosition.x":
                        LocalPositionX = curve;
                        break;

                    case "localPosition.y":
                        LocalPositionY = curve;
                        break;

                    case "localPosition.z":
                        LocalPositionZ = curve;
                        break;

                    case "localEulerAnglesBaked.x":
                        EulerX = curve;
                        break;

                    case "localEulerAnglesBaked.y":
                        EulerY = curve;
                        break;

                    case "localEulerAnglesBaked.z":
                        EulerZ = curve;
                        break;

                    default:
                        break;
                }
            }
        }

        static Animation LoadAnimation(string name, Bvh.Bvh bvh, Model model, float scalingFactor)
        {
            var animation = new Animation(name);

            Dictionary<string, BvhNodeCurves> pathMap = new Dictionary<string, BvhNodeCurves>();

            for (int i = 0; i < bvh.Channels.Length; ++i)
            {
                var channel = bvh.Channels[i];

                if (!bvh.TryGetPathWithPropertyFromChannel(channel, out Bvh.Bvh.PathWithProperty prop))
                {
                    throw new Exception();
                }

                if (!pathMap.TryGetValue(prop.Path, out BvhNodeCurves curves))
                {
                    curves = new BvhNodeCurves();
                    pathMap.Add(prop.Path, curves);
                }

                curves.Set(prop.Property, channel);
            }

            // setup time
            var timeBytes = new byte[Marshal.SizeOf(typeof(float)) * bvh.FrameCount];
            var timeSpan = SpanLike.Wrap<Single>(new ArraySegment<byte>(timeBytes));
            var now = 0.0;
            for (int i = 0; i < timeSpan.Length; ++i, now += bvh.FrameTime.TotalSeconds)
            {
                timeSpan[i] = (float)now;
            }
            var times = new BufferAccessor(new ArraySegment<byte>(timeBytes), AccessorValueType.FLOAT, AccessorVectorType.SCALAR, bvh.FrameCount);

            foreach (var (key, nodeCurve) in pathMap)
            {
                var node = Model.GetNode(model.Root, key);
                var bvhNode = GetNode(bvh.Root, key);
                var curve = new NodeAnimation();

                if (nodeCurve.LocalPositionX != null)
                {
                    var values = new byte[Marshal.SizeOf(typeof(Vector3))
                        * nodeCurve.LocalPositionX.Keys.Length];
                    var span = SpanLike.Wrap<Vector3>(new ArraySegment<byte>(values));
                    for (int i = 0; i < nodeCurve.LocalPositionX.Keys.Length; ++i)
                    {
                        span[i] = new Vector3
                        {
                            X = nodeCurve.LocalPositionX.Keys[i] * scalingFactor,
                            Y = nodeCurve.LocalPositionY.Keys[i] * scalingFactor,
                            Z = nodeCurve.LocalPositionZ.Keys[i] * scalingFactor,
                        };
                    }
                    var sampler = new CurveSampler
                    {
                        In = times,
                        Out = new BufferAccessor(new ArraySegment<byte>(values),
                            AccessorValueType.FLOAT, AccessorVectorType.VEC3, span.Length)
                    };
                    curve.Curves.Add(AnimationPathType.Translation, sampler);
                }

                if (nodeCurve.EulerX != null)
                {
                    var values = new byte[Marshal.SizeOf(typeof(Quaternion))
                        * nodeCurve.EulerX.Keys.Length];
                    var span = SpanLike.Wrap<Quaternion>(new ArraySegment<byte>(values));

                    Func<Quaternion, BvhNodeCurves, int, Quaternion> getRot = (q, c, i) => q;

                    foreach (var ch in bvhNode.Channels)
                    {
                        var tmp = getRot;
                        switch (ch)
                        {
                            case Channel.Xrotation:
                                getRot = (_, c, i) =>
                                {
                                    return tmp(_, c, i) *
                                    Quaternion.CreateFromAxisAngle(Vector3.UnitX, ToRad(c.EulerX.Keys[i]));
                                };
                                break;
                            case Channel.Yrotation:
                                getRot = (_, c, i) =>
                                {
                                    return tmp(_, c, i) *
                                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, ToRad(c.EulerY.Keys[i]));
                                };
                                break;
                            case Channel.Zrotation:
                                getRot = (_, c, i) =>
                                {
                                    return tmp(_, c, i) *
                                    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, ToRad(c.EulerZ.Keys[i]));
                                };
                                break;
                            default:
                                // throw new NotImplementedException();
                                break;
                        }
                    }

                    for (int i = 0; i < nodeCurve.EulerX.Keys.Length; ++i)
                    {
                        span[i] = getRot(Quaternion.Identity, nodeCurve, i);
                    }
                    var sampler = new CurveSampler
                    {
                        In = times,
                        Out = new BufferAccessor(new ArraySegment<byte>(values),
                            AccessorValueType.FLOAT, AccessorVectorType.VEC4, span.Length)
                    };
                    curve.Curves.Add(AnimationPathType.Rotation, sampler);
                }

                animation.AddCurve(node, curve);
            }

            return animation;
        }

        public static Model Load(string name, Bvh.Bvh bvh)
        {
            var model = CreateFromBvh(bvh.Root);

            // estimate skeleton
            var skeleton = SkeletonEstimator.Detect(model.Root);
            if (skeleton == null)
            {
                throw new Exception("fail to estimate skeleton");
            }

            // foot to zero
            var minY = model.Nodes.Min(x => x.Translation.Y);
            var hips = model.Nodes.First(x => x.HumanoidBone == HumanoidBones.hips);
            if (model.Root.Children.Count != 1)
            {
                throw new Exception();
            }
            if (model.Root.Children[0] != hips)
            {
                throw new Exception();
            }
            hips.Translation -= new Vector3(0, minY, 0);

            // normalize scale
            var pos = hips.Translation;
            var factor = 1.0f;
            if (pos.Y != 0)
            {
                factor = 1.0f / pos.Y;
                foreach (var x in hips.Traverse())
                {
                    x.LocalTranslation *= factor;
                }
                hips.Translation = new Vector3(pos.X, 1.0f, pos.Z);
            }

            // animation
            model.Animations.Add(LoadAnimation(name, bvh, model, factor));

            // add origin
            var origin = new Node("origin");
            origin.Add(model.Root.Children[0]);
            model.Nodes.Add(origin);
            model.Root.Add(origin);

            return model;
        }

        public static void CreateBoxMan(this Model model)
        {
            // skin
            var skin = new Skin();
            skin.Joints.AddRange(model.Nodes);
            skin.CalcInverseMatrices();

            // mesh
            var group = new MeshGroup("box-man")
            {
                Skin = skin,
            };
            var builder = new MeshBuilder();
            builder.Build(model.Nodes);
            group.Meshes.Add(builder.CreateMesh());
            model.MeshGroups.Add(group);

            // node
            var meshNode = new Node("mesh");
            meshNode.MeshGroup = group;
            model.Nodes.Add(meshNode);
            model.Root.Add(meshNode);
        }
    }
}