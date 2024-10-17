using System;
using System.Collections.Generic;
using SphereTriangle;
using UnityEngine;


namespace RotateParticle
{
    [Serializable]
    public class ParticleList
    {
        public List<RotateParticle> _particles = new();
        public List<Transform> _particleTransforms = new();

        public Strand MakeParticleStrand(SimulationEnv env, Transform t, float radius, CollisionGroupMask mask)
        {
            var strand = new Strand(mask);
            _MakeParticlesRecursive(strand, env, 0, t, radius, null);
            return strand;
        }

        RotateParticle _MakeParticlesRecursive(Strand strand, SimulationEnv env, int child_index, Transform t, float radius, RotateParticle parent)
        {
            var mass = parent == null ? 0 : 1;
            if (child_index > 0)
            {
                // 枝分かれ動かない
                mass = 0;
            }

            var particle_index = _MakeAParticle(env, t, radius, parent, mass, strand.CollisionMask);
            var joint = _particles[particle_index];
            strand.Particles.Add(joint);
            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild(i);
                var child_joint = _MakeParticlesRecursive(strand, env, i, child, radius, joint);
                joint.Children.Add(child_joint);
            }
            return joint;
        }

        int _MakeAParticle(SimulationEnv env, Transform t, float radius, RotateParticle parent, float mass, CollisionGroupMask collisionMask)
        {
            var index = _particles.Count;
            _particleTransforms.Add(t);
            _particles.Add(new RotateParticle(index, parent, env, t, radius, mass, collisionMask));
            return index;
        }

        public void EndInitialize(SphereTriangle.InitPosition initPos)
        {
            for (int i = 0; i < _particles.Count; ++i)
            {
                initPos(i, _particles[i].Init.Mass, _particleTransforms[i].position);
            }
        }

        public void BeginFrame(SimulationEnv env, FrameTime time, IReadOnlyList<Vector3> restPositions)
        {
            for (int i = 0; i < _particles.Count; ++i)
            {
                var p = _particles[i];
                if (p.Init.Mass <= 0)
                {
                    continue;
                }

                var rest = restPositions[p.Init.Index];
                p.BeginFrame(env, time, rest);
            }
        }

        public void Verlet(SimulationEnv env, FrameTime time, SphereTriangle.InitPosition initPos)
        {
            for (int i = 0; i < _particles.Count; ++i)
            {
                var p = _particles[i];
                if (p.Init.Mass > 0)
                {
                    var newPos = p.Verlet(env, time);
                    initPos(i, p.Init.Mass, newPos);
                }
            }
        }

        public void DrawGizmos()
        {
            foreach (var p in _particles)
            {
                p.OnDrawGizmos(_particleTransforms[p.Init.Index]);
            }
        }
    }
}