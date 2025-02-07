using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

//Manages the gravity effect, making blocks fall down to fill empty spaces in the grid.
public class GravityManager
{
    #region Variables
    private BoardManager boardManager;
    private Stack<Vector3> vector3Pool = new Stack<Vector3>();
    #endregion

    public GravityManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    public void ApplyGravity() //Applies gravity to the blocks in the grid
    {
        Block[,] grid = boardManager.grid;
        int rows = boardManager.currentLevelData.rows;
        int cols = boardManager.currentLevelData.cols;

        for (int c = 0; c < cols; c++)
        {
            int writeRow = 0;
            for (int r = 0; r < rows; r++)
            {
                if (grid[r, c] != null)
                {
                    if (r != writeRow)
                    {
                        Block block = grid[r, c];
                        grid[writeRow, c] = block;
                        grid[r, c] = null;

                        block.row = writeRow;
                        Vector3 newPos = boardManager.gridManager.CalculateBlockPosition(writeRow, c);

                        boardManager.blocksToCheck.Add(grid[writeRow, c]);
                        grid[writeRow, c].transform.DOLocalMove(newPos, 0.2f).SetEase(Ease.OutBounce);

                    }
                    writeRow++;
                }
            }
        }
    }
}

