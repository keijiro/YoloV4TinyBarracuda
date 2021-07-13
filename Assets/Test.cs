using UnityEngine;
using YoloV4Tiny;
using System.Linq;

public sealed class Test : MonoBehaviour
{
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Texture2D _image = null;
    [SerializeField] Shader _shader = null;

    ObjectDetector _detector;
    Material _material1;
    Material _material2;

    void Start()
    {
        _detector = new ObjectDetector(_resources);
        _detector.ProcessImage(_image);

        _material1 = new Material(_shader);
        _material2 = new Material(_shader);

        _material1.SetTexture("_FeatureMap", _detector.FeatureMap1);
        _material2.SetTexture("_FeatureMap", _detector.FeatureMap2);

        _material1.SetInt("_GridCount", 13);
        _material2.SetInt("_GridCount", 26);

        _material1.SetColor("_FillColor", Color.red);
        _material2.SetColor("_FillColor", Color.blue);

        _material1.SetFloatArray
          //("_Anchors", new float [] { 10, 14, 23, 27, 37, 58 });
          ("_Anchors", new float [] { 10, 14, 23, 27, 37, 58 });

        _material2.SetFloatArray
          ("_Anchors", new float [] { 81/4, 82/4, 135/4, 169/4, 344/4, 319/4 });
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material1);
        Destroy(_material2);
    }

    void LateUpdate()
    {
        var bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        Graphics.DrawProcedural
          (_material1, bounds, MeshTopology.Triangles, 6, 13 * 13 * 3);

        Graphics.DrawProcedural
          (_material2, bounds, MeshTopology.Triangles, 6, 26 * 26 * 3);
    }
}
