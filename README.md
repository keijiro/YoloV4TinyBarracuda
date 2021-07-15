YoloV4TinyBarracuda
===================

![screenshot](https://user-images.githubusercontent.com/343936/125791496-bea56f8d-7c7e-4e5e-b8b3-2fcefb45e2c9.png)
![gif](https://user-images.githubusercontent.com/343936/125790218-5c33a411-2a8e-4bbc-bbe3-0dd143a18439.gif)

**YoloV4TinyBarracuda** is an implementation of the [YOLOv4]-tiny object detection model on the [Unity Barracuda] neural network inference library.

[YOLOv4]: https://arxiv.org/abs/2004.10934
[Unity Barracuda]: https://docs.unity3d.com/Packages/com.unity.barracuda@latest

System requirements
-------------------

- Unity 2020.3 LTS or later

About the ONNX file
-------------------

The pre-trained model (YOLOv4-tiny with PASCAL VOC) contained in this package was trained by [Bubbliiiing].
Check the [yolov4-tiny-keras] repository for details.

[Bubbliiiing]: https://github.com/bubbliiiing
[yolov4-tiny-keras]: https://github.com/bubbliiiing/yolov4-tiny-keras

This model was converted into ONNX by [PINTO0309] (Katsuya Hyodo).
I reconverted it into a Barracuda-compatible form using [this Colab notebook].

[PINTO0309]: https://github.com/PINTO0309/PINTO_model_zoo
[this Colab notebook]: https://colab.research.google.com/drive/1YjSQ0IJvKimrc5-I4QXaWJ43-nbPqKOS?usp=sharing
