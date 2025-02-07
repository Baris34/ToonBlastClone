using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

//Manages block groups, updates combo icons, and performs group-related operations.
public class GroupManager 
{
    #region Variables
    private BoardManager boardManager;
    private Block[,] grid;
    private int rows;
    private int cols;
    public bool[,] visited { get; private set; }
    #endregion
    //Collection Pools: pool methods for HashSet, List, Queue
    #region CollectionPooling
    private Stack<HashSet<Block>> blockHashSetPool = new Stack<HashSet<Block>>();
    private Stack<List<Block>> blockListPool = new Stack<List<Block>>();
    private Stack<Queue<Block>> blockQueuePool = new Stack<Queue<Block>>();

    private HashSet<Block> GetHashSetFromPool()
    {
        if (blockHashSetPool.Count > 0)
        {
            return blockHashSetPool.Pop();
        }
        else
        {
            return new HashSet<Block>();
        }
    }
    private void ReturnHashSetToPool(HashSet<Block> hashSet) 
    {
        hashSet.Clear();
        blockHashSetPool.Push(hashSet);
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
    private Queue<Block> GetQueueFromPool()
    {
        if (blockQueuePool.Count > 0)
        {
            return blockQueuePool.Pop();
        }
        else
        {
            return new Queue<Block>();
        }
    }

    private void ReturnQueueToPool(Queue<Block> queue)
    {
        queue.Clear();
        blockQueuePool.Push(queue);
    }
    #endregion 
    public GroupManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
        this.grid = boardManager.grid;
        this.rows = boardManager.currentLevelData.rows;
        this.cols = boardManager.currentLevelData.cols;
        InitializeVisitedArray();
    }

    
    public void InitializeVisitedArray() //Initializes the 'visited' array for group finding.
    {
        visited = new bool[rows, cols];
    }

    public void AddNearbyBlocksToCheck(int row, int col, int radius) //Adds nearby blocks to the 'blocksToCheck' list.
    {
        for (int r = Mathf.Max(0, row - radius); r <= Mathf.Min(rows - 1, row + radius); r++)
        {
            for (int c = Mathf.Max(0, col - radius); c <= Mathf.Min(cols - 1, col + radius); c++)
            {
                if (grid[r, c] != null)
                {
                    boardManager.blocksToCheck.Add(grid[r, c]);
                }
            }
        }
    }

    public void AddNearbyBlocksToList(int row, int col, int radius, HashSet<Block> list) //Adds nearby blocks to a provided list.
    {
        for (int r = Mathf.Max(0, row - radius); r <= Mathf.Min(rows - 1, row + radius); r++)
        {
            for (int c = Mathf.Max(0, col - radius); c <= Mathf.Min(cols - 1, col + radius); c++)
            {
                if (grid[r, c] != null)
                {
                    list.Add(grid[r, c]);
                }
            }
        }
    }
    public void UpdateCombosAndSpritesForAffectedBlocks() //Updates combo icons and sprites for blocks affected by a move.
    {
        HashSet<Block> blocksToProcess = GetHashSetFromPool();
        blocksToProcess.UnionWith(boardManager.blocksToCheck);
        boardManager.blocksToCheck.Clear();

        InitializeVisitedArray();

        HashSet<Block> blocksAndTheirNeighbors = GetHashSetFromPool();
        foreach (Block b in blocksToProcess)
        {
            if (b != null)
            {
                blocksAndTheirNeighbors.Add(b);
                AddNearbyBlocksToList(b.row, b.col, 1, blocksAndTheirNeighbors);
            }
        }
        ReturnHashSetToPool(blocksToProcess);
        foreach (Block block in blocksAndTheirNeighbors)
        {
            if (block == null || visited[block.row, block.col]) continue;

            List<Block> group = FindGroup(block.row, block.col, block.flyweight, visited);

            int groupSize = group.Count;

            if (groupSize == 1)
            {
                group[0].UpdateIconByGroupSize(1, boardManager.currentLevelData);
            }
            else
            {
                foreach (Block b in group)
                {
                    b.UpdateIconByGroupSize(groupSize, boardManager.currentLevelData);
                }
            }

            ReturnListToPool(group);
        }
        ReturnHashSetToPool(blocksAndTheirNeighbors);
    }

    private List<List<Block>> FindAllGroupsIncludeSingles() //Finds all block groups, including singles, on the board.
    {
        List<List<Block>> groups = new List<List<Block>>();
        InitializeVisitedArray();
        HashSet<Block> blocksAndTheirNeighbors = new HashSet<Block>();
        foreach (Block b in boardManager.blocksToCheck)
        {
            if (b != null)
            {
                blocksAndTheirNeighbors.Add(b);
                AddNearbyBlocksToList(b.row, b.col, 1, blocksAndTheirNeighbors);
            }
        }
        if (blocksAndTheirNeighbors.Count == 0)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    blocksAndTheirNeighbors.Add(grid[r, c]);
                }
            }
        }

        foreach (Block block in blocksAndTheirNeighbors)
        {
            if (block == null || visited[block.row, block.col]) continue;

            List<Block> group = FindGroup(block.row, block.col, block.flyweight, visited);
            groups.Add(group);
        }

        return groups;
    }

    public void UpdateAllCombosAndSprites() //Updates combo icons and sprites for all blocks on the board.
    {
        List<List<Block>> groups = FindAllGroupsIncludeSingles();
        foreach (List<Block> group in groups)
        {
            int groupSize = group.Count;
            foreach (Block b in group)
            {
                b.UpdateIconByGroupSize(groupSize, boardManager.currentLevelData);
            }
        }
    }
    public List<Block> FindGroup(int row, int col, BlockFlyweight fw, bool[,] visited) //Finds a group of blocks of the same color using flood fill algorithm.
    {
        List<Block> group = new List<Block>();
        Queue<Block> queue = GetQueueFromPool();

        if (grid == null || fw == null || visited == null)
        {
            Debug.LogError("FindGroup: null parameter!");
            return group;
        }
        if (row < 0 || row >= rows || col < 0 || col >= cols || visited.GetLength(0) != rows || visited.GetLength(1) != cols)
        {
            Debug.LogError("FindGroup: Invalid row/col or visited dimensions!");
            return group;
        }

        if (visited[row, col] || grid[row, col] == null || grid[row, col].flyweight != fw)
        {
            return group;
        }

        queue.Enqueue(grid[row, col]);
        visited[row, col] = true;

        int[] rowOffsets = { -1, 1, 0, 0 };
        int[] colOffsets = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            Block currentBlock = queue.Dequeue();
            group.Add(currentBlock);

            for (int i = 0; i < 4; i++)
            {
                int newRow = currentBlock.row + rowOffsets[i];
                int newCol = currentBlock.col + colOffsets[i];

                if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols) continue;
                if (visited[newRow, newCol] || grid[newRow, newCol] == null || grid[newRow, newCol].flyweight != fw) continue;

                queue.Enqueue(grid[newRow, newCol]);
                visited[newRow, newCol] = true;
            }
        }
        ReturnQueueToPool(queue);
        return group;
    }

    private bool IsValidCell(int row, int col) //Checks if a given cell is valid within the grid boundaries.
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }
}