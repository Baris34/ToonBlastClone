using DG.Tweening;
using UnityEngine;

//Manages the grid refilling process after blocks are removed.
public class RefillManager
{
    #region Variables
    private BoardManager boardManager;
    private Block[,] grid;
    private int rows;
    private int cols;
    #endregion


    public RefillManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
        this.grid = boardManager.grid;
        this.rows = boardManager.currentLevelData.rows;
        this.cols = boardManager.currentLevelData.cols;
    }


    public void Refill() //Refills the grid with new blocks after blocks are removed.
    {
        float animTime = 0.3f;

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (grid[r, c] == null)
                {
                    CreateAndDropBlock(r, c, animTime);
                }
            }
        }
    }

    private void CreateAndDropBlock(int row, int col, float animTime) //Creates a new block and drops it into the grid.
    {
        int rand = Random.Range(0, boardManager.blockFlyweights.Count);
        BlockFlyweight fw = boardManager.blockFlyweights[rand];

        Block newBlock = boardManager.poolManager.GetBlockFromPool();
        newBlock.transform.SetParent(boardManager.BlockContainer.transform, false);

        Vector3 targetPos = boardManager.gridManager.CalculateBlockPosition(row, col);
        Vector3 startPos = targetPos + new Vector3(0, 2f, 0);

        newBlock.transform.localPosition = startPos;
        newBlock.transform.DOLocalMove(targetPos, animTime).SetEase(Ease.OutBounce);

        newBlock.Initialize(fw, row, col);
        newBlock.boardManager = boardManager;

        grid[row, col] = newBlock;

        boardManager.blocksToCheck.Add(newBlock);
        boardManager.groupManager.AddNearbyBlocksToCheck(row, col, 0); // Consider moving this to GroupManager if it's related to combo updates
    }
    
}