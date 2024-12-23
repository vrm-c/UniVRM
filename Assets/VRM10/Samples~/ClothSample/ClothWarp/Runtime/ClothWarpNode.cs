using System;
using System.Collections.Generic;
using SphereTriangle;
using UnityEngine;


namespace UniVRM10.ClothWarp
{
    [Serializable]
    public class ClothWarpNode
    {
        public ParticleInitState Init;
        public ParticleRuntimeState State;

        public readonly List<ClothWarpNode> Children = new();
        public readonly ClothWarpNode Parent;

        // 現フレームの力積算
        public Vector3 Force = Vector3.zero;

        // 直前で接触があった
        public bool HasCollide = false;

        public ClothWarpNode(int index, ClothWarpNode parent, SimulationEnv env, Transform transform, float radius, float mass)
        {
            Init = new ParticleInitState(index, transform, radius, mass);
            State = new ParticleRuntimeState(env, transform);
            Parent = parent;
        }


        public void BeginFrame(SimulationEnv env, FrameTime time, in Vector3 rest)
        {
            // integrate forces
            Force = Vector3.zero;

            // 曲げ
            if (HasCollide)
            {
                // 震え防止。ちょっとマイルドになるような気もする？
            }
            else
            {
                // Stiffness: 1 で即時に元に戻る
                Force += (rest - State.Current) * env.Stiffness / time.SqDt;
            }

            // 外力(sqDtで割るとピーキーすぎるのでこれでいいのでは？)
            Force += env.External / time.DeltaTime;
        }

        public Vector3 Verlet(SimulationEnv env, FrameTime time)
        {
            var velocity = (State.Current - State.Prev);
            if (HasCollide)
            {
                // 震え防止。ちょっとマイルドになるような気もする？
                velocity = Vector3.zero;
            }
            else
            {
                // DragForce: 1 で即時停止
                velocity *= (1 - env.DragForce);
            }

            HasCollide = false;
            return State.Current + velocity + Force * time.SqDt;
        }

        /// <summary>
        /// get ParentParent.rotatio * Parent.Init.LocalRotation
        /// </summary>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public Quaternion RestRotation(IReadOnlyList<Transform> transforms)
        {
            if (Parent == null)
            {
                return Quaternion.identity;
            }

            var parent = transforms[Parent.Init.Index];
            if (Parent.Parent == null)
            {
                var pt = parent.parent;
                if (pt == null)
                {
                    return Parent.Init.LocalRotation;
                }

                return pt.rotation * Parent.Init.LocalRotation;
            }

            var parentparent = transforms[Parent.Parent.Init.Index];
            var restRotation = parentparent.rotation * Parent.Init.LocalRotation;
            return restRotation;
        }

        public void OnDrawGizmos(Transform transform)
        {
            if (Init.Radius == 0)
            {
                return;
            }

            Gizmos.color = Init.Mass == 0 ? Color.red : Color.gray;
            if (transform.parent != null && Init.Mass > 0)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, Init.Radius);

            var r = Init.Radius * 2;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.left * r, Vector3.right * r);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.down * r, Vector3.up * r);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.back * r, Vector3.forward * r);
        }
    }
}