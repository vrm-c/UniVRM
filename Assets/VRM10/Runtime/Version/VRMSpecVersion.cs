using System;

namespace UniVRM10
{
    /// <summary>
    /// https://github.com/vrm-c/vrm-specification/tree/master/specification
    /// 
    /// spec version として解釈できる git tag を運用するべきか。
    /// 
    /// コード生成を通して自動で更新する必要がある。
    /// </summary>
    public class VRMSpecVersion
    {
        public const int Major = 1;
        public const int Minor = 0;

        public static string Version
        {
            get
            {
                return String.Format("{0}.{1}.draft", Major, Minor);
            }
        }

        public const string VERSION = "1.0.draft";
    }
}
