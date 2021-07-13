using Unity.Barracuda;
using UnityEngine;

namespace YoloV4Tiny {

public sealed class ObjectDetector : System.IDisposable
{
    #region Public members

    public ObjectDetector(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture)
      => RunModel(sourceTexture);

    public RenderTexture FeatureMap1
      => _buffers.features1;

    public RenderTexture FeatureMap2
      => _buffers.features2;

    public Vector4[] MakeAnchorArray(int i1, int i2, int i3)
      => new [] { GetAnchor(i1), GetAnchor(i2), GetAnchor(i3) };

    #endregion

    #region Private objects

    ResourceSet _resources;
    (int w, int h) _size;
    IWorker _worker;

    int FeatureMap1Size
      => (_size.w / 32) * (_size.h / 32);

    int FeatureMap2Size
      => (_size.w / 16) * (_size.h / 16);

    int FeatureDataSize
      => 25 * 3;

    (ComputeBuffer preprocess,
     RenderTexture features1,
     RenderTexture features2) _buffers;

    Vector4 GetAnchor(int i)
      => new Vector4((float)_resources.anchors[i * 2 + 0] / _size.w,
                     (float)_resources.anchors[i * 2 + 1] / _size.h, 0, 0);

    void AllocateObjects(ResourceSet resources)
    {
        _resources = resources;

        var model = ModelLoader.Load(_resources.model);

        var shape = model.inputs[0].shape; // NHWC
        _size = (shape[6], shape[5]);      // (W, H)

        _worker = model.CreateWorker();

        _buffers = (new ComputeBuffer(_size.w * _size.h * 3, sizeof(float)),
                    RTUtil.NewFloat(FeatureDataSize, FeatureMap1Size),
                    RTUtil.NewFloat(FeatureDataSize, FeatureMap2Size));
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _buffers.preprocess?.Dispose();
        _buffers.preprocess = null;

        ObjectUtil.Destroy(_buffers.features1);
        _buffers.features1 = null;

        ObjectUtil.Destroy(_buffers.features2);
        _buffers.features2 = null;
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInts("Size", _size.w, _size.h);
        pre.SetTexture(0, "Image", source);
        pre.SetBuffer(0, "Tensor", _buffers.preprocess);
        pre.DispatchThreads(0, _size.w, _size.h, 1);

        // NN worker invocation
        using (var tensor = new Tensor(1, _size.h, _size.w, 3,
                                       _buffers.preprocess))
            _worker.Execute(tensor);

        // Postprocessing
        using (var tensor = _worker.PeekOutput("Identity")
          .Reshape(new TensorShape(1, FeatureMap1Size, FeatureDataSize, 1)))
            tensor.ToRenderTexture(_buffers.features1);

        using (var tensor = _worker.PeekOutput("Identity_1")
          .Reshape(new TensorShape(1, FeatureMap2Size, FeatureDataSize, 1)))
            tensor.ToRenderTexture(_buffers.features2);
    }

    #endregion
}

} // namespace YoloV4Tiny
