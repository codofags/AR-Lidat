Shader "Custom/ColorReplacementShaderWithAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color to Replace", Color) = (1,1,1,1)
        _ReplacementColor ("Replacement Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // включаем поддержку многопоточности для Unity
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _ReplacementColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // Сравниваем компоненты цветов по модулю, используя максимальную разницу
                float threshold = 0.07; // Задаем порог близости цветов
                float maxDiff = max(max(abs(texColor.r - _Color.r), abs(texColor.g - _Color.g)), abs(texColor.b - _Color.b));
                if (maxDiff < threshold)
                {
                    // Если да, то заменяем его на новый цвет с сохранением альфа-канала
                    return _ReplacementColor;
                }
                else
                {
                    // Иначе, возвращаем исходный цвет текселя
                    return texColor;
                }
            }
            ENDCG
        }
    }
}

