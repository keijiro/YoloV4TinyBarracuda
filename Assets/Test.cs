using UnityEngine;
using Klak.TestTools;
using YoloV4Tiny;

public sealed class Test : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UnityEngine.UI.RawImage _preview = null;
    [SerializeField] Shader _shader = null;

    ObjectDetector _detector;
    Material _material;
    ComputeBuffer _drawArgs;

    void Start()
    {
        _detector = new ObjectDetector(_resources);
        _material = new Material(_shader);
        _drawArgs = new ComputeBuffer
          (4, sizeof(uint), ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material);
        _drawArgs.Dispose();
    }

    void LateUpdate()
    {
        _detector.ProcessImage(_source.Texture);
        _detector.SetIndirectDrawCount(_drawArgs);

        _material.SetBuffer("_Detections", _detector.DetectionBuffer);

        Graphics.DrawProceduralIndirect
          (_material, new Bounds(Vector3.zero, Vector3.one * 1000),
           MeshTopology.Triangles, _drawArgs);

        _preview.texture = _source.Texture;
    }
}
