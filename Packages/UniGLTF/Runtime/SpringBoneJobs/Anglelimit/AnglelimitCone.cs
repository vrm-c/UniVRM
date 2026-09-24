using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitCone
    {
        /// <param name="tailDir">AngleLimit空間の方向ベクトル</param>
        /// <param name="limitAngle">radius</param>
        /// <returns>AngleLimit空間の方向ベクトル</returns>
        public static float3 Apply(float3 tailDir, float limitAngle)
        {
            // angleを0以上π以下に制限する
            limitAngle = math.clamp(limitAngle, 0.0f, math.PI);

            // tailDirのy要素をlimitに設定されたangleの余弦と比較する
            var cosLimitAngle = math.cos(limitAngle);
            if (tailDir.y < cosLimitAngle)
            {
                // x・z要素を、tailDirの正弦とjointに設定されたangleの正弦の比を用いてスケールする
                var horizontalLengthSquared = 1.0f - tailDir.y * tailDir.y;

                if (horizontalLengthSquared <= Anglelimit.SINGULARITY_EPSILON)
                {
                    // tailDirがy軸負方向の場合、z軸正方向側を選択する
                    tailDir.x = 0.0f;
                    tailDir.z = math.sqrt(1.0f - cosLimitAngle * cosLimitAngle);
                }
                else
                {
                    var scale = math.sqrt(
                        (1.0f - cosLimitAngle * cosLimitAngle) / horizontalLengthSquared
                    );
                    tailDir.x *= scale;
                    tailDir.z *= scale;
                }

                // y要素をlimitに設定されたangleの余弦とする
                tailDir.y = cosLimitAngle;
            }

            return tailDir;
        }
    }
}
