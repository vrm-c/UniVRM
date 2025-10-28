namespace UniVRM10
{
    public static class IndexExtensions
    {
        public static bool TryGetValidIndex(this int value, int count, out int index)
        {
            if (value < 0)
            {
                index = -1;
                return false;
            }
            if (value >= count)
            {
                index = -1;
                return false;
            }

            index = value;
            return true;
        }

        public static bool TryGetValidIndex(this int? value, int count, out int index)
        {
            if (!value.HasValue)
            {
                index = -1;
                return false;
            }
            if (value < 0)
            {
                index = -1;
                return false;
            }
            if (value >= count)
            {
                index = -1;
                return false;
            }

            index = value.Value;
            return true;
        }
    }
}
