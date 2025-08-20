Shader "Unlit/paint"
{

 
  
     Properties
    {
        _MainTex ("Tile Texture", 2D) = "white" {}
       
        _Opacity("透明度",range(0,1))=0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  // 改为透明类型
            "Queue"="Transparent"    }
    
        Cull Off
          Blend SrcAlpha OneMinusSrcAlpha // 添加alpha混合
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            half  _Opacity;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy* _MainTex_ST.xy+_MainTex_ST.zw;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
             
                // 采样纹理
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // 混合结果
                return texColor;
            }
            ENDCG
        }
    }



}
