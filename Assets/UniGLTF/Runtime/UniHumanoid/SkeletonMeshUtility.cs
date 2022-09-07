using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UniHumanoid
{
    public static class SkeletonMeshUtility
    {
        class MeshBuilder
        {
            List<Vector3> m_positions = new List<Vector3>();
            List<int> m_indices = new List<int>();
            List<BoneWeight> m_boneWeights = new List<BoneWeight>();

            public void AddBone(Vector3 head, Vector3 tail, int boneIndex, float xWidth, float zWidth)
            {
                var dir = (tail - head).normalized;
                Vector3 xaxis;
                Vector3 zaxis;
                if (Vector3.Dot(dir, Vector3.forward) >= 1.0f - float.Epsilon)
                {
                    xaxis = Vector3.right;
                    zaxis = Vector3.down;
                }
                else
                {
                    xaxis = Vector3.Cross(dir, Vector3.forward).normalized;
                    zaxis = Vector3.forward;
                }
                AddBox((head+tail)*0.5f, 
                    xaxis*xWidth, 
                    (tail-head)*0.5f, 
                    zaxis*zWidth, 
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

            void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int boneIndex, bool reverse=false)
            {
                var i = m_positions.Count;
                m_positions.Add(v0);
                m_positions.Add(v1);
                m_positions.Add(v2);
                m_positions.Add(v3);

                var bw = new BoneWeight
                {
                    boneIndex0=boneIndex,
                    weight0=1.0f,
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
            public float XWidth;
            public float ZWidth;

            public BoneHeadTail(HumanBodyBones head, HumanBodyBones tail, float xWidth = 0.05f, float zWidth = 0.05f)
            {
                Head = head;
                Tail = tail;
                TailOffset = Vector3.zero;
                XWidth = xWidth;
                ZWidth = zWidth;
            }

            public BoneHeadTail(HumanBodyBones head, Vector3 tailOffset, float xWidth = 0.05f, float zWidth = 0.05f)
            {
                Head = head;
                Tail = HumanBodyBones.LastBone;
                TailOffset = tailOffset;
                XWidth = xWidth;
                ZWidth = zWidth;
            }
        }

        static BoneHeadTail[] Bones = new BoneHeadTail[]
        {
            new BoneHeadTail(HumanBodyBones.Hips, HumanBodyBones.Spine, 0.1f, 0.06f),
            new BoneHeadTail(HumanBodyBones.Spine, HumanBodyBones.Chest),
            new BoneHeadTail(HumanBodyBones.Chest, HumanBodyBones.Neck, 0.1f, 0.06f),
            new BoneHeadTail(HumanBodyBones.Neck, HumanBodyBones.Head, 0.03f, 0.03f),
            new BoneHeadTail(HumanBodyBones.Head, new Vector3(0, 0.1f, 0), 0.1f, 0.1f),

            new BoneHeadTail(HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm),
            new BoneHeadTail(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
            new BoneHeadTail(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),
            new BoneHeadTail(HumanBodyBones.LeftHand, new Vector3(-0.1f, 0, 0)),

            new BoneHeadTail(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
            new BoneHeadTail(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot),
            new BoneHeadTail(HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes),
            new BoneHeadTail(HumanBodyBones.LeftToes, new Vector3(0, 0, 0.1f)),

            new BoneHeadTail(HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm),
            new BoneHeadTail(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
            new BoneHeadTail(HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),
            new BoneHeadTail(HumanBodyBones.RightHand, new Vector3(0.1f, 0, 0)),

            new BoneHeadTail(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
            new BoneHeadTail(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot),
            new BoneHeadTail(HumanBodyBones.RightFoot, HumanBodyBones.RightToes),
            new BoneHeadTail(HumanBodyBones.RightToes, new Vector3(0, 0, 0.1f)),
        };

        public static SkinnedMeshRenderer CreateRenderer(Animator animator)
        {
            var bones = animator.transform.Traverse().ToList();

            var builder = new MeshBuilder();
            foreach(var headTail in Bones)
            {
                var head = animator.GetBoneTransform(headTail.Head);
                if (head!=null)
                {
                    Transform tail = null;
                    if(headTail.Tail!= HumanBodyBones.LastBone)
                    {
                        tail = animator.GetBoneTransform(headTail.Tail);
                    }

                    if (tail != null)
                    {
                        builder.AddBone(head.position, tail.position, bones.IndexOf(head), headTail.XWidth, headTail.ZWidth);
                    }
                    else
                    {
                        builder.AddBone(head.position, head.position + headTail.TailOffset, bones.IndexOf(head), headTail.XWidth, headTail.ZWidth);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("{0} not found", headTail.Head);
                }
            }

            var mesh = builder.CreateMesh();
            mesh.name = "box-man";
            mesh.bindposes = bones.Select(x =>
                            x.worldToLocalMatrix * animator.transform.localToWorldMatrix).ToArray();
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
