Shader "Campus/StudentWalk"
{
 Properties { _BaseMap("Texture",2D)="white"{} _BaseColor("Color",Color)=(1,1,1,1) _Gait("Gait",Float)=0 _MeshHeight("Mesh height",Float)=1 _MeshBottom("Mesh bottom",Float)=0 }
 SubShader
 {
 Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass
 {
 Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseMap_ST; float4 _BaseColor; float _Gait; float _MeshHeight; float _MeshBottom;
 CBUFFER_END
 struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
 struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 normalWS:TEXCOORD1; float4 shadowCoord:TEXCOORD2; };
 float3 rotZ(float3 v,float a) { float s=sin(a),c=cos(a);return float3(c*v.x-s*v.y,s*v.x+c*v.y,v.z); }
 float3 rotX(float3 v,float a) { float s=sin(a),c=cos(a);return float3(v.x,c*v.y-s*v.z,s*v.y+c*v.z); }
 Varyings vert(Attributes IN)
 {
 Varyings OUT;
 float h=_MeshHeight;
 float3 p=float3(IN.positionOS.x,IN.positionOS.z-_MeshBottom,-IN.positionOS.y);
 float3 n=float3(IN.normalOS.x,IN.normalOS.z,-IN.normalOS.y);
 float side=p.x>=0?1:-1;
 float arm=smoothstep(.17*h,.28*h,abs(p.x))*smoothstep(.47*h,.62*h,p.y);
 float3 shoulder=float3(side*.17*h,.72*h,0);
 float angle=-side*1.12*arm;
 p=rotZ(p-shoulder,angle)+shoulder;n=rotZ(n,angle);
 float sway=sin(_Time.y*8)*_Gait*.32;
 if(arm>.3) {p=rotX(p-shoulder,side*sway*.7)+shoulder; n=rotX(n,side*sway*.7);}
 float leg=(1-smoothstep(.35*h,.47*h,p.y))*(1-arm);
 float3 hip=float3(side*.09*h,.43*h,0);
 p=rotX(p-hip,side*sway*leg)+hip;n=rotX(n,side*sway*leg);
 float3 local=float3(p.x,-p.z,p.y+_MeshBottom);
 float3 normal=float3(n.x,-n.z,n.y);
 VertexPositionInputs inputs=GetVertexPositionInputs(local);
 OUT.positionCS=inputs.positionCS; OUT.shadowCoord=GetShadowCoord(inputs);
 OUT.normalWS=TransformObjectToWorldNormal(normal);OUT.uv=TRANSFORM_TEX(IN.uv,_BaseMap);return OUT;
 }
 half4 frag(Varyings IN):SV_Target
 {
 Light light=GetMainLight(IN.shadowCoord);
 half lambert=saturate(dot(normalize(IN.normalWS),light.direction));
 half3 color=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,IN.uv).rgb*_BaseColor.rgb;
 return half4(color*(half3(.45,.48,.52)+light.color*lambert*.7*light.shadowAttenuation),1);
 }
 ENDHLSL
 }
 }
}
