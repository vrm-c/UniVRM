using System;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    [Serializable]
    public readonly struct BlittableSpan
    {
        private readonly int _startIndex;
        private readonly int _count;
        
        public int startIndex => _startIndex;
        public int count => _count;

        public int EndIndex => startIndex + count;

        public BlittableSpan(int startIndex, int count)
        {
            _startIndex = startIndex;
            _count = count;
        }
    }
}