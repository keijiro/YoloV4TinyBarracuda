Shader "Hidden/YOLOv4-tiny/SimpleFill"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Packages/jp.keijiro.yolov4tiny/Shader/Common.hlsl"

    StructuredBuffer<Detection> _Detections;

    float3 GetClassColor(uint i)
    {
        float h = frac(i * 0.74) * 6 - 2;
        return saturate(float3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)));
    }

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        Detection d = _Detections[iid];

        float x = d.x + d.w * lerp(-0.5, 0.5, vid & 1);
        float y = d.y + d.h * lerp(-0.5, 0.5, vid < 2 || vid == 5);

        x = 2 * x - 1;
        y = 1 - y * 2;

        // Aspect ratio compensation
        x = x * _ScreenParams.y / _ScreenParams.x;

        // Vertex attributes
        position = float4(x, y, 1, 1);
        color = float4(GetClassColor(d.classIndex), d.score);
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        return color * color.a;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Overlay+100" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend One One
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
