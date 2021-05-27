using System.Collections.Generic;

namespace VRMShaders
{
    /// <summary>
    /// TextureImportParam の集合を Unique な集合にする。
    /// </summary>
    public sealed class TextureDescriptorSet
    {
        private readonly Dictionary<SubAssetKey, TextureDescriptor> _texDescDict = new Dictionary<SubAssetKey, TextureDescriptor>();

        public void Add(TextureDescriptor texDesc)
        {
            if (_texDescDict.ContainsKey(texDesc.SubAssetKey)) return;

            _texDescDict.Add(texDesc.SubAssetKey, texDesc);
        }

        public IEnumerable<TextureDescriptor> GetEnumerable()
        {
            foreach (var kv in _texDescDict)
            {
                yield return kv.Value;
            }
        }
    }
}
