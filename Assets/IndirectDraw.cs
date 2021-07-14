using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using YoloV4Tiny;

public sealed class IndirectDraw : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField, Range(0, 1)] float _threshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] RawImage _preview = null;
    [SerializeField] Shader _shader = null;

    ObjectDetector _detector;
    ComputeBuffer _drawArgs;
    Material _material;

    Bounds UnitBox => new Bounds(Vector3.zero, Vector3.one);

    void Start()
    {
        _detector = new ObjectDetector(_resources);
        _drawArgs = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});
        _material = new Material(_shader);
    }

    void OnDestroy()
    {
        _detector.Dispose();
        _drawArgs.Dispose();
        Destroy(_material);
    }

    void LateUpdate()
    {
        _detector.ProcessImage(_source.Texture, _threshold);
        _detector.SetIndirectDrawCount(_drawArgs);
        _material.SetBuffer("_Detections", _detector.DetectionBuffer);
        Graphics.DrawProceduralIndirect(_material, UnitBox, MeshTopology.Triangles, _drawArgs);
        _preview.texture = _source.Texture;
    }
}
