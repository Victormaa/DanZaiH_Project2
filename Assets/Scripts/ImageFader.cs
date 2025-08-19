using UnityEngine;
using UnityEngine.UI;

public class ImageFader : FaderBase
{
    public Image element;

    protected override Color GetColor()
    {
        return element.color;
    }

    protected override void SetColor(Color color)
    {
        element.color = color;
    }
}