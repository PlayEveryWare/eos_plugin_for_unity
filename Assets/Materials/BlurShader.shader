Shader "Custom/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 3.0
    }
    SubShader
    {
        Tags {"Queue"="Overlay" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            uniform sampler2D _MainTex;
            uniform float _BlurAmount;

            half4 frag(v2f_img i) : COLOR
            {
                half4 color = half4(0,0,0,0);
                float2 uv = i.uv;
                
                // Sample surrounding pixels to create a blur effect
                for(float x = -_BlurAmount; x <= _BlurAmount; x++)
                {
                    for(float y = -_BlurAmount; y <= _BlurAmount; y++)
                    {
                        color += tex2D(_MainTex, uv + float2(x, y) / _ScreenParams.xy);
                    }
                }
                
                color /= (_BlurAmount * 2 + 1) * (_BlurAmount * 2 + 1);
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
