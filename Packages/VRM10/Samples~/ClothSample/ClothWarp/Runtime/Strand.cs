using System.Collections.Generic;
using SphereTriangle;
using UnityEngine;

namespace UniVRM10.ClothWarp
{
    public class Strand
    {
        public Strand()
        {
        }

        public readonly List<ClothWarpNode> Particles = new();

        public void UpdateRoot(IReadOnlyList<Transform> transforms, SphereTriangle.PositionList positions, Vector3[] restPositions)
        {
            var root = Particles[0];

            // only root affected
            var t = transforms[root.Init.Index];
            root.State.Update(t);
            positions.Init(root.Init.Index, root.Init.Mass, t.position);

            // Debug.Assert(root.Children.Count == 1);
            foreach (var child in root.Children)
            {
                _CalcRest(transforms, positions.Result, restPositions, root.Children[0]);
            }
        }

        void _CalcRest(IReadOnlyList<Transform> transforms, IReadOnlyList<Vector3> positions,
            Vector3[] restPositions,
            ClothWarpNode particle)
        {
            var restRotation = particle.RestRotation(transforms);


            if (particle.Parent != null)
            {
                // var localPosition = particle.Init.BoneAxis * Vector3.Distance(positions[particle.Parent.Init.Index], positions[particle.Init.Index]);
                restPositions[particle.Init.Index] = positions[particle.Parent.Init.Index] + restRotation * particle.Init.LocalPosition;
            }
            else
            {
                restPositions[particle.Init.Index] = restRotation * particle.Init.LocalPosition;
            }

            foreach (var child in particle.Children)
            {
                _CalcRest(transforms, positions, restPositions, child);
            }
        }

        public void ForceLength(IReadOnlyList<Transform> transforms, PositionList positions)
        {
            var root = Particles[0];
            for (int i = 0; i < root.Children.Count; ++i)
            {
                _ForceConstraint(transforms, positions, i, root.Children[i], positions.Get(root.Init.Index));
            }
        }

        void _ForceConstraint(IReadOnlyList<Transform> transforms, PositionList positions, int child_index, ClothWarpNode particle, in Vector3 parent)
        {
            // update position
            Vector3 newPosition;
            if (child_index == 0)
            {
                newPosition = parent + (positions.Get(particle.Init.Index) - parent).normalized * particle.Init.StrandLength;
            }
            else
            {
                // 枝分かれ。特別処理
                var firstSibling = particle.Parent.Children[0];
                var firstPosition = positions.Get(firstSibling.Init.Index);
                newPosition = firstPosition + transforms[particle.Parent.Init.Index].rotation * (particle.Init.LocalPosition - firstSibling.Init.LocalPosition);
            }
            positions.Init(particle.Init.Index, particle.Init.Mass, newPosition);

            for (int i = 0; i < particle.Children.Count; ++i)
            {
                _ForceConstraint(transforms, positions, i, particle.Children[i], newPosition);
            }
        }

        public void Reset(IReadOnlyList<Transform> transforms)
        {
            var root = Particles[0];
            _ResetRecursive(transforms, root);
        }

        void _ResetRecursive(IReadOnlyList<Transform> transforms,
            ClothWarpNode joint)
        {
            var t = transforms[joint.Init.Index];
            t.localPosition = joint.Init.LocalPosition;
            t.localRotation = joint.Init.LocalRotation;
            joint.State.Apply(t.position, zeroVelocity: true);
            for (int i = 0; i < joint.Children.Count; ++i)
            {
                _ResetRecursive(transforms, joint.Children[i]);
            }
        }

        public void Apply(IReadOnlyList<Transform> transforms, IReadOnlyList<Vector3> positions)
        {
            var root = Particles[0];
            for (int i = 0; i < root.Children.Count; ++i)
            {
                var child = root.Children[i];
                _ApplyRecursive(transforms, positions, i, child);
            }
        }

        void _ApplyRecursive(IReadOnlyList<Transform> transforms, IReadOnlyList<Vector3> positions,
            int child_index, ClothWarpNode joint)
        {
            var t = transforms[joint.Init.Index];

            if (child_index > 0)
            {
                // 枝分かれ。mass=0 にもしている。
                // self position
                t.position = positions[joint.Init.Index];
                joint.State.Apply(t.position);
            }
            else if (joint.Init.Mass > 0)
            {
                Debug.Assert(joint.Parent != null);
                var restRotation = joint.RestRotation(transforms);

                Quaternion aimRotation = Quaternion.FromToRotation(
                    // 初期状態
                    restRotation * joint.Init.BoneAxis,
                    // 現状
                    positions[joint.Init.Index] - positions[joint.Parent.Init.Index]);

                var r = aimRotation * restRotation;

                // parent rotation
                transforms[joint.Parent.Init.Index].rotation = r;

                // self position
                t.position = positions[joint.Init.Index];
                joint.State.Apply(t.position);
            }

            if (joint.Children.Count > 0)
            {
                for (int i = 0; i < joint.Children.Count; ++i)
                {
                    _ApplyRecursive(transforms, positions, i, joint.Children[i]);
                }
            }
            else
            {
                // tail
                t.rotation = t.parent.rotation * joint.Init.LocalRotation;
            }
        }
    }
}