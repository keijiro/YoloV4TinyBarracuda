using UnityEngine;
using Klak.TestTools;
using YoloV4Tiny;
using System.Linq;

public sealed class Test : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UnityEngine.UI.RawImage _preview = null;
    [SerializeField] Shader _shader = null;

    ObjectDetector _detector;
    Material _material1;
    Material _material2;

    void Start()
    {
        _detector = new ObjectDetector(_resources);

        _material1 = new Material(_shader);
        _material2 = new Material(_shader);

        _material1.SetTexture("_FeatureMap", _detector.FeatureMap1);
        _material2.SetTexture("_FeatureMap", _detector.FeatureMap2);

        _material1.SetInt("_GridCount", 13);
        _material2.SetInt("_GridCount", 26);

        _material1.SetColor("_FillColor", Color.red);
        _material2.SetColor("_FillColor", Color.blue);

        _material1.SetVectorArray("_Anchors", _detector.MakeAnchorArray(3, 4, 5));
        _material2.SetVectorArray("_Anchors", _detector.MakeAnchorArray(1, 2, 3));
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material1);
        Destroy(_material2);
    }

    void LateUpdate()
    {
        _detector.ProcessImage(_source.Texture);

        var bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        Graphics.DrawProcedural
          (_material1, bounds, MeshTopology.Triangles, 6, 13 * 13 * 3);

        Graphics.DrawProcedural
          (_material2, bounds, MeshTopology.Triangles, 6, 26 * 26 * 3);

        _preview.texture = _source.Texture;
    }
}
