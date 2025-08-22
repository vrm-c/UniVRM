using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitSpherical
    {
        /// <param name="tailDir">AngleLimit空間の方向ベクトル</param>
        /// <param name="limitAnglePhi">radius</param>
        /// <param name="limitAngleTheta">radius</param>
        /// <returns>AngleLimit空間の方向ベクトル</returns>
        public static float3 Apply(in float3 tailDir, float limitAnglePhi, float limitAngleTheta)
        {
            // tailDirのphi・thetaを計算する
            var phi = math.atan2(tailDir.z, tailDir.y);
            var theta = math.asin(tailDir.x);

            // phi・thetaをjointに設定されたphi・thetaを用いて制限する
            // var isLimited = false;
            if (math.abs(phi) > limitAnglePhi)
            {
                // isLimited = true;
                phi = limitAnglePhi * math.sign(phi);
            }

            // thetaをjointに設定されたthetaを用いて制限する
            if (math.abs(theta) > limitAngleTheta)
            {
                //   isLimited = true;
                theta = limitAngleTheta * math.sign(theta);
            }

            // tailDirをphi・thetaを用いて再計算する
            var cos_theta = math.cos(theta);
            return new float3(math.sin(theta), cos_theta * math.cos(phi), cos_theta * math.sin(phi));
        }
    }
}