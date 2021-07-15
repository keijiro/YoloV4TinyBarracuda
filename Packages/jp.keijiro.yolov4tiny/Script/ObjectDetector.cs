using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace YoloV4Tiny {

public sealed class ObjectDetector : System.IDisposable
{
    #region Public methods/properties

    public ObjectDetector(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture, float threshold)
      => RunModel(sourceTexture, threshold);

    public IEnumerable<Detection> Detections
      => _readCache.Cached;

    public ComputeBuffer DetectionBuffer
      => _buffers.post2;

    public ComputeBuffer DetectionCountBuffer
      => _buffers.countRead;

    public void SetIndirectDrawCount(ComputeBuffer drawArgs)
      => ComputeBuffer.CopyCount(_buffers.post2, drawArgs, sizeof(uint));

    #endregion

    #region Private objects

    ResourceSet _resources;
    Config _config;
    IWorker _worker;

    (ComputeBuffer preprocess,
     RenderTexture feature1,
     RenderTexture feature2,
     ComputeBuffer post1,
     ComputeBuffer post2,
     ComputeBuffer counter,
     ComputeBuffer countRead) _buffers;

    DetectionCache _readCache;

    void AllocateObjects(ResourceSet resources)
    {
        // NN model loading
        var model = ModelLoader.Load(resources.model);

        // Private object initialization
        _resources = resources;
        _config = new Config(resources, model);
        _worker = model.CreateWorker();

        // Buffer allocation
        _buffers.preprocess = new ComputeBuffer
          (_config.InputFootprint, sizeof(float));

        _buffers.feature1 = RTUtil.NewFloat
          (_config.FeatureDataSize, _config.FeatureMap1Footprint);

        _buffers.feature2 = RTUtil.NewFloat
          (_config.FeatureDataSize, _config.FeatureMap2Footprint);

        _buffers.post1 = new ComputeBuffer
          (Config.MaxDetection, Detection.Size);

        _buffers.post2 = new ComputeBuffer
          (Config.MaxDetection, Detection.Size, ComputeBufferType.Append);

        _buffers.counter = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Counter);

        _buffers.countRead = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        // Detection data read cache initialization
        _readCache = new DetectionCache(_buffers.post2, _buffers.countRead);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _buffers.preprocess?.Dispose();
        _buffers.preprocess = null;

        ObjectUtil.Destroy(_buffers.feature1);
        _buffers.feature1 = null;

        ObjectUtil.Destroy(_buffers.feature2);
        _buffers.feature2 = null;

        _buffers.post1?.Dispose();
        _buffers.post1 = null;

        _buffers.post2?.Dispose();
        _buffers.post2 = null;

        _buffers.counter?.Dispose();
        _buffers.counter = null;

        _buffers.countRead?.Dispose();
        _buffers.countRead = null;
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source, float threshold)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInt("Size", _config.InputWidth);
        pre.SetTexture(0, "Image", source);
        pre.SetBuffer(0, "Tensor", _buffers.preprocess);
        pre.DispatchThreads(0, _config.InputWidth, _config.InputWidth, 1);

        // NN worker invocation
        using (var t = new Tensor(_config.InputShape, _buffers.preprocess))
            _worker.Execute(t);

        // NN output retrieval
        _worker.CopyOutput("Identity", _buffers.feature1);
        _worker.CopyOutput("Identity_1", _buffers.feature2); 

        // Counter buffer reset
        _buffers.post2.SetCounterValue(0);
        _buffers.counter.SetCounterValue(0);

        // First stage postprocessing: detection data aggregation
        var post1 = _resources.postprocess1;
        post1.SetInt("ClassCount", _config.ClassCount);
        post1.SetFloat("Threshold", threshold);
        post1.SetBuffer(0, "Output", _buffers.post1);
        post1.SetBuffer(0, "OutputCount", _buffers.counter);

        // (feature map 1)
        var width1 = _config.FeatureMap1Width;
        post1.SetTexture(0, "Input", _buffers.feature1);
        post1.SetInt("InputSize", width1);
        post1.SetFloats("Anchors", _config.AnchorArray1);
        post1.DispatchThreads(0, width1, width1, 1);

        // (feature map 2)
        var width2 = _config.FeatureMap2Width;
        post1.SetTexture(0, "Input", _buffers.feature2);
        post1.SetInt("InputSize", width2);
        post1.SetFloats("Anchors", _config.AnchorArray2);
        post1.DispatchThreads(0, width2, width2, 1);

        // Second stage postprocessing: overlap removal
        var post2 = _resources.postprocess2;
        post2.SetFloat("Threshold", 0.5f);
        post2.SetBuffer(0, "Input", _buffers.post1);
        post2.SetBuffer(0, "InputCount", _buffers.counter);
        post2.SetBuffer(0, "Output", _buffers.post2);
        post2.Dispatch(0, 1, 1, 1);

        // Bounding box count after removal
        ComputeBuffer.CopyCount(_buffers.post2, _buffers.countRead, 0);

        // Cache data invalidation
        _readCache.Invalidate();
    }

    #endregion
}

} // namespace YoloV4Tiny
