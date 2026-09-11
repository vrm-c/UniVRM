using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class AnglelimitSpherical
    {
        /// <param name="tailDir">AngleLimit空間の方向ベクトル</param>
        /// <param name="limitPitch">radius</param>
        /// <param name="limitYaw">radius</param>
        /// <returns>AngleLimit空間の方向ベクトル</returns>
        public static float3 Apply(float3 tailDir, float limitPitch, float limitYaw)
        {
            // pitchを0以上π以下、yawを0以上π/2以下に制限する
            limitPitch = math.clamp(limitPitch, 0.0f, math.PI);
            limitYaw = math.clamp(limitYaw, 0.0f, math.PI / 2.0f);

            // tailDirのpitch・yawを計算する
            float pitch;
            if (tailDir.y <= -1.0f + Anglelimit.SINGULARITY_EPSILON)
            {
                // tailDirがy軸負方向の場合、Z軸正方向側の境界を選択するため、pitchをπとする
                pitch = math.PI;
            }
            else if (math.abs(tailDir.x) >= 1.0f - Anglelimit.SINGULARITY_EPSILON)
            {
                // tailDirがx軸正方向または負方向の場合、pitchを0とする
                pitch = 0.0f;
            }
            else
            {
                pitch = math.atan2(tailDir.z, tailDir.y);
            }
            var yaw = math.asin(math.clamp(tailDir.x, -1f, 1f));

            // pitchをlimitに設定されたpitchを用いて制限する
            if (math.abs(pitch) > limitPitch)
            {
                // isLimited = true;
                pitch = limitPitch * math.sign(pitch);
            }

            // yawをlimitに設定されたyawを用いて制限する
            if (math.abs(yaw) > limitYaw)
            {
                // isLimited = true;
                yaw = limitYaw * math.sign(yaw);
            }

            // tailDirをpitch・yawを用いて再計算する
            tailDir = math.float3(
                math.sin(yaw),
                math.cos(yaw) * math.cos(pitch),
                math.cos(yaw) * math.sin(pitch)
            );

            return tailDir;
        }
    }
}
