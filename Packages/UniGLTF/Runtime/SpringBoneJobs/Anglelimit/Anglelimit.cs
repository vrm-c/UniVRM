using System;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class Anglelimit
    {
        public static readonly float SINGULARITY_EPSILON = 1e-8f;

        public static float3 Apply(
            in BlittableJointImmutable logic,
            in BlittableJointMutable joint,
            in quaternion parentRotation,
            in float3 head,
            in float3 nextTail
        )
        {
            if (joint.anglelimitType == AnglelimitTypes.None)
            {
                // do nothing
                return nextTail;
            }

            var angleSpaceToWorld = anglelimitSpaceToWorld(logic, joint, parentRotation);
            var tailDir = math.mul(
                math.inverse(angleSpaceToWorld),
                math.normalizesafe(nextTail - head)
            );

            switch (joint.anglelimitType)
            {
                case AnglelimitTypes.None:
                    throw new Exception("not reach here");

                case AnglelimitTypes.Cone:

                    tailDir = AnglelimitCone.Apply(tailDir, joint.anglelimit1);
                    break;

                case AnglelimitTypes.Hinge:
                    tailDir = AnglelimitHinge.Apply(tailDir, joint.anglelimit1);
                    break;

                case AnglelimitTypes.Spherical:
                    tailDir = AnglelimitSpherical.Apply(
                        tailDir,
                        joint.anglelimit1,
                        joint.anglelimit2
                    );
                    break;

                default:
                    throw new System.ArgumentException(
                        $"unknown joint.anglelimitType: {joint.anglelimitType}"
                    );
            }

            return head + math.mul(angleSpaceToWorld, tailDir) * logic.length;
        }

        /// <param name="nextTail">nextTail(position vector in world space)</param>
        /// <returns>tailDir(directionay vector in angle space)</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static quaternion anglelimitSpaceToWorld(
            in BlittableJointImmutable logic,
            in BlittableJointMutable joint,
            in quaternion parentRotation
        )
        {
            // Y+方向からjointのheadからtailに向かうベクトルへの最小回転
            var axisRotation = getAxisRotation(logic.boneAxis);

            // limitのローカル空間をワールド空間に写像する回転
            return math.mul(
                parentRotation,
                math.mul(logic.localRotation, math.mul(axisRotation, joint.anglelimitOffset))
            );
        }

        /// <summary>
        /// Y軸正方向から `to` への回転を表すクォータニオンを計算して返す。
        /// `to` は正規化されていると仮定する。
        ///
        /// See: https://github.com/0b5vr/vrm-specification/blob/75fbd48a7cb1d7250fa955838af6140e9c84844c/specification/VRMC_springBone_limit-1.0/README.ja.md#rotation-1
        ///
        /// TODO: Replace with the appropriate link to the specification later
        /// </summary>
        public static quaternion getAxisRotation(in float3 boneAxis)
        {
            // headからtailに向かうベクトルとY+方向との内積
            var dot = boneAxis.y;

            if (dot <= -1f + SINGULARITY_EPSILON)
            {
                // headからtailに向かうベクトルがY-方向の場合、X軸周りに180度回転させた回転を設定する
                return new quaternion(1, 0, 0, 0);
            }
            else
            {
                // それ以外の場合、Y+方向からjointのheadからtailに向かうベクトルへの最小回転を設定する
                // quaternion(cross(from, to); dot(from, to) + 1).normalized
                return math.normalizesafe(new quaternion(boneAxis.z, 0, -boneAxis.x, dot + 1));
            }
        }
    }
}
