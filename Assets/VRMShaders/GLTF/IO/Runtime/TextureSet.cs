using System.Collections.Generic;

namespace VRMShaders
{
    public sealed class TextureSet
    {
        private readonly Dictionary<SubAssetKey, TextureImportParam> _params = new Dictionary<SubAssetKey, TextureImportParam>();

        public void Add(TextureImportParam param)
        {
            if (_params.ContainsKey(param.SubAssetKey)) return;

            _params.Add(param.SubAssetKey, param);
        }

        public IEnumerable<TextureImportParam> GetTextureParamsDistinct()
        {
            foreach (var kv in _params)
            {
                yield return kv.Value;
            }
        }
    }
}
