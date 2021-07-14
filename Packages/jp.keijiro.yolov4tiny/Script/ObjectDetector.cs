using Unity.Barracuda;
using UnityEngine;

namespace YoloV4Tiny {

public sealed class ObjectDetector : System.IDisposable
{
    #region Public members

    public const int MaxDetection = 256;

    public ObjectDetector(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture)
      => RunModel(sourceTexture);

    public int InputSize => _inputSize;
    public int ClassCount => _classCount;
    public ComputeBuffer DetectionBuffer => _buffers.post2;
    public ComputeBuffer DetectionCountBuffer => _buffers.count;

    #endregion

    #region Private objects

    ResourceSet _resources;
    int _inputSize;
    int _classCount;
    IWorker _worker;

    const int AnchorCount = 3;

    int FeatureMap1Size
      => _inputSize * _inputSize / (32 * 32);

    int FeatureMap2Size
      => _inputSize * _inputSize / (16 * 16);

    int FeatureDataSize
      => (5 + _classCount) * AnchorCount;

    float[] MakeAnchorArray(int i1, int i2, int i3)
    {
        var scale = 1.0f / _inputSize;
        return new float[]
          { _resources.anchors[i1 * 2 + 0] * scale,
            _resources.anchors[i1 * 2 + 1] * scale, 0, 0,
            _resources.anchors[i2 * 2 + 0] * scale,
            _resources.anchors[i2 * 2 + 1] * scale, 0, 0,
            _resources.anchors[i3 * 2 + 0] * scale,
            _resources.anchors[i3 * 2 + 1] * scale, 0, 0 };
    }

    (ComputeBuffer preprocess,
     RenderTexture features1,
     RenderTexture features2,
     ComputeBuffer post1,
     ComputeBuffer post2,
     ComputeBuffer count) _buffers;

    void AllocateObjects(ResourceSet resources)
    {
        _resources = resources;

        var model = ModelLoader.Load(_resources.model);

        _inputSize = model.inputs[0].shape[6]; // W in (****NHWC)

        var outc = model.GetShapeByName(model.outputs[0]).Value.channels;
        _classCount = outc / AnchorCount - 5;

        _worker = model.CreateWorker();

        _buffers.preprocess = new ComputeBuffer
          (_inputSize * _inputSize * 3, sizeof(float));

        _buffers.features1 = RTUtil.NewFloat(FeatureDataSize, FeatureMap1Size);
        _buffers.features2 = RTUtil.NewFloat(FeatureDataSize, FeatureMap2Size);

        _buffers.post1 = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _buffers.post2 = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _buffers.count = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);
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

        _buffers.post1?.Dispose();
        _buffers.post1 = null;

        _buffers.post2?.Dispose();
        _buffers.post2 = null;

        _buffers.count?.Dispose();
        _buffers.count = null;
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

        // First stage postprocessing (detection data aggregation)
        _buffers.post1.SetCounterValue(0);

        var post1 = _resources.postprocess1;
        post1.SetTexture(0, "Input", _buffers.features1);
        post1.SetInt("InputSize", 13);
        post1.SetInt("ClassCount", _classCount);
        post1.SetFloats("Anchors", MakeAnchorArray(3, 4, 5));
        post1.SetFloat("Threshold", 0.1f);
        post1.SetBuffer(0, "Output", _buffers.post1);
        post1.DispatchThreads(0, 13, 13, 1);

        post1.SetTexture(0, "Input", _buffers.features2);
        post1.SetInt("InputSize", 26);
        post1.SetFloats("Anchors", MakeAnchorArray(1, 2, 3));
        post1.DispatchThreads(0, 26, 26, 1);

        // Detection count
        ComputeBuffer.CopyCount(_buffers.post1, _buffers.count, 0);

        // Second stage postprocessing (overlap removal)
        _buffers.post2.SetCounterValue(0);

        var post2 = _resources.postprocess2;
        post2.SetFloat("Threshold", 0.5f);
        post2.SetBuffer(0, "Input", _buffers.post1);
        post2.SetBuffer(0, "Count", _buffers.count);
        post2.SetBuffer(0, "Output", _buffers.post2);
        post2.Dispatch(0, 1, 1, 1);

        // Bounding box count after removal
        ComputeBuffer.CopyCount(_buffers.post2, _buffers.count, 0);
    }

    #endregion
}

} // namespace YoloV4Tiny
