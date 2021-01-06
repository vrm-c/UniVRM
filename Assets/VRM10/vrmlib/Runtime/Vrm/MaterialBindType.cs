namespace VrmLib
{
    public enum MaterialBindType
    {
        // /// float2: テクスチャーのUVの拡大率。UVでアクセスするテクスチャーすべてに適用される
        // UvScale,
        // /// float2: テクスチャーのUVのoffset。UVでアクセスするテクスチャーすべてに適用される
        // UvOffset,

        // float4: Unlit, PBR, MToon
        Color,
        /// float4: PBR, MToon
        EmissionColor,
        /// float4: MToon
        ShadeColor,
        /// float4: MToon
        RimColor,
        /// float4: MToon
        OutlineColor,
    }
}
