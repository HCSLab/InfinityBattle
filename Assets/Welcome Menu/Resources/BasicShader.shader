Shader "BasicShader"{
    //声明本例使用的各个属性
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
        _Brightness("Brightness", Float) = 1
        _Saturation("Saturation", Float) = 1
        _Contrast("Contrast", Float) = 1
    }

    //定义用于屏幕后处理的pass
    SubShader{
        Pass{
            //关闭深度写入
            //为了防止它“挡住”在其后面被渲染的物体。
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //在CG代码块中声明对应的变量
            sampler2D _MainTex;
            half _Brightness;
            half _Saturation;
            half _Contrast;

            //定义顶点着色器
            struct v2f{
                float4 pos : SV_POSITION;
                half2 uv: TEXCOORD0;
            };

            v2f vert(appdata_img v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            //定义片元着色器
            fixed4 frag(v2f i) : SV_Target{
                fixed4 renderTex = tex2D(_MainTex, i.uv);
                //Apply brightness
                fixed3 finalColor = renderTex.rgb * _Brightness;

                //Apply saturation
                //['luːmɪn(ə)ns]亮度
                fixed luminance = 0.2125 * renderTex.r + 0.7154 * renderTex.g + 0.0721 * renderTex.b;
                fixed3 luminanceColor = fixed3(luminance, luminance, luminance);
                finalColor = lerp(luminanceColor, finalColor, _Saturation);

                //Apply contrast
                fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
                finalColor = lerp(avgColor, finalColor, _Contrast);

                return fixed4(finalColor, renderTex.a);
            }

            ENDCG
        }
    }

    Fallback Off
}