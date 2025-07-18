using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitCone
    {
        /// <param name="src">AngleLimit空間の方向ベクトル</param>
        /// <param name="angleLimit">radius</param>
        /// <returns>AngleLimit空間の方向ベクトル</returns>
        public static float3 Apply(in float3 src, float angleLimit)
        {
            // tailDirのy要素をjointに設定されたangleの余弦と比較する
            var cosAngle = math.cos(angleLimit);
            if (src.y >= cosAngle)
            {
                return src;
            }

            var tailDir = src;
            {
                // x・z要素を、tailDirの正弦とjointに設定されたangleの正弦の比を用いてスケールする
                var ratio = math.sqrt((1.0f - cosAngle * cosAngle) / (1.0f - tailDir.y * tailDir.y));
                tailDir.x *= ratio;
                tailDir.z *= ratio;

                // y要素を、jointに設定されたangleの余弦とする
                tailDir.y = cosAngle;
            }

            return tailDir;
        }
    }
}