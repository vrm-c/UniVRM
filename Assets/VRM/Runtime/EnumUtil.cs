using System;


namespace VRM
{
    public static class EnumUtil
    {
        public static T TryParseOrDefault<T>(string src, T defaultValue=default(T)) where T : struct
        {
            try
            {
                return (T)Enum.Parse(typeof(T), src, true);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
