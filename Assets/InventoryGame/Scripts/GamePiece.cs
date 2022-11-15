using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.InventoryGame
{
    public class GamePiece : MonoBehaviour
    {
        public const int TileSize = 128;

        [SerializeField] private int tileValue = 1;
        [SerializeField] private Color tileColor = Color.white;

        /// <summary>
        /// Matrix of the blocks by [row, col].
        /// </summary>
        private int[,] blockMatrix;

        public List<RectTransform> BlockTransforms { get; private set; } = new List<RectTransform>();

        public InventoryGameManager BoardManager { get; set; }

        public int TileValue => tileValue;

        private void Start()
        {
            // Given the current sprite layout for the game object, generate the pieces matrix.
            // This assumes the bottom left corner is the origin, so the matrix is flipped
            // compared to the visual block positions.
            // Scan and add each child object and get the longest width/height for the NxN matrix.
            int longestSide = 1;
            foreach (RectTransform t in transform)
            {
                BlockTransforms.Add(t);
                longestSide = Mathf.Max((int)t.anchoredPosition.x / TileSize + 1, longestSide);
                longestSide = Mathf.Max((int)t.anchoredPosition.y / TileSize + 1, longestSide);
            }

            // Create the NxN matrix based on the longest length and store the tile value
            blockMatrix = new int[longestSide, longestSide];
            foreach (RectTransform t in BlockTransforms)
            {
                int row = (int)t.anchoredPosition.y / TileSize;
                int col = (int)t.anchoredPosition.x / TileSize;
                blockMatrix[row, col] = tileValue;

                // While setting the tile values, also set the color
                t.GetComponent<Image>().color = tileColor;
            }
        }

        /// <summary>
        /// When dragging begins, clear any blocked tiles so it doesn't block the view.
        /// </summary>
        public void BeginDrag()
        {
            BoardManager.ClearBlockedTiles();
        }

        /// <summary>
        /// Once dragging is complete, attempt tp place the piece on the board.
        /// </summary>
        public void TryPlacePiece()
        {
            BoardManager.TryPlacePiece(this);
        }

        /// <summary>
        /// Rotates a piece 90 degrees.
        /// </summary>
        public void RotatePiece()
        {
            // Transpose the matrix along the (x,x) axis
            for (int r = 0; r < blockMatrix.GetLength(0); r++)
            {
                for (int c = r + 1; c < blockMatrix.GetLength(1); c++)
                {
                    var temp = blockMatrix[r, c];
                    blockMatrix[r, c] = blockMatrix[c, r];
                    blockMatrix[c, r] = temp;
                }
            }

            // Reverse each row to get the final rotated matrix
            for (int r = 0; r < blockMatrix.GetLength(0); r++)
            {
                for (int c = 0; c < blockMatrix.GetLength(1) / 2; c++)
                {
                    var temp = blockMatrix[r, c];
                    blockMatrix[r, c] = blockMatrix[r, blockMatrix.GetLength(1) - 1 - c];
                    blockMatrix[r, blockMatrix.GetLength(1) - 1 - c] = temp;
                }
            }

            // Update the visual block locations based on the new matrix
            int currentIndex = 0;
            for (int r = 0; r < blockMatrix.GetLength(0); r++)
            {
                for (int c = 0; c < blockMatrix.GetLength(1); c++)
                {
                    if (blockMatrix[r, c] != 0)
                    {
                        BlockTransforms[currentIndex].anchoredPosition = new Vector2(c * TileSize, r * TileSize);
                        currentIndex++;
                    }
                }
            }
        }
    }
}
