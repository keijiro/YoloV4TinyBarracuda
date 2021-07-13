using UnityEngine;
using Unity.Barracuda;

namespace YoloV4Tiny {

[CreateAssetMenu(fileName = "YoloV4Tiny",
                 menuName = "ScriptableObjects/YoloV4Tiny Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public ComputeShader preprocess;
    public ComputeShader postprocess;
}

} // namespace YoloV4Tiny
