using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniGLTF;

namespace VrmLib
{
    public enum ChildMatrixMode
    {
        KeepLocal,
        KeepWorld,
    }

    public class Node : GltfId, IEnumerable<Node>
    {
        static int s_nextUniqueId = 1;

        public readonly int UniqueID;

        public string Name
        {
            get;
            set;
        }

        public Node(string name)
        {
            UniqueID = s_nextUniqueId++;
            Name = name;
        }

        #region Transform
        //
        // Localで値を保持する
        //
        public Vector3 LocalTranslationWithoutUpdate = Vector3.zero;
        public Vector3 LocalTranslation
        {
            get => LocalTranslationWithoutUpdate;
            set
            {
                if (LocalTranslationWithoutUpdate == value) return;
                LocalTranslationWithoutUpdate = value;
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.identity);
            }
        }

        public Quaternion LocalRotationWithoutUpdate = Quaternion.identity;
        public Quaternion LocalRotation
        {
            get => LocalRotationWithoutUpdate;
            set
            {
                if (LocalRotationWithoutUpdate == value) return;
                LocalRotationWithoutUpdate = value;
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.identity);
            }
        }

        public Vector3 LocalScalingWithoutUpdate = Vector3.one;
        public Vector3 LocalScaling
        {
            get => LocalScalingWithoutUpdate;
            set
            {
                if (LocalScalingWithoutUpdate == value) return;
                LocalScalingWithoutUpdate = value;
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.identity);
            }
        }

        public Matrix4x4 LocalMatrix
        {
            get => Matrix4x4.Translate(LocalTranslation)
            * Matrix4x4.Rotate(LocalRotation)
            * Matrix4x4.Scale(LocalScaling)
            ;
        }

        public void SetLocalMatrix(Matrix4x4 value, bool calcWorldMatrix)
        {
            (LocalTranslationWithoutUpdate, LocalRotationWithoutUpdate, LocalScalingWithoutUpdate) = value.Decompose();
            CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.identity, calcWorldMatrix);
        }

        Matrix4x4 m_matrix = Matrix4x4.identity;
        public Matrix4x4 Matrix
        {
            get => m_matrix;
        }

        public Quaternion Rotation
        {
            get
            {
                return Matrix.rotation;
            }
            set
            {
                if (Parent == null)
                {
                    LocalRotation = value;
                }
                else
                {
                    LocalRotation = Quaternion.Inverse(Parent.Rotation) * value;
                }
            }
        }

        public void SetMatrix(Matrix4x4 m, bool calcWorldMatrix)
        {
            if (Parent != null)
            {
                SetLocalMatrix(Parent.InverseMatrix * m, calcWorldMatrix);
            }
            else
            {
                SetLocalMatrix(m, calcWorldMatrix);
            }
        }

        public Matrix4x4 InverseMatrix
        {
            get
            {
                return Matrix.inverse;
            }
        }

        public void CalcWorldMatrix(bool calcChildren = true)
        {
            if (Parent == null)
            {
                CalcWorldMatrix(Matrix4x4.identity, calcChildren);
            }
            else
            {
                CalcWorldMatrix(Parent.Matrix, calcChildren);
            }
        }

        public void CalcWorldMatrix(Matrix4x4 parent, bool calcChildren = true)
        {
            var value = parent * LocalMatrix;
            m_matrix = value;

            RaiseMatrixUpdated();

            if (calcChildren)
            {
                foreach (var child in Children)
                {
                    child.CalcWorldMatrix(m_matrix, calcChildren);
                }
            }
        }

        public event Action<Matrix4x4> MatrixUpdated;
        void RaiseMatrixUpdated()
        {
            var handle = MatrixUpdated;
            if (handle != null)
            {
                handle(Matrix);
            }
        }

        public Vector3 Translation
        {
            get => Matrix.GetColumn(3);
            set
            {
                if (Parent == null)
                {
                    LocalTranslation = value;
                }
                else
                {
                    LocalTranslation = Parent.InverseMatrix.MultiplyPoint(value);
                }
            }
        }

        public Vector3 SkeletonLocalPosition
        {
            get => Translation;
            set
            {
                Translation = value;
            }
        }
        #endregion

        #region Hierarchy

        public Node Parent { get; private set; }

        public IEnumerable<Node> Ancestors()
        {
            if (Parent == null)
            {
                yield break;
            }
            yield return Parent;
            foreach (var x in Parent.Ancestors())
            {
                yield return x;
            }
        }

        readonly List<Node> m_children = new List<Node>();

        public void Add(Node child, ChildMatrixMode mode = ChildMatrixMode.KeepLocal)
        {
            if (child.Parent != null)
            {
                child.Parent.m_children.Remove(child);
            }
            m_children.Add(child);
            child.Parent = this;

            switch (mode)
            {
                case ChildMatrixMode.KeepLocal:
                    child.CalcWorldMatrix(Matrix);
                    break;

                case ChildMatrixMode.KeepWorld:
                    child.SetMatrix(child.Matrix, true);
                    break;
            }
        }

        public void Remove(Node child)
        {
            child.Parent = null;
            m_children.Remove(child);
        }

        public IEnumerable<Node> Traverse()
        {
            yield return this;

            foreach (var child in Children)
            {
                foreach (var x in child.Traverse())
                {
                    yield return x;
                }
            }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return ((IEnumerable<Node>)Children).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Node>)Children).GetEnumerator();
        }
        #endregion


        public MeshGroup MeshGroup;

        // VRMでは、Meshes.Count==1
        public Mesh Mesh => MeshGroup?.Meshes?[0];

        HumanoidBones? m_bone;
        public HumanoidBones? HumanoidBone
        {
            get => m_bone;
            set
            {
                if (m_bone == value)
                {
                    return;
                }
                if (value == HumanoidBones.unknown)
                {
                    return;
                }
                m_bone = value;
            }
        }

        public IReadOnlyList<Node> Children => m_children;

        public Node FindBone(HumanoidBones bone)
        {
            return Traverse().FirstOrDefault(x => x.HumanoidBone == bone);
        }

        public void RotateFromTo(Vector3 worldSrc, Vector3 worldDst)
        {
            // world to local
            var src = Quaternion.Inverse(Rotation) * worldSrc.normalized;
            var dst = Quaternion.Inverse(Rotation) * worldDst.normalized;

            var dot = Vector3.Dot(src, dst);
            Quaternion rot;
            if (Math.Abs(1.0f - dot) < float.Epsilon)
            {
                // 0degree
                rot = Quaternion.identity;
            }
            else if (Math.Abs(-1.0f - dot) < float.Epsilon)
            {
                // 180degree
                rot = Quaternion.Euler(0, MathFWrap.PI, 0);
            }
            else
            {
                var axis = Vector3.Normalize(Vector3.Cross(src, dst));
                rot = Quaternion.AngleAxis((float)Math.Acos(dot), axis);
            }

            LocalRotation = rot;
        }

        public override string ToString()
        {
            if (HumanoidBone.HasValue)
            {
                return $"{Name}[{HumanoidBone.Value}]: {LocalTranslation.x:0.00}, {LocalTranslation.y:0.00}, {LocalTranslation.z:0.00}";
            }
            else
            {
                return $"{Name}: {LocalTranslation.x:0.00}, {LocalTranslation.y:0.00}, {LocalTranslation.z:0.00}";
            }
        }
    }

    public static class NodeExtensions
    {
        public static IEnumerable<Node> Traverse(this Node self)
        {
            yield return self;
            foreach (var child in self.Children)
            {
                foreach (var x in child.Traverse())
                {
                    yield return x;
                }
            }
        }

        public static Vector3 CenterOfDescendant(this Node self)
        {
            var sum = Vector3.zero;
            int i = 0;
            foreach (var x in self.Traverse())
            {
                sum += x.SkeletonLocalPosition;
                ++i;
            }
            return sum / i;
        }
    }
}