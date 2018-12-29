using System;


namespace UniJSON
{
    public static class Utf8StringSplitterExtensions
    {
        /// <summary>
        /// Split integer from start
        /// 
        /// "123 " => "123"
        /// " 123" => FormatException
        /// 
        /// must start +-0123456789
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Utf8String SplitInteger(this Utf8String src)
        {
            var i = 0;
            if(src[0]=='+' || src[0] == '-')
            {
                ++i;
            }

            var j = i;
            for(; j<src.ByteLength; ++j)
            {
                if(src[j]<'0' || src[j]>'9')
                {
                    break;
                }
            }

            if (i == j)
            {
                throw new FormatException();
            }

            return src.Subbytes(0, j);
        }
    }
}
