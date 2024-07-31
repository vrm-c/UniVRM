#include "./MToonCore.cginc"

appdata_full vert_forward_base_with_outline(appdata_full v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    v.normal = normalize(v.normal);
    return v;
}

v2f vert_forward_add(appdata_full v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    v.normal = normalize(v.normal);
    return InitializeV2F(v, UnityObjectToClipPos(v.vertex), 0);
}

[maxvertexcount(6)]
void geom_forward_base(triangle appdata_full IN[3], inout TriangleStream<v2f> stream)
{
    v2f o;
    
#if defined(MTOON_OUTLINE_WIDTH_WORLD) || defined(MTOON_OUTLINE_WIDTH_SCREEN)
    for (int i = 2; i >= 0; --i)
    {
        appdata_full v = IN[i];
        v2f o = InitializeV2F(v, CalculateOutlineVertexClipPosition(v), 1);
        stream.Append(o);
    }
    stream.RestartStrip();
#endif

    for (int j = 0; j < 3; ++j)
    {
        appdata_full v = IN[j];
        v2f o = InitializeV2F(v, UnityObjectToClipPos(v.vertex), 0);
        stream.Append(o);
    }
    stream.RestartStrip();
}
