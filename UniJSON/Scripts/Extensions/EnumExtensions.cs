using System;


namespace UniJSON
{
    public static class EnumExtensions
    {
        public static bool HasFlag(this Enum keys, Enum flag)
        {
            if (keys.GetType() != flag.GetType())
                throw new ArgumentException("Type Mismatch");
            return (Convert.ToUInt64(keys) & Convert.ToUInt64(flag)) != 0;
        }
    }
}
