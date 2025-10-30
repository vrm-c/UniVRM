namespace UniGLTF
{
    public static class IndexExtensions
    {
        public static bool HasValidIndex(this int? self)
        {
            if (!self.HasValue)
            {
                return false;
            }
            if (self.Value < 0)
            {
                // 古いモデルで index の無効値に -1 を使っている場合がある
                return false;
            }
            return true;
        }

        public static bool HasValidIndex(this int? self, int collectionLength)
        {
            if (!self.HasValue)
            {
                return false;
            }
            if (self.Value < 0)
            {
                // 古いモデルで index の無効値に -1 を使っている場合がある
                return false;
            }
            if (self.Value >= collectionLength)
            {
                // ついでに上限もチェック
                return false;
            }
            return true;
        }
    }
}
