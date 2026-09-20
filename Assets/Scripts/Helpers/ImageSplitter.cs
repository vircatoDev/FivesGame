using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Helpers
{
    class ImageSplitter
    {
        public static List<Texture2D> SplitImage(Texture2D sourceImage, int rows, int cols)
        {
            int pieceWidth = sourceImage.width / cols;
            int pieceHeight = sourceImage.height / rows;
            List<Texture2D> pieces = new List<Texture2D>();

            for (int row = rows-1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    Texture2D piece = new Texture2D(pieceWidth, pieceHeight);
                    piece.SetPixels(sourceImage.GetPixels(col * pieceWidth, row * pieceHeight, pieceWidth, pieceHeight));
                    piece.Apply();
                    pieces.Add(piece);
                }
            }

            return pieces;
        }
    }
}