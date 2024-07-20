namespace UniGLTF
{
    internal enum TextureExportTypes
    {
        // sRGB テクスチャとして出力
        Srgb,
        // Linear テクスチャとして出力
        Linear,
        // Unity Standard様式 から glTF PBR様式への変換
        OcclusionMetallicRoughness,
        // Assetを使うときはそのバイト列を無変換で、それ以外は DXT5nm 形式からのデコードを行う
        Normal,
    }
}