using UnityEngine;
using UnityEngine.UI;

public class PicturesTaken : MonoBehaviour
{
    public Text imageCountText;

    private int _imageCount = 0;

    public void AddToCount(int count = 1)
    {
        _imageCount += count;

        imageCountText.text = $"IMGS| {_imageCount}";
    }
    
}