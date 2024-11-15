using System;
using System.Collections.Generic;
using RotateParticle.Components;
using UnityEngine;


namespace RotateParticle
{
    [Serializable]
    public class ParticleList
    {
        public List<RotateParticle> _particles = new();
        public List<Transform> _particleTransforms = new();

        public Strand MakeParticleStrand(SimulationEnv env, Warp warp, CollisionGroupMask mask)
        {
            var strand = new Strand(mask);

            // root kinematic
            var particle_index = _MakeAParticle(null, env, warp.transform, 0, 0, strand.CollisionMask);
            var joint = _particles[particle_index];
            strand.Particles.Add(joint);

            foreach (var particle in warp.Particles)
            {
                var child_index = _MakeAParticle(joint, env, particle.Transform,
                    particle.GetSettings(warp.BaseSettings).HitRadius, 1, strand.CollisionMask);
                var child = _particles[child_index];
                strand.Particles.Add(child);
                joint.Children.Add(child);
                joint = child;
            }

            return strand;
        }

        int _MakeAParticle(RotateParticle parent, SimulationEnv env, Transform t, float radius, float mass, CollisionGroupMask collisionMask)
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