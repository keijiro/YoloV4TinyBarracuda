using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Barracuda;

namespace YoloV4Tiny {

public sealed class Test : MonoBehaviour
{
    [SerializeField] NNModel _model;
    [SerializeField] Texture2D _image;

    void Start()
    {
        // Input image -> Tensor (1, 416, 416, 3)
        var source = new float[416 * 416 * 3];

        var offs = 0;
        for (var y = 0; y < 416; y++)
        {
            for (var x = 0; x < 416; x++)
            {
                var p = _image.GetPixel(x, 415 - y);
                source[offs++] = p.r;
                source[offs++] = p.g;
                source[offs++] = p.b;
            }
        }

        // Inference
        var model = ModelLoader.Load(_model);
        using var worker = WorkerFactory.CreateWorker(model);

        using (var tensor = new Tensor(1, 416, 416, 3, source))
            worker.Execute(tensor);

        // Visualization
        var layer1 = worker.PeekOutput("Identity");
        var layer2 = worker.PeekOutput("Identity_1");

        var vtx = new List<Vector3>();

        for (var yi = 0; yi < 13; yi++)
        {
            for (var xi = 0; xi < 13; xi++)
            {
                for (var ai = 0; ai < 3; ai++)
                {
                    var sc = layer1[0, yi, xi, ai * 25 + 4];
                    sc = 1 / (1 + Mathf.Exp(-sc));
                    if (sc < 0.1f) continue;

                    var x1 =     (xi * 32 - 16) / 416.0f;
                    var y1 = 1 - (yi * 32 - 16) / 416.0f;
                    var x2 =     (xi * 32 + 16) / 416.0f;
                    var y2 = 1 - (yi * 32 + 16) / 416.0f;

                    vtx.Add(new Vector3(x1, y1, 0));
                    vtx.Add(new Vector3(x2, y1, 0));
                    vtx.Add(new Vector3(x2, y2, 0));
                    vtx.Add(new Vector3(x1, y2, 0));
                }
            }
        }

        for (var yi = 0; yi < 26; yi++)
        {
            for (var xi = 0; xi < 26; xi++)
            {
                for (var ai = 0; ai < 3; ai++)
                {
                    var sc = layer2[0, yi, xi, ai * 25 + 4];
                    sc = 1 / (1 + Mathf.Exp(-sc));
                    if (sc < 0.1f) continue;

                    var x1 =     (xi * 16 - 8) / 416.0f;
                    var y1 = 1 - (yi * 16 - 8) / 416.0f;
                    var x2 =     (xi * 16 + 8) / 416.0f;
                    var y2 = 1 - (yi * 16 + 8) / 416.0f;

                    vtx.Add(new Vector3(x1, y1, 0));
                    vtx.Add(new Vector3(x2, y1, 0));
                    vtx.Add(new Vector3(x2, y2, 0));
                    vtx.Add(new Vector3(x1, y2, 0));
                }
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(vtx);
        mesh.SetIndices(Enumerable.Range(0, vtx.Count).ToArray(), MeshTopology.Quads, 0);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}

} // namespace YoloV4Tiny
