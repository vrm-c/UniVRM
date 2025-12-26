using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class Anglelimit
    {
        public static float3 Apply(
            in BlittableJointImmutable logic, in BlittableJointMutable joint,
            in quaternion parentRotation, in float3 head, in float3 nextTail)
        {
            switch (joint.anglelimitType)
            {
                case AnglelimitTypes.None:
                    // do nothing
                    return nextTail;

                case AnglelimitTypes.Cone:
                    {
                        var angleSpaceToWorld = anglelimitSpaceToWorld(logic, joint, parentRotation);
                        var tailDir = math.mul(math.inverse(angleSpaceToWorld), math.normalize(nextTail - head));
                        tailDir = AnglelimitCone.Apply(tailDir, joint.anglelimit1);
                        return head + math.mul(angleSpaceToWorld, tailDir) * logic.length;
                    }

                case AnglelimitTypes.Hinge:
                    {
                        var angleSpaceToWorld = anglelimitSpaceToWorld(logic, joint, parentRotation);
                        var tailDir = math.mul(math.inverse(angleSpaceToWorld), math.normalize(nextTail - head));
                        tailDir = AnglelimitHinge.Apply(tailDir, joint.anglelimit1);
                        return head + math.mul(angleSpaceToWorld, tailDir) * logic.length;
                    }


                case AnglelimitTypes.Spherical:
                    {
                        var angleSpaceToWorld = anglelimitSpaceToWorld(logic, joint, parentRotation);
                        var tailDir = math.mul(math.inverse(angleSpaceToWorld), math.normalize(nextTail - head));
                        tailDir = AnglelimitSpherical.Apply(tailDir, joint.anglelimit1, joint.anglelimit2);
                        return head + math.mul(angleSpaceToWorld, tailDir) * logic.length;
                    }

                default:
                    throw new System.ArgumentException($"unknown joint.anglelimitType: {joint.anglelimitType}");
            }
        }

        /// <param name="nextTail">nextTail(position vector in world space)</param>
        /// <returns>tailDir(directionay vector in angle space)</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static quaternion anglelimitSpaceToWorld(in BlittableJointImmutable logic, in BlittableJointMutable joint,
        in quaternion parentRotation)
        {
            // Y+方向からjointのheadからtailに向かうベクトルへの最小回転
            var axisRotation = getAxisRotation(logic.boneAxis);

            // limitのローカル空間をワールド空間に写像する回転
            return
                math.mul(parentRotation,
                math.mul(logic.localRotation,
                math.mul(axisRotation,
                joint.anglelimitOffset)))
            ;
        }

        /// <summary>
        /// Y軸正方向から `to` への回転を表すクォータニオンを計算して返す。
        /// `to` は正規化されていると仮定する。
        ///
        /// See: https://github.com/0b5vr/vrm-specification/blob/75fbd48a7cb1d7250fa955838af6140e9c84844c/specification/VRMC_springBone_limit-1.0/README.ja.md#rotation-1
        ///
        /// TODO: Replace with the appropriate link to the specification later
        /// </summary>
        public static quaternion getAxisRotation(in float3 to)
        {
            // dot(from, to) + 1
            var dot1 = to.y + 1f;

            // Handle the case where from and to are parallel and opposite
            if (dot1 < 1e-8f) // dot is approximately -1
            {
                return new quaternion(1f, 0f, 0f, 0f);
            }

            // General case
            // quaternion(cross(from, to); dot(from, to) + 1).normalized
            return math.normalize(new quaternion(to.z, 0f, -to.x, dot1));
        }
    }
}