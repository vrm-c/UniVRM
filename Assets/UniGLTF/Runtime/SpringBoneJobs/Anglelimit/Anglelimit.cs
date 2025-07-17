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
                        var tailDir = math.mul(math.inverse(angleSpaceToWorld), nextTail - head);
                        tailDir = AnglelimitCone.Apply(tailDir, joint.anglelimit1);
                        return math.mul(angleSpaceToWorld, tailDir);
                    }

                case AnglelimitTypes.Hinge:
                    {
                        var angleSpaceToWorld = anglelimitSpaceToWorld(logic, joint, parentRotation);
                        var tailDir = math.mul(math.inverse(angleSpaceToWorld), nextTail - head);
                        tailDir = AnglelimitHinge.Apply(tailDir, joint.anglelimit1);
                        return math.mul(angleSpaceToWorld, tailDir);
                    }


                case AnglelimitTypes.Spherical:
                    {
                        var angleSpaceToWorld = anglelimitSpaceToWorld(logic, joint, parentRotation);
                        var tailDir = math.mul(math.inverse(angleSpaceToWorld), nextTail - head);
                        tailDir = AnglelimitSpherical.Apply(tailDir, joint.anglelimit1, joint.anglelimit2);
                        return math.mul(angleSpaceToWorld, tailDir);
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
            var axisRotation = fromToQuaternion(new float3(0, 1, 0), logic.boneAxis);

            // limitのローカル空間をワールド空間に写像する回転
            return
                math.mul(parentRotation,
                math.mul(logic.localRotation,
                math.mul(axisRotation,
                joint.anglelimitOffset)))
            ;
        }

        // https://discussions.unity.com/t/unity-mathematics-equivalent-to-quaternion-fromtorotation/237459
        public static quaternion fromToQuaternion(in float3 from, in float3 to)
        {
            return quaternion.AxisAngle(
                  angle: math.acos(
                    math.clamp(
                      math.dot(
                        math.normalize(from),
                        math.normalize(to)
                      ),
                    -1f,
                  1f)
                  ),
                  axis: math.normalize(math.cross(from, to))
                );
        }

    }
}