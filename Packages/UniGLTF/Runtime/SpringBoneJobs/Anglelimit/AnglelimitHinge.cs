using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitHinge
    {
        /// <param name="tailDir">AngleLimit空間の方向ベクトル</param>
        /// <param name="limitAngle">radius</param>
        /// <returns>AngleLimit空間の方向ベクトル</returns>
        public static float3 Apply(float3 tailDir, float limitAngle)
        {
            // angleを0以上π以下に制限する
            limitAngle = math.clamp(limitAngle, 0.0f, math.PI);

            var projectedLengthSquared = tailDir.y * tailDir.y + tailDir.z * tailDir.z;
            if (projectedLengthSquared <= Anglelimit.SINGULARITY_EPSILON)
            {
                // tailDirがx軸正方向または負方向の場合、Y軸正方向を選択する
                tailDir = math.float3(0.0f, 1.0f, 0.0f);
            }
            else
            {
                // tailDirをヒンジのYZ平面へ射影する
                tailDir =
                    math.float3(0.0f, tailDir.y, tailDir.z) / math.sqrt(projectedLengthSquared);

                // tailDirのy要素をlimitに設定されたangleの余弦と比較する
                var cosLimitAngle = math.cos(limitAngle);
                if (tailDir.y < cosLimitAngle)
                {
                    var sinLimitAngle = math.sqrt(1.0f - cosLimitAngle * cosLimitAngle);

                    // zの符号を維持し、z==0 の場合は z軸正方向側を選択する
                    var zSign = (tailDir.z < 0.0f) ? -1.0f : 1.0f;
                    tailDir.y = cosLimitAngle;
                    tailDir.z = sinLimitAngle * zSign;
                }
            }
            return tailDir;
        }
    }
}
