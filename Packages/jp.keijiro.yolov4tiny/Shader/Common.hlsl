#ifndef _YOLOV4TINYBARRACUDA_COMMON_H_
#define _YOLOV4TINYBARRACUDA_COMMON_H_

// Compile-time constants
#define MAX_DETECTION 512
#define ANCHOR_COUNT 3

// Detection data structure - The layout of this structure must be matched
// with the one defined in Detection.cs.
struct Detection
{
    float x, y, w, h;
    uint classIndex;
    float score;
};

// Misc math functions

float CalculateIOU(in Detection d1, in Detection d2)
{
    float x0 = max(d1.x - d1.w / 2, d2.x - d2.w / 2);
    float x1 = min(d1.x + d1.w / 2, d2.x + d2.w / 2);
    float y0 = max(d1.y - d1.h / 2, d2.y - d2.h / 2);
    float y1 = min(d1.y + d1.h / 2, d2.y + d2.h / 2);

    float area0 = d1.w * d1.h;
    float area1 = d2.w * d2.h;
    float areaInner = max(0, x1 - x0) * max(0, y1 - y0);

    return areaInner / (area0 + area1 - areaInner);
}

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
