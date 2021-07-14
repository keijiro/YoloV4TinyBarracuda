using UnityEngine;
using UnityEngine.UI;
using YoloV4Tiny;

sealed class Marker : MonoBehaviour
{
    RectTransform _parent;
    RectTransform _xform;
    Image _panel;
    Text _label;

    public static string[] _labels = new[]
    {
        "Plane", "Bicycle", "Bird", "Boat",
        "Bottle", "Bus", "Car", "Cat",
        "Chair", "Cow", "Table", "Dog",
        "Horse", "Motorbike", "Person", "Plant",
        "Sheep", "Sofa", "Train", "TV"
    };

    void Start()
    {
        _xform = GetComponent<RectTransform>();
        _parent = (RectTransform)_xform.parent;
        _panel = GetComponent<Image>();
        _label = GetComponentInChildren<Text>();
    }

    public void SetAttributes(in Detection d)
    {
        var rect = _parent.rect;

        // Bounding box position
        var x = d.x * rect.width;
        var y = (1 - d.y) * rect.height;
        var w = d.w * rect.width;
        var h = d.h * rect.height;

        _xform.anchoredPosition = new Vector2(x, y);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // Label (class name + score)
        var name = _labels[(int)d.classIndex];
        _label.text = $"{name} {(int)(d.score * 100)}%";

        // Panel color
        var hue = d.classIndex * 0.073f % 1.0f;
        var color = Color.HSVToRGB(hue, 1, 1);
        color.a = 0.4f;
        _panel.color = color;

        // Enable
        gameObject.SetActive(true);
    }

    public void Hide()
      => gameObject.SetActive(false);
}
