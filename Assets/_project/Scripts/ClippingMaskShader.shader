Shader "Custom/ClippingMaskShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ClippingMask ("Clipping Mask", 2D) = "white" {}
    }
 
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
         
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
         
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
         
            sampler2D _MainTex;
            sampler2D _ClippingMask;
         
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
         
            fixed4 frag(v2f i) : SV_Target
            {
                // Считываем цвет из основной текстуры
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Считываем значение маски обрезки
                fixed4 maskValue = tex2D(_ClippingMask, i.uv);
                
                // Если значение маски меньше 0.5, обрезаем цвет
                if (maskValue.r < 0.5)
                {
                    discard;
                }
                
                return texColor;
            }
            ENDCG
        }
    }
}
