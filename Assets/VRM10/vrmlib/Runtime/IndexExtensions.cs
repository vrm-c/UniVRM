namespace VrmLib
{
    public static class IndexExtensions
    {
        public static bool TryGetValidIndex(this int? index, int count, out int outValue)
        {
            outValue = -1;
            if (!index.HasValue)
            {
                return false;
            }
            var value = index.Value;
            if (value < 0)
            {
                return false;
            }
            if (value >= count)
            {
                return false;
            }
            outValue = value;
            return true;
        }
    }
}