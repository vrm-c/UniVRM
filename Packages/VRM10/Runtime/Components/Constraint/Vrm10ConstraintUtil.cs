namespace UniVRM10
{
    public static class Vrm10ConstraintUtil
    {
        /// <summary>
        /// 右手系と左手系を相互に変換する
        /// </summary>
        public static UniGLTF.Extensions.VRMC_node_constraint.AimAxis ReverseX(UniGLTF.Extensions.VRMC_node_constraint.AimAxis src)
        {
            switch (src)
            {
                case UniGLTF.Extensions.VRMC_node_constraint.AimAxis.PositiveX: return UniGLTF.Extensions.VRMC_node_constraint.AimAxis.NegativeX;
                case UniGLTF.Extensions.VRMC_node_constraint.AimAxis.NegativeX: return UniGLTF.Extensions.VRMC_node_constraint.AimAxis.PositiveX;
                default: return src;
            }
        }
    }
}
