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

    public float[] MakeAnchorArray(int i1, int i2, int i3)
    {
        var scale = 1.0f / _inputSize;
        return new float[]
          { _resources.anchors[i1 * 2 + 0] * scale,
            _resources.anchors[i1 * 2 + 1] * scale,
            _resources.anchors[i2 * 2 + 0] * scale,
            _resources.anchors[i2 * 2 + 1] * scale,
            _resources.anchors[i3 * 2 + 0] * scale,
            _resources.anchors[i3 * 2 + 1] * scale };
    }

    #endregion

    #region Private objects

    ResourceSet _resources;
    int _inputSize;
    IWorker _worker;

    int FeatureMap1Size
      => _inputSize * _inputSize / (32 * 32);

    int FeatureMap2Size
      => _inputSize * _inputSize / (16 * 16);

    int FeatureDataSize
      => 25 * 3;

    (ComputeBuffer preprocess,
     RenderTexture features1,
     RenderTexture features2) _buffers;

    void AllocateObjects(ResourceSet resources)
    {
        _resources = resources;

        var model = ModelLoader.Load(_resources.model);

        _inputSize = model.inputs[0].shape[6]; // W in (****NHWC)

        _worker = model.CreateWorker();

        _buffers = (new ComputeBuffer(_inputSize * _inputSize * 3, sizeof(float)),
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
        pre.SetInt("Size", _inputSize);
        pre.SetTexture(0, "Image", source);
        pre.SetBuffer(0, "Tensor", _buffers.preprocess);
        pre.DispatchThreads(0, _inputSize, _inputSize, 1);

        // NN worker invocation
        using (var tensor = new Tensor(1, _inputSize, _inputSize, 3,
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
