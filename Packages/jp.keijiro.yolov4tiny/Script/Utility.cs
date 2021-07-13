using UnityEngine;

namespace YoloV4Tiny {

static class ObjectUtil
{
    public static void Destroy(Object o)
    {
        if (o == null) return;
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}

static class RTUtil
{
    public static RenderTexture NewFloat(int w, int h)
      => new RenderTexture(w, h, 0, RenderTextureFormat.RFloat);

    public static RenderTexture NewFloat4(int w, int h)
      => new RenderTexture(w, h, 0, RenderTextureFormat.ARGBFloat);

    public static RenderTexture NewFloat4UAV(int w, int h)
    {
        var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBFloat);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

static class ComputeShaderExtensions
{
    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);

        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;

        compute.Dispatch(kernel, x, y, z);
    }
}

} // namespace YoloV4Tiny
