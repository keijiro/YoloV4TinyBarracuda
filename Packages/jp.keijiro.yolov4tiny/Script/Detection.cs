using System.Runtime.InteropServices;

namespace YoloV4Tiny {

// Detection data structure - The layout of this structure must be matched
// with the one defined in Common.hlsl.
[StructLayout(LayoutKind.Sequential)]
public readonly struct Detection
{
    public readonly float x, y, w, h;
    public readonly uint classIndex;
    public readonly float score;

    // sizeof(Detection)
    public static int Size = 6 * sizeof(int);

    // String formatting
    public override string ToString()
      => $"({x},{y})-({w}x{h}):{classIndex}({score})";
};

} // namespace YoloV4Tiny
