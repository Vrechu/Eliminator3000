Shader "Custom/NeonGrid"
{
    Properties
    {
        [HDR] _LineColor ("Line Color", Color) = (0, 0.7, 5, 1)
        _BaseColor ("Base Color", Color) = (0.01, 0.01, 0.05, 1)
        _Tiling ("Cells X, Cells Y", Vector) = (20, 1200, 0, 0)
        _LineWidth ("Line Width", Range(0.005, 0.2)) = 0.03
        _Glow ("Glow Strength", Range(0, 1)) = 0.35
        _FadeStart ("Fade Start (distance)", Float) = 300
        _FadeEnd ("Fade End (distance)", Float) = 1500
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _LineColor;
                half4 _BaseColor;
                float4 _Tiling;
                float _LineWidth;
                float _Glow;
                float _FadeStart;
                float _FadeEnd;
            CBUFFER_END

            struct A { float4 pos : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 cs : SV_POSITION; float2 uv : TEXCOORD0; float3 ws : TEXCOORD1; };

            V vert(A i)
            {
                V o;
                o.cs = TransformObjectToHClip(i.pos.xyz);
                o.ws = TransformObjectToWorld(i.pos.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                float2 g = i.uv * _Tiling.xy;
                float2 d = abs(frac(g - 0.5) - 0.5);
                float2 aa = fwidth(g);
                float2 l = 1 - smoothstep(_LineWidth - aa, _LineWidth + aa, d);
                float lineMask = max(l.x, l.y);
                float2 h = saturate(1 - d / (_LineWidth * 8));
                float halo = max(h.x, h.y);
                halo = halo * halo * _Glow;

                float dist = distance(_WorldSpaceCameraPos, i.ws);
                float fade = 1 - saturate((dist - _FadeStart) / max(1, _FadeEnd - _FadeStart));

                half3 col = _BaseColor.rgb + _LineColor.rgb * saturate(lineMask + halo) * fade;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}