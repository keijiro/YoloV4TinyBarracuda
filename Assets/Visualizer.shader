Shader "Hidden/YOLOv4-tiny/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Texture2D<float> _FeatureMap;
    uint _GridCount;
    float4 _FillColor;
    float _Anchors[6];

    float Sigmoid(float x)
    {
        return x / (1 + exp(x));
    }

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        uint idx = iid / 3;

        uint ix = idx % _GridCount; 
        uint iy = idx / _GridCount; 

        uint ref_y = (_GridCount - 1 - iy) * _GridCount +
                     (_GridCount - 1 - ix);

        uint anchor = iid % 3;
        uint ref_x = anchor * 25;

        float x = _FeatureMap[uint2(ref_x + 0, ref_y)];
        float y = _FeatureMap[uint2(ref_x + 1, ref_y)];
        float w = _FeatureMap[uint2(ref_x + 2, ref_y)];
        float h = _FeatureMap[uint2(ref_x + 3, ref_y)];
        float c = _FeatureMap[uint2(ref_x + 4, ref_y)];

        c = Sigmoid(c);
        x = Sigmoid(x) / _GridCount;
        y = Sigmoid(y) / _GridCount;
        w = exp(w) * _Anchors[anchor * 2 + 0];
        h = exp(h) * _Anchors[anchor * 2 + 1];

        float vx = idx % _GridCount; 
        float vy = idx / _GridCount; 

        vx = (vx + 0.5) / _GridCount;
        vy = (vy + 0.5) / _GridCount;

        vx += w * lerp(-0.5, 0.5, vid & 1);
        vy += h * lerp(-0.5, 0.5, vid < 2 || vid == 5);

        vx =  x +     vx * 2 - 1;
        vy = -y + 1 - vy * 2;

        // Aspect ratio compensation
        vx = vx * _ScreenParams.y / _ScreenParams.x;

        // Vertex attributes
        position = float4(vx, vy, 1, 1);
        color = float4(1, 1, 1, c) * _FillColor;
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
