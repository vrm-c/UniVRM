using System;

namespace UniGLTF
{
    public sealed class GlbBinaryParser
    {
        private readonly byte[] _data;
        private readonly string _name;
        
        public GlbBinaryParser(byte[] data, string uniqueName)
        {
            _data = data;

            if (string.IsNullOrWhiteSpace(uniqueName))
            {
                _name = Guid.NewGuid().ToString();
            }
            else
            {
                _name = uniqueName;
            }
        }

        public GltfData Parse()
        {
            return new GlbLowLevelParser(_name, _data).Parse();
        }
    }
}