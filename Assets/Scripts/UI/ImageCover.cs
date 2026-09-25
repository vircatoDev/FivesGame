using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public static class ImageCover
    {
        /// <summary>
        /// Shows the sprite without distortion. With an AspectRatioFitter in EnvelopeParent mode the image
        /// covers its parent, and the parent's mask crops it to the centre.
        /// </summary>
        public static void SetCover(this Image image, Sprite sprite)
        {
            image.sprite = sprite;
            if (sprite != null && image.TryGetComponent<AspectRatioFitter>(out var fitter))
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }
    }
}
