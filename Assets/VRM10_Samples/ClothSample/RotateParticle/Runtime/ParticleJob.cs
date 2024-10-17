using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace RotateParticle
{
    public struct ParticleJob : IJob
    {
        public float DeltaTime;
        public float Stiffness;
        public float DragForce;
        public Vector3 External;

        public int Index;
        public int ParentIndex;

        public float Mass;
        public Vector3 Rest;
        public Vector3 CurrentPosition;
        public Vector3 Prev;

        public Quaternion ParentParentRotation;
        public Quaternion InitParentLocalRotation;
        public Vector3 InitBoneAxis;
        public Vector3 ParentPosition;

        public NativeArray<Vector3> NewPos;
        public NativeArray<Quaternion> NewRot;

        public void Execute()
        {
            var tr = Step();
            if (tr.HasValue)
            {
                var (newPos, parentRot) = tr.Value;
                NewPos[Index] = newPos;
                NewRot[ParentIndex] = parentRot;
            }
        }

        public (Vector3 NewPos, Quaternion ParentRotation)? Step()
        {
            if (Mass == 0)
            {
                return default;
            }

            var sqDt = DeltaTime * DeltaTime;

            var f = (Rest - CurrentPosition) * Stiffness / sqDt;
            // Debug.Log($"{rest}");

            f += (External / sqDt);

            var newPos = CurrentPosition + (CurrentPosition - Prev) * (1 - DragForce) + f * sqDt;

            var parentRotation = CalcParentRotation(newPos, ParentParentRotation, InitParentLocalRotation, InitBoneAxis, ParentPosition);

            return (newPos, parentRotation);
        }

        public static Quaternion CalcParentRotation(in Vector3 newPos,
            in Quaternion parentparent,
            in Quaternion initParentLocalRotation,
            in Vector3 initBoneAxis,
            in Vector3 parentPosition)
        {
            // 親の回転として結果を適用する(位置から回転を作る)
            var restRotation = parentparent * initParentLocalRotation;
            var r = CalcRotation(restRotation, initBoneAxis, newPos - parentPosition);
            // _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, transform.position);
            return r;
        }

        static Quaternion CalcRotation(Quaternion restRotation, Vector3 boneAxis, Vector3 to)
        {
            Quaternion aimRotation = Quaternion.FromToRotation(restRotation * boneAxis, to);
            return aimRotation * restRotation;
        }
    }
}