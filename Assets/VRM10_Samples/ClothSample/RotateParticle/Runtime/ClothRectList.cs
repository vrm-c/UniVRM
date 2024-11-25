using System.Collections.Generic;
using System.Linq;
using RotateParticle.Components;
using SphereTriangle;
using UnityEngine;
using UniVRM10;

namespace RotateParticle
{
    class ClothRectList
    {
        readonly List<Transform> _particles;

        public List<(SpringConstraint, ClothRect)> List = new();
        public readonly bool[] ClothUsedParticles;

        public ClothRectList(List<Transform> particles, Vrm10Instance vrm)
        {
            _particles = particles;
            ClothUsedParticles = new bool[_particles.Count];

            var cloths = vrm.GetComponentsInChildren<RectCloth>();
            foreach (var cloth in cloths)
            {
                AddCloth(cloth, vrm);
            }
        }

        void AddCloth(RectCloth cloth, Vrm10Instance vrm)
        {
            for (int i = 1; i < cloth.Warps.Count; ++i)
            {
                var s0 = cloth.Warps[i - 1];
                var s1 = cloth.Warps[i];
                for (int j = 0; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                {
                    // d x x c
                    //   | |
                    // a x-x b
                    var a = s0.Particles[j].Transform;
                    var b = s1.Particles[j].Transform;
                    var c = j == 0 ? s1.transform : s1.Particles[j - 1].Transform;
                    var d = j == 0 ? s0.transform : s0.Particles[j - 1].Transform;
                    ClothUsedParticles[_particles.IndexOf(a)] = true;
                    ClothUsedParticles[_particles.IndexOf(b)] = true;
                    ClothUsedParticles[_particles.IndexOf(c)] = true;
                    ClothUsedParticles[_particles.IndexOf(d)] = true;
                    if (i % 2 == 1)
                    {
                        // 互い違いに
                        // abcd to badc
                        (a, b) = (b, a);
                        (c, d) = (d, c);
                    }
                    List.Add((
                        new SpringConstraint(
                            _particles.IndexOf(a),
                            _particles.IndexOf(b),
                            Vector3.Distance(
                                vrm.DefaultTransformStates[a].Position,
                                vrm.DefaultTransformStates[b].Position)),
                        new ClothRect(
                            _particles.IndexOf(a),
                            _particles.IndexOf(b),
                            _particles.IndexOf(c),
                            _particles.IndexOf(d))));
                }
            }

            if (cloth.Warps.Count >= 3 && cloth.LoopIsClosed)
            {
                // close loop
                var i = cloth.Warps.Count;
                var s0 = cloth.Warps.Last();
                var s1 = cloth.Warps.First();
                for (int j = 0; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                {
                    var a = s0.Particles[j].Transform;
                    var b = s1.Particles[j].Transform;
                    var c = j == 0 ? s1.transform : s1.Particles[j - 1].Transform;
                    var d = j == 0 ? s0.transform : s0.Particles[j - 1].Transform;
                    ClothUsedParticles[_particles.IndexOf(a)] = true;
                    ClothUsedParticles[_particles.IndexOf(b)] = true;
                    ClothUsedParticles[_particles.IndexOf(c)] = true;
                    ClothUsedParticles[_particles.IndexOf(d)] = true;
                    if (i % 2 == 1)
                    {
                        // 互い違いに
                        // abcd to badc
                        (a, b) = (b, a);
                        (c, d) = (d, c);
                    }
                    List.Add((
                        new SpringConstraint(
                            _particles.IndexOf(a),
                            _particles.IndexOf(b),
                            Vector3.Distance(
                                vrm.DefaultTransformStates[a].Position,
                                vrm.DefaultTransformStates[b].Position)
                            ),
                        new ClothRect(
                            _particles.IndexOf(a),
                            _particles.IndexOf(b),
                            _particles.IndexOf(c),
                            _particles.IndexOf(d)
                        )
                    ));
                }
            }
        }
    }

}