using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitHinge
    {
        /// <param name="src">AngleLimit空間の方向ベクトル</param>
        /// <param name="angleLimit">radius</param>
        /// <returns>AngleLimit空間の方向ベクトル</returns>
        public static float3 Apply(in float3 src, float limitAngle)
        {
            // x要素を0にし、正規化する
            float3 tailDir = src;
            tailDir.x = 0.0f;
            tailDir = math.normalize(tailDir);

            // tailDirのy要素をjointに設定されたangleの余弦と比較する
            var cosAngle = math.cos(limitAngle);
            if (tailDir.y < cosAngle)
            {
                // z要素を、tailDirの正弦とjointに設定されたangleの正弦の比を用いてスケールする
                var ratio = math.sqrt((1.0f - cosAngle * cosAngle) / (1.0f - tailDir.y * tailDir.y));
                tailDir.z *= ratio;

                // y要素を、jointに設定されたangleの余弦とする
                tailDir.y = cosAngle;
            }

            return tailDir;
        }
    }
}