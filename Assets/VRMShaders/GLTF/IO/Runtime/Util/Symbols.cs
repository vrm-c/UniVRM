namespace VRMShaders
{
    public static class Symbols
    {
        /// <summary>
        /// #if 文を局所化する。
        /// VRMShaders が最下層になるため、ここに配置している
        /// </summary>
        /// <value></value>
        public const bool VRM_DEVELOP =
#if VRM_DEVELOP
true
#else
false
#endif
        ;
    }
}
