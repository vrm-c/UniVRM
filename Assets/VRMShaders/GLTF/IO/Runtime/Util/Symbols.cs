namespace VRMShaders
{
    public static class Symbols
    {
        /// <summary>
        /// #if 文を局所化する。
        /// VRMShaders が最下層になるため、ここに配置している
        /// </summary>
        /// <value></value>
        public static bool VRM_DEVELOP
        {
            get
            {
#if VRM_DEVELOP
                return true;
#else
                return false;
#endif
            }
        }
    }
}
