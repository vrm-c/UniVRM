using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VrmLib
{
#pragma warning disable 0649
    struct BoneWeight
    {
        public int boneIndex0;
        public int boneIndex1;
        public int boneIndex2;
        public int boneIndex3;

        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;
    }
#pragma warning restore

    class MeshBuilder
    {
        public static BoneHeadTail[] Bones = new BoneHeadTail[]
        {
            new BoneHeadTail(HumanoidBones.hips, HumanoidBones.spine, 0.1f, 0.06f),
            new BoneHeadTail(HumanoidBones.spine, HumanoidBones.chest),
            new BoneHeadTail(HumanoidBones.chest, HumanoidBones.neck, 0.1f, 0.06f),
            new BoneHeadTail(HumanoidBones.neck, HumanoidBones.head, 0.03f, 0.03f),
            new BoneHeadTail(HumanoidBones.head, new Vector3(0, 0.1f, 0), 0.1f, 0.1f),

            new BoneHeadTail(HumanoidBones.leftShoulder, HumanoidBones.leftUpperArm),
            new BoneHeadTail(HumanoidBones.leftUpperArm, HumanoidBones.leftLowerArm),
            new BoneHeadTail(HumanoidBones.leftLowerArm, HumanoidBones.leftHand),
            new BoneHeadTail(HumanoidBones.leftHand, new Vector3(-0.1f, 0, 0)),

            new BoneHeadTail(HumanoidBones.leftUpperLeg, HumanoidBones.leftLowerLeg),
            new BoneHeadTail(HumanoidBones.leftLowerLeg, HumanoidBones.leftFoot),
            new BoneHeadTail(HumanoidBones.leftFoot, HumanoidBones.leftToes),
            new BoneHeadTail(HumanoidBones.leftToes, new Vector3(0, 0, 0.1f)),

            new BoneHeadTail(HumanoidBones.rightShoulder, HumanoidBones.rightUpperArm),
            new BoneHeadTail(HumanoidBones.rightUpperArm, HumanoidBones.rightLowerArm),
            new BoneHeadTail(HumanoidBones.rightLowerArm, HumanoidBones.rightHand),
            new BoneHeadTail(HumanoidBones.rightHand, new Vector3(0.1f, 0, 0)),

            new BoneHeadTail(HumanoidBones.rightUpperLeg, HumanoidBones.rightLowerLeg),
            new BoneHeadTail(HumanoidBones.rightLowerLeg, HumanoidBones.rightFoot),
            new BoneHeadTail(HumanoidBones.rightFoot, HumanoidBones.rightToes),
            new BoneHeadTail(HumanoidBones.rightToes, new Vector3(0, 0, 0.1f)),
        };

        public void Build(List<Node> bones)
        {
            foreach (var headTail in Bones)
            {
                var head = bones.FirstOrDefault(x => x.HumanoidBone == headTail.Head);
                if (head != null)
                {
                    Node tail = default(Node);
                    if (headTail.Tail != HumanoidBones.unknown)
                    {
                        tail = bones.FirstOrDefault(x => x.HumanoidBone == headTail.Tail);
                    }

                    if (tail != null)
                    {
                        AddBone(head.SkeletonLocalPosition, tail.SkeletonLocalPosition, bones.IndexOf(head), headTail.XWidth, headTail.ZWidth);
                    }
                    else if (headTail.TailOffset != Vector3.zero)
                    {
                        AddBone(head.SkeletonLocalPosition, head.SkeletonLocalPosition + headTail.TailOffset, bones.IndexOf(head), headTail.XWidth, headTail.ZWidth);
                    }
                }
                else
                {
                    Console.Error.WriteLine($"{headTail.Head} not found");
                }
            }
        }

        List<Vector3> m_positioins = new List<Vector3>();
        List<int> m_indices = new List<int>();
        List<BoneWeight> m_boneWeights = new List<BoneWeight>();

        void AddBone(Vector3 head, Vector3 tail, int boneIndex, float xWidth, float zWidth)
        {
            var yaxis = (tail - head).normalized;
            Vector3 xaxis;
            Vector3 zaxis;
            if (Vector3.Dot(yaxis, Vector3.forward) >= 1.0f - float.Epsilon)
            {
                // ほぼZ軸
                xaxis = Vector3.right;
                zaxis = -Vector3.up;
            }
            else
            {
                xaxis = Vector3.Normalize(Vector3.Cross(yaxis, Vector3.forward));
                zaxis = Vector3.forward;
            }
            AddBox((head + tail) * 0.5f,
                xaxis * xWidth,
                (tail - head) * 0.5f,
                zaxis * zWidth,
                boneIndex);
        }

        void AddBox(Vector3 center, Vector3 xaxis, Vector3 yaxis, Vector3 zaxis, int boneIndex)
        {
            AddQuad(
                center - yaxis - xaxis - zaxis,
                center - yaxis + xaxis - zaxis,
                center - yaxis + xaxis + zaxis,
                center - yaxis - xaxis + zaxis,
                boneIndex);
            AddQuad(
                center + yaxis - xaxis - zaxis,
                center + yaxis + xaxis - zaxis,
                center + yaxis + xaxis + zaxis,
                center + yaxis - xaxis + zaxis,
                boneIndex, true);
            AddQuad(
                center - xaxis - yaxis - zaxis,
                center - xaxis + yaxis - zaxis,
                center - xaxis + yaxis + zaxis,
                center - xaxis - yaxis + zaxis,
                boneIndex, true);
            AddQuad(
                center + xaxis - yaxis - zaxis,
                center + xaxis + yaxis - zaxis,
                center + xaxis + yaxis + zaxis,
                center + xaxis - yaxis + zaxis,
                boneIndex);
            AddQuad(
                center - zaxis - xaxis - yaxis,
                center - zaxis + xaxis - yaxis,
                center - zaxis + xaxis + yaxis,
                center - zaxis - xaxis + yaxis,
                boneIndex, true);
            AddQuad(
                center + zaxis - xaxis - yaxis,
                center + zaxis + xaxis - yaxis,
                center + zaxis + xaxis + yaxis,
                center + zaxis - xaxis + yaxis,
                boneIndex);
        }

        void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int boneIndex, bool reverse = false)
        {
            var i = m_positioins.Count;
            if (float.IsNaN(v0.x) || float.IsNaN(v0.y) || float.IsNaN(v0.z))
            {
                throw new Exception();
            }
            m_positioins.Add(v0);

            if (float.IsNaN(v1.x) || float.IsNaN(v1.y) || float.IsNaN(v1.z))
            {
                throw new Exception();
            }
            m_positioins.Add(v1);

            if (float.IsNaN(v2.x) || float.IsNaN(v2.y) || float.IsNaN(v2.z))
            {
                throw new Exception();
            }
            m_positioins.Add(v2);

            if (float.IsNaN(v3.x) || float.IsNaN(v3.y) || float.IsNaN(v3.z))
            {
                throw new Exception();
            }
            m_positioins.Add(v3);

            var bw = new BoneWeight
            {
                boneIndex0 = boneIndex,
                weight0 = 1.0f,
            };
            m_boneWeights.Add(bw);
            m_boneWeights.Add(bw);
            m_boneWeights.Add(bw);
            m_boneWeights.Add(bw);

            if (reverse)
            {
                m_indices.Add(i + 3);
                m_indices.Add(i + 2);
                m_indices.Add(i + 1);

                m_indices.Add(i + 1);
                m_indices.Add(i);
                m_indices.Add(i + 3);
            }
            else
            {
                m_indices.Add(i);
                m_indices.Add(i + 1);
                m_indices.Add(i + 2);

                m_indices.Add(i + 2);
                m_indices.Add(i + 3);
                m_indices.Add(i);
            }
        }
    }

    struct BoneHeadTail
    {
        public HumanoidBones Head;
        public HumanoidBones Tail;
        public Vector3 TailOffset;
        public float XWidth;
        public float ZWidth;

        public BoneHeadTail(HumanoidBones head, HumanoidBones tail, float xWidth = 0.05f, float zWidth = 0.05f)
        {
            Head = head;
            Tail = tail;
            TailOffset = Vector3.zero;
            XWidth = xWidth;
            ZWidth = zWidth;
        }

        public BoneHeadTail(HumanoidBones head, Vector3 tailOffset, float xWidth = 0.05f, float zWidth = 0.05f)
        {
            Head = head;
            Tail = HumanoidBones.unknown;
            TailOffset = tailOffset;
            XWidth = xWidth;
            ZWidth = zWidth;
        }
    }
}
