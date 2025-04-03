using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace UniHumanoid
{
    public static class SkeletonMeshUtility
    {
        readonly struct TSRBox
        {
            public readonly Vector3 T;
            public readonly Vector3 S;
            public readonly Quaternion R;
            public readonly Vector3 HeadPosition;

            public TSRBox(
             Vector3 t,
             Vector3 s,
             Quaternion r,
             Vector3 head)
            {
                T = t;
                S = s;
                R = r;
                HeadPosition = head;
            }

            public Matrix4x4 ToMatrix()
            {
                return Matrix4x4.Translate(HeadPosition)
                    * Matrix4x4.Rotate(R) * Matrix4x4.Scale(S) * Matrix4x4.Translate(T);
            }
        }

        class MeshBuilder
        {
            List<Vector3> m_positions = new List<Vector3>();
            List<int> m_indices = new List<int>();
            List<BoneWeight> m_boneWeights = new List<BoneWeight>();

            public void AddBone(Vector3 head, Vector3 tail, int boneIndex, float width, float height, Vector3 xAxis)
            {
                var yAxis = (tail - head).normalized;
                var zAxis = Vector3.Cross(xAxis, yAxis);
                xAxis = Vector3.Cross(yAxis, zAxis);

                AddBox(boneIndex, new TSRBox
                (
                    new Vector3(0, 0.5f, 0),
                    new Vector3(width, (tail - head).magnitude, height),
                    new Matrix4x4(
                        xAxis, yAxis, zAxis, new Vector4(0, 0, 0, 1)
                    ).rotation,
                    head
                ));
            }

            // rotation * scale * beforeTranslation
            void AddBox(int boneIndex, TSRBox box)
            {
                var m = box.ToMatrix();

                //  v6+---+v7
                // v2/| v3|
                //  +---+ |
                //  | +v5-+v4
                //  |/  |/
                //  +---+
                // v1   v0
                var s = 0.5f;
                var cube = new Vector3[]
                {
                    m.MultiplyPoint(new Vector3(+s, -s, -s)),
                    m.MultiplyPoint(new Vector3(-s, -s, -s)),
                    m.MultiplyPoint(new Vector3(-s, +s, -s)),
                    m.MultiplyPoint(new Vector3(+s, +s, -s)),
                    m.MultiplyPoint(new Vector3(+s, -s, +s)),
                    m.MultiplyPoint(new Vector3(-s, -s, +s)),
                    m.MultiplyPoint(new Vector3(-s, +s, +s)),
                    m.MultiplyPoint(new Vector3(+s, +s, +s)),
                };

                AddQuad(boneIndex, cube[0], cube[1], cube[2], cube[3]);
                AddQuad(boneIndex, cube[1], cube[5], cube[6], cube[2]);
                AddQuad(boneIndex, cube[5], cube[4], cube[7], cube[6]);
                AddQuad(boneIndex, cube[4], cube[0], cube[3], cube[7]);
                AddQuad(boneIndex, cube[3], cube[2], cube[6], cube[7]);
                AddQuad(boneIndex, cube[4], cube[5], cube[1], cube[0]);
            }

            void AddQuad(int boneIndex, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
            {
                var i = m_positions.Count;
                m_positions.Add(v0);
                m_positions.Add(v1);
                m_positions.Add(v2);
                m_positions.Add(v3);

                var bw = new BoneWeight
                {
                    boneIndex0 = boneIndex,
                    weight0 = 1.0f,
                };
                m_boneWeights.Add(bw);
                m_boneWeights.Add(bw);
                m_boneWeights.Add(bw);
                m_boneWeights.Add(bw);

                {
                    m_indices.Add(i);
                    m_indices.Add(i + 1);
                    m_indices.Add(i + 2);

                    m_indices.Add(i + 2);
                    m_indices.Add(i + 3);
                    m_indices.Add(i);
                }
            }

            public Mesh CreateMesh()
            {
                var mesh = new Mesh();
                mesh.SetVertices(m_positions);
                mesh.boneWeights = m_boneWeights.ToArray();
                mesh.triangles = m_indices.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }

        struct BoneHeadTail
        {
            public HumanBodyBones Head;
            public HumanBodyBones Tail;
            public Vector3 TailOffset;
            public float Width;
            public float Height;
            public Vector3 XAxis;

            public BoneHeadTail(HumanBodyBones head, HumanBodyBones tail, Vector3 xAxis, float width = 0.05f, float height = 0.05f)
            {
                Head = head;
                Tail = tail;
                TailOffset = Vector3.zero;
                XAxis = xAxis;
                Width = width;
                Height = height;
            }

            public BoneHeadTail(HumanBodyBones head, Vector3 tailOffset, Vector3 xAxis, float width = 0.05f, float height = 0.05f)
            {
                Head = head;
                Tail = HumanBodyBones.LastBone;
                TailOffset = tailOffset;
                XAxis = xAxis;
                Width = width;
                Height = height;
            }
        }

        static BoneHeadTail[] Bones = new BoneHeadTail[]
        {
            new BoneHeadTail(HumanBodyBones.Hips, HumanBodyBones.Spine, Vector3.right, 0.2f, 0.12f),
            new BoneHeadTail(HumanBodyBones.Spine, HumanBodyBones.Chest, Vector3.right),
            new BoneHeadTail(HumanBodyBones.Chest, HumanBodyBones.Neck, Vector3.right, 0.2f, 0.12f),
            new BoneHeadTail(HumanBodyBones.Neck, HumanBodyBones.Head, Vector3.right, 0.06f, 0.06f),
            new BoneHeadTail(HumanBodyBones.Head, new Vector3(0, 0.2f, 0), Vector3.right, 0.2f, 0.2f),
            // new BoneHeadTail(HumanBodyBones.Head, new Vector3(0, 0, 0.1f), Vector3.right, 0.2f, 0.2f),

            new BoneHeadTail(HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, Vector3.forward),
            new BoneHeadTail(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, Vector3.forward),
            new BoneHeadTail(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, Vector3.forward),

            new BoneHeadTail(HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal, Vector3.forward, 0.05f, 0.02f),
            new BoneHeadTail(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftThumbDistal, new Vector3(-0.03f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftIndexDistal, new Vector3(-0.03f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftMiddleDistal, new Vector3(-0.032f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftRingDistal, new Vector3(-0.028f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.LeftLittleDistal, new Vector3(-0.025f, 0, 0), Vector3.forward, 0.01f, 0.01f),

            new BoneHeadTail(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, Vector3.left),
            new BoneHeadTail(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, Vector3.left),
            new BoneHeadTail(HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes, Vector3.left),
            new BoneHeadTail(HumanBodyBones.LeftToes, new Vector3(0, 0, 0.03f), Vector3.left),

            new BoneHeadTail(HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, Vector3.back),
            new BoneHeadTail(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, Vector3.back),
            new BoneHeadTail(HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, Vector3.back),

            new BoneHeadTail(HumanBodyBones.RightHand, new Vector3(0.1f, 0, 0), Vector3.back, 0.05f, 0.02f),
            new BoneHeadTail(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightThumbDistal, new Vector3(0.03f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightIndexDistal, new Vector3(0.03f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightMiddleDistal, new Vector3(0.032f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightRingDistal, new Vector3(0.028f, 0, 0), Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal, Vector3.forward, 0.01f, 0.01f),
            new BoneHeadTail(HumanBodyBones.RightLittleDistal, new Vector3(0.025f, 0, 0), Vector3.forward, 0.01f, 0.01f),

            new BoneHeadTail(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, Vector3.left),
            new BoneHeadTail(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, Vector3.left),
            new BoneHeadTail(HumanBodyBones.RightFoot, HumanBodyBones.RightToes, Vector3.left),
            new BoneHeadTail(HumanBodyBones.RightToes, new Vector3(0, 0, 0.03f), Vector3.left),


        };

        public static SkinnedMeshRenderer CreateRenderer(Animator animator)
        {
            var bones = animator.transform.Traverse().ToList();

            var builder = new MeshBuilder();
            foreach (var headTail in Bones)
            {
                var head = animator.GetBoneTransform(headTail.Head);
                if (head != null)
                {
                    Transform tail = null;
                    if (headTail.Tail != HumanBodyBones.LastBone)
                    {
                        tail = animator.GetBoneTransform(headTail.Tail);
                    }

                    if (tail != null)
                    {
                        builder.AddBone(head.position, tail.position, bones.IndexOf(head), headTail.Width, headTail.Height, headTail.XAxis);
                    }
                    else
                    {
                        builder.AddBone(head.position, head.position + headTail.TailOffset, bones.IndexOf(head), headTail.Width, headTail.Height, headTail.XAxis);
                    }
                }
                else
                {
                    if (Application.isEditor)
                    {
                        // UniGLTFLogger.Warning($"{headTail.Head} not found");
                    }
                }
            }

            var mesh = builder.CreateMesh();
            mesh.name = "box-man";
            mesh.bindposes = bones.Select(x => x.worldToLocalMatrix * animator.transform.localToWorldMatrix).ToArray();

            var renderer = animator.gameObject.AddComponent<SkinnedMeshRenderer>();
            renderer.bones = bones.ToArray();
            renderer.rootBone = animator.GetBoneTransform(HumanBodyBones.Hips);
            renderer.sharedMesh = mesh;
            //var bounds = new Bounds(Vector3.zero, mesh.bounds.size);
            //renderer.localBounds = bounds;
            return renderer;
        }
    }
}