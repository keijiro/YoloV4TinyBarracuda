using Unity.Barracuda;
using UnityEngine;

namespace YoloV4Tiny {

struct Config
{
    #region Compile-time constants

    // These values must be matched with the ones defined in Common.hlsl.
    public const int MaxDetection = 512;
    public const int AnchorCount = 3;

    #endregion

    #region Variables from tensor shapes

    public int InputWidth { get; private set; }
    public int ClassCount { get; private set; }
    public int FeatureMap1Width { get; private set; }
    public int FeatureMap2Width { get; private set; }

    #endregion

    #region Data size calculation properties

    public int FeatureDataSize => (5 + ClassCount) * AnchorCount;
    public int InputFootprint => InputWidth * InputWidth * 3;
    public int FeatureMap1Footprint => FeatureMap1Width * FeatureMap1Width;
    public int FeatureMap2Footprint => FeatureMap2Width * FeatureMap2Width;

    #endregion

    #region Tensor shape utilities

    public TensorShape InputShape
      => new TensorShape(1, InputWidth, InputWidth, 3);

    public TensorShape FlattenFeatureMap1
      => new TensorShape(1, FeatureMap1Footprint, FeatureDataSize, 1);

    public TensorShape FlattenFeatureMap2
      => new TensorShape(1, FeatureMap2Footprint, FeatureDataSize, 1);

    #endregion

    #region Anchor arrays (16 byte aligned for compute shader use)

    public float[] AnchorArray1 { get; private set; }
    public float[] AnchorArray2 { get; private set; }

    static float[] MakeAnchorArray
      (float[] anchors, int i1, int i2, int i3, float scale)
      => new float[]
          { anchors[i1 * 2 + 0] * scale, anchors[i1 * 2 + 1] * scale, 0, 0,
            anchors[i2 * 2 + 0] * scale, anchors[i2 * 2 + 1] * scale, 0, 0,
            anchors[i3 * 2 + 0] * scale, anchors[i3 * 2 + 1] * scale, 0, 0 };

    #endregion

    #region Constructor

    public Config(ResourceSet resources, Model model)
    {
        var inShape = model.inputs[0].shape;
        var out1Shape = model.GetShapeByName(model.outputs[0]).Value;
        var out2Shape = model.GetShapeByName(model.outputs[1]).Value;

        InputWidth = inShape[6]; // 6: width
        ClassCount = out1Shape.channels / AnchorCount - 5;
        FeatureMap1Width = out1Shape.width;
        FeatureMap2Width = out2Shape.width;

        var scale = 1.0f / InputWidth;
        AnchorArray1 = MakeAnchorArray(resources.anchors, 3, 4, 5, scale);
        AnchorArray2 = MakeAnchorArray(resources.anchors, 1, 2, 3, scale);
    }

    #endregion
}


} // namespace YoloV4Tiny
