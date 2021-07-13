Shader "Hidden/YOLOv4-tiny/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Texture2D<float> _FeatureMap;
    uint _GridCount;
    float4 _FillColor;

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        uint idx = iid / 3;
        uint anchor = iid % 3;

        float sc = _FeatureMap[uint2(25 * anchor + 4, idx)];

        sc = 1 / (1 + exp(-sc));

        float x = idx % _GridCount; 
        float y = idx / _GridCount; 

        x += 0.5;
        y += 0.5;

        x += lerp(-0.5, 0.5, vid & 1);
        y += lerp(-0.5, 0.5, vid < 2 || vid == 5);

        x = 1 - x / _GridCount * 2;
        y =     y / _GridCount * 2 - 1;

        // Aspect ratio compensation
        x = x * _ScreenParams.y / _ScreenParams.x;

        // Vertex attributes
        position = float4(x, y, 1, 1);
        color = float4(1, 1, 1, sc) * _FillColor;
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        return color * color.a;
    }

    ENDCG

    SubShader
    {
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
