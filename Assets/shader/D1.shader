Shader "Unlit/D1"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "grey" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _TintIntensity ("Tint Intensity", Range(0,1)) = 0.5
        _Roughness ("Roughness", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma instancing_options assumeuniformscaling
        
        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        fixed4 _Color;
        float _TintIntensity;
        float _Roughness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 采样原始纹理
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            
            // 颜色混合公式
            fixed3 tinted = lerp(tex.rgb, tex.rgb * _Color.rgb, _TintIntensity);
            
            o.Albedo = tinted;
            o.Metallic = 0;
            o.Smoothness = 1 - _Roughness;
            o.Alpha = tex.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}