using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


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
        public Vector3 LocalTranslationWithoutUpdate = Vector3.Zero;
        public Vector3 LocalTranslation
        {
            get => LocalTranslationWithoutUpdate;
            set
            {
                if (LocalTranslationWithoutUpdate == value) return;
                LocalTranslationWithoutUpdate = value;
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.Identity);
            }
        }

        public Quaternion LocalRotationWithoutUpdate = Quaternion.Identity;
        public Quaternion LocalRotation
        {
            get => LocalRotationWithoutUpdate;
            set
            {
                if (LocalRotationWithoutUpdate == value) return;
                LocalRotationWithoutUpdate = value;
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.Identity);
            }
        }

        public Vector3 LocalScalingWithoutUpdate = Vector3.One;
        public Vector3 LocalScaling
        {
            get => LocalScalingWithoutUpdate;
            set
            {
                if (LocalScalingWithoutUpdate == value) return;
                LocalScalingWithoutUpdate = value;
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.Identity);
            }
        }

        public Matrix4x4 LocalMatrix
        {
            get => Matrix4x4.CreateScale(LocalScaling)
            * Matrix4x4.CreateFromQuaternion(LocalRotation)
            * Matrix4x4.CreateTranslation(LocalTranslation);
        }

        public void SetLocalMatrix(Matrix4x4 value, bool calcWorldMatrix)
        {
            if (Matrix4x4.Decompose(value, out LocalScalingWithoutUpdate, out LocalRotationWithoutUpdate, out LocalTranslationWithoutUpdate))
            {
                CalcWorldMatrix(Parent != null ? Parent.Matrix : Matrix4x4.Identity, calcWorldMatrix);
            }
            else
            {
                throw new Exception($"fail to decompose matrix: {Name}");
            }
        }

        Matrix4x4 m_matrix = Matrix4x4.Identity;
        public Matrix4x4 Matrix
        {
            get => m_matrix;
        }
        // public void SetMatrixWithoutUpdate(Matrix4x4 m)
        // {
        //     m_matrix = m;
        // }

        public Quaternion Rotation
        {
            get
            {
                return Quaternion.CreateFromRotationMatrix(Matrix);
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
                SetLocalMatrix(m * Parent.InverseMatrix, calcWorldMatrix);
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
                Matrix4x4 inverted = Matrix4x4.Identity;
                if (!Matrix4x4.Invert(Matrix, out inverted))
                {
                    throw new Exception();
                }
                return inverted;
            }
        }

        public void CalcWorldMatrix(bool calcChildren = true)
        {
            if (Parent == null)
            {
                CalcWorldMatrix(Matrix4x4.Identity, calcChildren);
            }
            else
            {
                CalcWorldMatrix(Parent.Matrix, calcChildren);
            }
        }

        public void CalcWorldMatrix(Matrix4x4 parent, bool calcChildren = true)
        {
            var value = LocalMatrix * parent;
            // if (value == m_matrix) return;
            m_matrix = value;

            RaiseMatrixUpdated();

            // if (float.IsNaN(m_matrix.M11))
            // {
            //     var a = 0;
            // }
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
            get => Matrix.Translation;
            set
            {
                if (Parent == null)
                {
                    LocalTranslation = value;
                }
                else
                {
                    var localPosition = Vector4.Transform(value, Parent.InverseMatrix);
                    LocalTranslation = new Vector3(localPosition.X, localPosition.Y, localPosition.Z);
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
            var src = Vector3.Transform(Vector3.Normalize(worldSrc), Quaternion.Inverse(Rotation));
            var dst = Vector3.Transform(Vector3.Normalize(worldDst), Quaternion.Inverse(Rotation));

            var dot = Vector3.Dot(src, dst);
            Quaternion rot;
            if (Math.Abs(1.0f - dot) < float.Epsilon)
            {
                // 0degree
                rot = Quaternion.Identity;
            }
            else if (Math.Abs(-1.0f - dot) < float.Epsilon)
            {
                // 180degree
                rot = Quaternion.CreateFromYawPitchRoll(MathFWrap.PI, 0, 0);
            }
            else
            {
                var axis = Vector3.Normalize(Vector3.Cross(src, dst));
                rot = Quaternion.CreateFromAxisAngle(axis, (float)Math.Acos(dot));
            }

            LocalRotation = rot;
        }

        public override string ToString()
        {
            if (HumanoidBone.HasValue)
            {
                return $"{Name}[{HumanoidBone.Value}]: {LocalTranslation.X:0.00}, {LocalTranslation.Y:0.00}, {LocalTranslation.Z:0.00}";
            }
            else
            {
                return $"{Name}: {LocalTranslation.X:0.00}, {LocalTranslation.Y:0.00}, {LocalTranslation.Z:0.00}";
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
            var sum = Vector3.Zero;
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