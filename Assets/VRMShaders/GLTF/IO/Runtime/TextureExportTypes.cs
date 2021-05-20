namespace VRMShaders
{
    public enum TextureExportTypes
    {
        // 無変換
        None,
        // Unity Standard様式 から glTF PBR様式への変換
        OcclusionMetallicRoughness,
        // Assetを使うときはそのバイト列を無変換で、それ以外は DXT5nm 形式からのデコードを行う
        Normal,
    }
}