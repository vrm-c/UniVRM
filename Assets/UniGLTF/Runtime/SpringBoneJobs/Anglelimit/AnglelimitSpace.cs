using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitSpace
    {
        public static float3 ToTailDir(in quaternion src)
        {
            // // Y+方向からjointのheadからtailに向かうベクトルへの最小回転
            // let axisRotation = fromToQuaternion(vec3(0, 1, 0), boneAxis);

            // // limitのローカル空間をワールド空間に写像する回転
            // let rotation =
            //   joint.parent.worldRotation *
            //   joint.localSpaceInitialRotation *
            //   axisRotation *
            //   joint.limit.rotation;

            // // tailの向きをlimitのローカル空間に写像する
            // tailDir = tailDir.applyQuaternion(rotation.inverse);

            // // limitを適用する
            // joint.limit.apply(tailDir);

            // // tailの向きをワールド空間に戻す
            // tailDir = tailDir.applyQuaternion(rotation);

            throw new System.NotImplementedException();
        }

        public static quaternion FromTailDir(in float3 src)
        {
            throw new System.NotImplementedException();
        }
    }
}