Shader "Unlit/RightToLeft_BlackOverlay"
{
    Properties
    {
        _Fade    ("Fade (0=off, 1=full)", Range(0,1)) = 0
        _Feather ("Feather Width", Range(0,0.5)) = 0.1
        _Color   ("Overlay Color", Color) = (0,0,0,1) // เผื่ออยากเปลี่ยนจากดำ
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };
            struct v2f {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float  _Fade;     // 0..1
            float  _Feather;  // 0..0.5
            fixed4 _Color;    // สีของม่าน (default ดำ)

            v2f vert (appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // เส้นขอบจากขวา→ซ้าย
                float edge = 1.0 - _Fade;

                // อัลฟาของม่าน: ขวา=ทึบ, ซ้าย=โปร่ง, ขอบนุ่มด้วย feather
                float a = smoothstep(edge, edge + _Feather, i.uv.x);

                // วาด "สีดำที่มีอัลฟา a" ทับฉากหลัง
                fixed4 col = _Color;
                col.a *= a;
                return col;
            }
            ENDCG
        }
    }
}
