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

    void Start()
    {
        _detector = new ObjectDetector(_resources);
        _material = new Material(_shader);
        _material.SetColor("_FillColor", Color.red);
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material);
    }

    void LateUpdate()
    {
        _detector.ProcessImage(_source.Texture);

        _material.SetBuffer("_Detections", _detector.DetectionBuffer);
        _material.SetBuffer("_DetectionCount", _detector.DetectionCountBuffer);

        Graphics.DrawProcedural
          (_material, new Bounds(Vector3.zero, Vector3.one * 1000),
           MeshTopology.Triangles, 6, 256);

        _preview.texture = _source.Texture;
    }
}
