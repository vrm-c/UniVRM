using System;
using UnityEngine;

namespace UniVRM10.ClothWarp
{
    public class ParticleRuntimeState
    {
        public Vector3 Current;
        public Vector3 Prev;

        public ParticleRuntimeState(SimulationEnv env, Transform transform)
        {
            if (env.Center == null)
            {
                Prev = transform.position;
                Current = transform.position;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Update(Transform t)
        {
            Current = t.position;
        }

        public void Apply(in Vector3 newPos, bool zeroVelocity = false)
        {
            if (zeroVelocity)
            {
                Prev = newPos;
            }
            else
            {
                Prev = Current;
            }
            Current = newPos;
        }
    }
}