using UnityEngine;
using Unity.Barracuda;

namespace YoloV4Tiny {

[CreateAssetMenu(fileName = "YoloV4Tiny",
                 menuName = "ScriptableObjects/YoloV4Tiny Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public float[] anchors = new float[12];
    public ComputeShader preprocess;
    public ComputeShader postprocess1;
    public ComputeShader postprocess2;
}

} // namespace YoloV4Tiny
