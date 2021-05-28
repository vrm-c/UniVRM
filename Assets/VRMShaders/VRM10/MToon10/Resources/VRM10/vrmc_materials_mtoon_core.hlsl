#ifndef MTOON_CORE_INCLUDED
#define MTOON_CORE_INCLUDED

#include "Lighting.cginc"
#include "AutoLight.cginc"

half _Cutoff;
fixed4 _Color;
fixed4 _ShadeColor;
sampler2D _MainTex; float4 _MainTex_ST;
sampler2D _ShadeTexture;
half _BumpScale;
sampler2D _BumpMap;
sampler2D _ReceiveShadowTexture; 
half _ReceiveShadowRate;
sampler2D _ShadingGradeTexture;
half _ShadingGradeRate;
half _ShadeShift;
half _ShadeToony;
half _LightColorAttenuation;
half _IndirectLightIntensity;
sampler2D _RimTexture;
half4 _RimColor;
half _RimLightingMix;
half _RimFresnelPower;
half _RimLift;
sampler2D _SphereAdd;
half4 _EmissionColor;
sampler2D _EmissionMap;
sampler2D _OutlineWidthTexture;
half _OutlineWidth;
half _OutlineScaledMaxDistance;
fixed4 _OutlineColor;
half _OutlineLightingMix;
sampler2D _UvAnimMaskTexture;
float _UvAnimScrollX;
float _UvAnimScrollY;
float _UvAnimRotation;

//UNITY_INSTANCING_BUFFER_START(Props)
//UNITY_INSTANCING_BUFFER_END(Props)

struct v2f
{
    float4 pos : SV_POSITION;
    float4 posWorld : TEXCOORD0;
    half3 tspace0 : TEXCOORD1;
    half3 tspace1 : TEXCOORD2;
    half3 tspace2 : TEXCOORD3;
    float2 uv0 : TEXCOORD4;
    float isOutline : TEXCOORD5;
    fixed4 color : TEXCOORD6;
    UNITY_FOG_COORDS(7)
    UNITY_SHADOW_COORDS(8)
    //UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if any instanced properties are going to be accessed in the fragment Shader.
    UNITY_VERTEX_OUTPUT_STEREO 
};

inline v2f InitializeV2F(appdata_full v, float4 projectedVertex, float isOutline)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    //UNITY_TRANSFER_INSTANCE_ID(v, o);
    
    o.pos = projectedVertex;
    o.posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.uv0 = v.texcoord;
    half3 worldNormal = UnityObjectToWorldNormal(v.normal);
    half3 worldTangent = UnityObjectToWorldDir(v.tangent);
    half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
    half3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
    o.tspace0 = half3(worldTangent.x, worldBitangent.x, worldNormal.x);
    o.tspace1 = half3(worldTangent.y, worldBitangent.y, worldNormal.y);
    o.tspace2 = half3(worldTangent.z, worldBitangent.z, worldNormal.z);
    o.isOutline = isOutline;
    o.color = v.color;
    UNITY_TRANSFER_SHADOW(o, o._ShadowCoord);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

inline float4 CalculateOutlineVertexClipPosition(appdata_full v)
{
    float outlineTex = tex2Dlod(_OutlineWidthTexture, float4(TRANSFORM_TEX(v.texcoord, _MainTex), 0, 0)).r;
    
 #if defined(MTOON_OUTLINE_WIDTH_WORLD)
    float3 worldNormalLength = length(mul((float3x3)transpose(unity_WorldToObject), v.normal));
    float3 outlineOffset = 0.01 * _OutlineWidth * outlineTex * worldNormalLength * v.normal;
    float4 vertex = UnityObjectToClipPos(v.vertex + outlineOffset);
 #elif defined(MTOON_OUTLINE_WIDTH_SCREEN)
    float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
    float aspect = abs(nearUpperRight.y / nearUpperRight.x);
    float4 vertex = UnityObjectToClipPos(v.vertex);
    float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal.xyz);
    float3 clipNormal = TransformViewToProjection(viewNormal.xyz);
    float2 projectedNormal = normalize(clipNormal.xy);
    projectedNormal *= min(vertex.w, _OutlineScaledMaxDistance);
    projectedNormal.x *= aspect;
    vertex.xy += 0.01 * _OutlineWidth * outlineTex * projectedNormal.xy * saturate(1 - abs(normalize(viewNormal).z)); // ignore offset when normal toward camera
 #else
    float4 vertex = UnityObjectToClipPos(v.vertex);
 #endif
    return vertex;
}

float4 frag_forward(v2f i) : SV_TARGET
{
#ifdef MTOON_CLIP_IF_OUTLINE_IS_NONE
    #ifdef MTOON_OUTLINE_WIDTH_WORLD
    #elif MTOON_OUTLINE_WIDTH_SCREEN
    #else
        clip(-1);
    #endif
#endif

    //UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    
    // const
    const float PI_2 = 6.28318530718;
    const float EPS_COL = 0.00001;
    
    // uv
    float2 mainUv = TRANSFORM_TEX(i.uv0, _MainTex);
    
    // uv anim
    float uvAnim = tex2D(_UvAnimMaskTexture, mainUv).r * _Time.y;
    // translate uv in bottom-left origin coordinates.
    mainUv += float2(_UvAnimScrollX, _UvAnimScrollY) * uvAnim;
    // rotate uv counter-clockwise around (0.5, 0.5) in bottom-left origin coordinates.
    float rotateRad = _UvAnimRotation * PI_2 * uvAnim;
    const float2 rotatePivot = float2(0.5, 0.5);
    mainUv = mul(float2x2(cos(rotateRad), -sin(rotateRad), sin(rotateRad), cos(rotateRad)), mainUv - rotatePivot) + rotatePivot;
    
    // main tex
    half4 mainTex = tex2D(_MainTex, mainUv);
    
    // alpha
    half alpha = 1;
#ifdef _ALPHATEST_ON
    alpha = _Color.a * mainTex.a;
    alpha = (alpha - _Cutoff) / max(fwidth(alpha), EPS_COL) + 0.5; // Alpha to Coverage
    clip(alpha - _Cutoff);
    alpha = 1.0; // Discarded, otherwise it should be assumed to have full opacity
#endif
#ifdef _ALPHABLEND_ON
    alpha = _Color.a * mainTex.a;
#if !_ALPHATEST_ON && SHADER_API_D3D11 // Only enable this on D3D11, where I tested it
    clip(alpha - 0.0001);              // Slightly improves rendering with layered transparency
#endif
#endif
    
    // normal
#ifdef _NORMALMAP
    half3 tangentNormal = UnpackScaleNormal(tex2D(_BumpMap, mainUv), _BumpScale);
    half3 worldNormal;
    worldNormal.x = dot(i.tspace0, tangentNormal);
    worldNormal.y = dot(i.tspace1, tangentNormal);
    worldNormal.z = dot(i.tspace2, tangentNormal);
#else
    half3 worldNormal = half3(i.tspace0.z, i.tspace1.z, i.tspace2.z);
#endif
    float3 worldView = normalize(lerp(_WorldSpaceCameraPos.xyz - i.posWorld.xyz, UNITY_MATRIX_V[2].xyz, unity_OrthoParams.w));
    worldNormal *= step(0, dot(worldView, worldNormal)) * 2 - 1; // flip if projection matrix is flipped
    worldNormal *= lerp(+1.0, -1.0, i.isOutline);
    worldNormal = normalize(worldNormal);

    // Unity lighting
    UNITY_LIGHT_ATTENUATION(shadowAttenuation, i, i.posWorld.xyz);
    half3 lightDir = lerp(_WorldSpaceLightPos0.xyz, normalize(_WorldSpaceLightPos0.xyz - i.posWorld.xyz), _WorldSpaceLightPos0.w);
    half3 lightColor = _LightColor0.rgb * step(0.5, length(lightDir)); // length(lightDir) is zero if directional light is disabled.
    half dotNL = dot(lightDir, worldNormal);
#ifdef MTOON_FORWARD_ADD
    half lightAttenuation = 1;
#else
    half lightAttenuation = shadowAttenuation * lerp(1, shadowAttenuation, _ReceiveShadowRate * tex2D(_ReceiveShadowTexture, mainUv).r);
#endif
    
    // Decide albedo color rate from Direct Light
    half shadingGrade = 1.0 - _ShadingGradeRate * (1.0 - tex2D(_ShadingGradeTexture, mainUv).r);
    half lightIntensity = dotNL; // [-1, +1]
    lightIntensity = lightIntensity * 0.5 + 0.5; // from [-1, +1] to [0, 1]
    lightIntensity = lightIntensity * lightAttenuation; // receive shadow
    lightIntensity = lightIntensity * shadingGrade; // darker
    lightIntensity = lightIntensity * 2.0 - 1.0; // from [0, 1] to [-1, +1]
    // tooned. mapping from [minIntensityThreshold, maxIntensityThreshold] to [0, 1]
    half maxIntensityThreshold = lerp(1, _ShadeShift, _ShadeToony);
    half minIntensityThreshold = _ShadeShift;
    lightIntensity = saturate((lightIntensity - minIntensityThreshold) / max(EPS_COL, (maxIntensityThreshold - minIntensityThreshold)));
    
    // Albedo color
    half4 shade = _ShadeColor * tex2D(_ShadeTexture, mainUv);
    half4 lit = _Color * mainTex;
    half3 col = lerp(shade.rgb, lit.rgb, lightIntensity);

    // Direct Light
    half3 lighting = lightColor;
    lighting = lerp(lighting, max(EPS_COL, max(lighting.x, max(lighting.y, lighting.z))), _LightColorAttenuation); // color atten
#ifdef MTOON_FORWARD_ADD
#ifdef _ALPHABLEND_ON
    lighting *= step(0, dotNL); // darken if transparent. Because Unity's transparent material can't receive shadowAttenuation.
#endif
    lighting *= 0.5; // darken if additional light.
    lighting *= min(0, dotNL) + 1; // darken dotNL < 0 area by using half lambert
    lighting *= shadowAttenuation; // darken if receiving shadow
#else
    // base light does not darken.
#endif
    col *= lighting;

    // Indirect Light
#ifdef MTOON_FORWARD_ADD
#else
    half3 toonedGI = 0.5 * (ShadeSH9(half4(0, 1, 0, 1)) + ShadeSH9(half4(0, -1, 0, 1)));
    half3 indirectLighting = lerp(toonedGI, ShadeSH9(half4(worldNormal, 1)), _IndirectLightIntensity);
    indirectLighting = lerp(indirectLighting, max(EPS_COL, max(indirectLighting.x, max(indirectLighting.y, indirectLighting.z))), _LightColorAttenuation); // color atten
    col += indirectLighting * lit;
    
    col = min(col, lit); // comment out if you want to PBR absolutely.
#endif

    // parametric rim lighting
#ifdef MTOON_FORWARD_ADD
    half3 staticRimLighting = 0;
    half3 mixedRimLighting = lighting;
#else
    half3 staticRimLighting = 1;
    half3 mixedRimLighting = lighting + indirectLighting;
#endif
    half3 rimLighting = lerp(staticRimLighting, mixedRimLighting, _RimLightingMix);
    half3 rim = pow(saturate(1.0 - dot(worldNormal, worldView) + _RimLift), _RimFresnelPower) * _RimColor.rgb * tex2D(_RimTexture, mainUv).rgb;
    col += lerp(rim * rimLighting, half3(0, 0, 0), i.isOutline);

    // additive matcap
#ifdef MTOON_FORWARD_ADD
#else
    half3 worldCameraUp = normalize(UNITY_MATRIX_V[1].xyz);
    half3 worldViewUp = normalize(worldCameraUp - worldView * dot(worldView, worldCameraUp));
    half3 worldViewRight = normalize(cross(worldView, worldViewUp));
    half2 matcapUv = half2(dot(worldViewRight, worldNormal), dot(worldViewUp, worldNormal)) * 0.5 + 0.5;
    half3 matcapLighting = tex2D(_SphereAdd, matcapUv);
    col += lerp(matcapLighting, half3(0, 0, 0), i.isOutline);
#endif

    // Emission
#ifdef MTOON_FORWARD_ADD
#else
    half3 emission = tex2D(_EmissionMap, mainUv).rgb * _EmissionColor.rgb;
    col += lerp(emission, half3(0, 0, 0), i.isOutline);
#endif

    // outline
#ifdef MTOON_OUTLINE_COLOR_FIXED
    col = lerp(col, _OutlineColor, i.isOutline);
#elif MTOON_OUTLINE_COLOR_MIXED
    col = lerp(col, _OutlineColor * lerp(half3(1, 1, 1), col, _OutlineLightingMix), i.isOutline);
#else
#endif

    // debug
#ifdef MTOON_DEBUG_NORMAL
    #ifdef MTOON_FORWARD_ADD
        return float4(0, 0, 0, 0);
    #else
        return float4(worldNormal * 0.5 + 0.5, alpha);
    #endif
#elif MTOON_DEBUG_LITSHADERATE
    #ifdef MTOON_FORWARD_ADD
        return float4(0, 0, 0, 0);
    #else
        return float4(lightIntensity * lighting, alpha);
    #endif
#endif


    half4 result = half4(col, alpha);
    UNITY_APPLY_FOG(i.fogCoord, result);
    return result;
}

#endif // MTOON_CORE_INCLUDED
