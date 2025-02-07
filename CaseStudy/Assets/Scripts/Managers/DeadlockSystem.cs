using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//Manages deadlock detection and shuffling of the game board.
public class DeadlockSystem : MonoBehaviour
{

    private float shuffleTimer = 0.3f;
    #region CollectionPool
    private Stack<Queue<Vector2Int>> vector2IntQueuePool = new Stack<Queue<Vector2Int>>();
    private Stack<List<Block>> blockListPool = new Stack<List<Block>>();

    private Queue<Vector2Int> GetQueueFromPool(int capacity)
    {
        if (vector2IntQueuePool.Count > 0)
        {
            Queue<Vector2Int> queue = vector2IntQueuePool.Pop();
            queue.Clear();
            return queue;
        }
        else
        {
            return new Queue<Vector2Int>(capacity);
        }
    }

    private void ReturnQueueToPool(Queue<Vector2Int> queue)
    {
        vector2IntQueuePool.Push(queue);
    }

    private List<Block> GetListFromPool()
    {
        if (blockListPool.Count > 0)
        {
            return blockListPool.Pop();
        }
        else
        {
            return new List<Block>();
        }
    }

    private void ReturnListToPool(List<Block> list)
    {
        list.Clear();
        blockListPool.Push(list);
    }
    #endregion



    #region Generic Fisher-Yates Shuffle
    private void FisherYatesShuffle<T>(List<T> list) // Generic Fisher-Yates Shuffle
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
    #endregion
    public bool CheckNoMoves(Block[,] grid, BoardManager bm) //Checks if there are any possible moves left on the board.
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;
        bool[,] visited = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!visited[r, c] && grid[r, c] != null)
                {
                    List<Block> group = FloodFill(r, c, grid[r, c].flyweight, visited, grid, bm);
                    if (group.Count >= 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true; 
    }

    private List<Block> FloodFill(int startRow, int startCol, BlockFlyweight fw, //Flood-fills the grid to find groups of blocks.
                                      bool[,] visited, Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;
        List<Block> group = GetListFromPool();
        group.Clear();

        if (startRow < 0 || startRow >= rows || startCol < 0 || startCol >= cols) return group;
        if (visited[startRow, startCol]) return group;
        if (grid[startRow, startCol] == null) return group;
        if (grid[startRow, startCol].flyweight != fw) return group;


        Queue<Vector2Int> queue = GetQueueFromPool(rows * cols);
        visited[startRow, startCol] = true;
        queue.Enqueue(new Vector2Int(startRow, startCol));
        group.Add(grid[startRow, startCol]);


        int[] rowOffsets = { -1, 1, 0, 0 };
        int[] colOffsets = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            int r = cell.x;
            int c = cell.y;

            for (int i = 0; i < 4; i++)
            {
                int newRow = r + rowOffsets[i];
                int newCol = c + colOffsets[i];

                if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols) continue;
                if (visited[newRow, newCol]) continue;
                if (grid[newRow, newCol] == null) continue;
                if (grid[newRow, newCol].flyweight != fw) continue;

                visited[newRow, newCol] = true;
                queue.Enqueue(new Vector2Int(newRow, newCol));
                group.Add(grid[newRow, newCol]);
            }

            if (group.Count >= 2) // Early exit if group is already big enough
            {
                ReturnQueueToPool(queue);
                return group; 
            }
        }

        ReturnQueueToPool(queue);
        ReturnListToPool(group);
        return group;
    }

    public void SmartShuffle(Block[,] grid, BoardManager bm) //Shuffles the board in a way that guarantees a possible move.
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        List<Block> allBlocks = new List<Block>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    allBlocks.Add(grid[r, c]);
                }
            }
        }

        Dictionary<BlockFlyweight, List<Block>> groups = new Dictionary<BlockFlyweight, List<Block>>();
        foreach (Block block in allBlocks)
        {
            if (!groups.ContainsKey(block.flyweight))
            {
                groups[block.flyweight] = new List<Block>();
            }
            groups[block.flyweight].Add(block);
        }

        BlockFlyweight chosenFlyweight = null;
        foreach (var kvp in groups)
        {
            if (kvp.Value.Count >= 2)
            {
                chosenFlyweight = kvp.Key;
                break;
            }
        }
        if (chosenFlyweight == null)
        {
            FisherYatesShuffle(allBlocks);
            FillGridWithBlocks(allBlocks, grid, bm);
            return;
        }
        List<Block> chosenBlocks = groups[chosenFlyweight];
        Block pairBlock1 = chosenBlocks[0];
        Block pairBlock2 = chosenBlocks[1];

        allBlocks.Remove(pairBlock1);
        allBlocks.Remove(pairBlock2);

        List<Vector2Int> positions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                positions.Add(new Vector2Int(r, c));
            }
        }

        List<(Vector2Int, Vector2Int)> adjacentPairs = new List<(Vector2Int, Vector2Int)>();
        foreach (Vector2Int pos in positions)
        {
            if (pos.y < cols - 1)
            {
                adjacentPairs.Add((pos, new Vector2Int(pos.x, pos.y + 1)));
            }
            if (pos.x < rows - 1)
            {
                adjacentPairs.Add((pos, new Vector2Int(pos.x + 1, pos.y)));
            }
        }

        var chosenPair = adjacentPairs[Random.Range(0, adjacentPairs.Count)];

        positions.Remove(chosenPair.Item1);
        positions.Remove(chosenPair.Item2);

        FisherYatesShuffle(positions);
        FisherYatesShuffle(allBlocks);

        Block[,] newGrid = new Block[rows, cols];

        pairBlock1.row = chosenPair.Item1.x;
        pairBlock1.col = chosenPair.Item1.y;
        pairBlock2.row = chosenPair.Item2.x;
        pairBlock2.col = chosenPair.Item2.y;
        newGrid[chosenPair.Item1.x, chosenPair.Item1.y] = pairBlock1;
        newGrid[chosenPair.Item2.x, chosenPair.Item2.y] = pairBlock2;

        int index = 0;
        foreach (Vector2Int pos in positions)
        {
            if (index < allBlocks.Count)
            {
                Block b = allBlocks[index];
                b.row = pos.x;
                b.col = pos.y;
                newGrid[pos.x, pos.y] = b;
                index++;
            }
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = newGrid[r, c];
                if (grid[r, c] != null)
                {
                    Vector3 targetPos = bm.gridManager.CalculateBlockPosition(r, c);
                    grid[r, c].transform.DOLocalMove(targetPos, shuffleTimer).SetEase(Ease.OutBounce);
                }
            }
        }
    }
    private void FillGridWithBlocks(List<Block> blocks, Block[,] grid, BoardManager bm) //Fills the grid with blocks in a random order.
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                positions.Add(new Vector2Int(r, c));
            }
        }
        FisherYatesShuffle(positions);
        Block[,] newGrid = new Block[rows, cols];
        int index = 0;
        foreach (Vector2Int pos in positions)
        {
            if (index < blocks.Count)
            {
                Block b = blocks[index];
                b.row = pos.x;
                b.col = pos.y;
                newGrid[pos.x, pos.y] = b;
                index++;
            }
        }
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = newGrid[r, c];
                if (grid[r, c] != null)
                {
                    Vector3 targetPos = bm.gridManager.CalculateBlockPosition(r, c);
                    grid[r, c].transform.DOLocalMove(targetPos, shuffleTimer).SetEase(Ease.OutBounce);
                }
            }
        }
    }
}
