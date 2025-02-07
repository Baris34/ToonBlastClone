using UnityEngine;
using System.Collections.Generic;

//Manages the block pool, instantiates and recycles block objects.
public class BlockPoolManager : MonoBehaviour
{
    #region Variables
    [Header("Block Pool Settings")]
    public GameObject blockPrefab;
    public int initialPoolSize;
    public BoardManager boardManager;
    private Queue<Block> blockPool = new Queue<Block>();

    #endregion

    private void Awake()
    {
        initialPoolSize = boardManager.currentLevelData.rows * boardManager.currentLevelData.cols;
        InitializePool();
    }

    private void InitializePool() //Initializes the block pool with a set number of blocks
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(blockPrefab, transform);
            obj.SetActive(false);
            blockPool.Enqueue(obj.GetComponent<Block>());
        }
    }
    public Block GetBlockFromPool() //Returns a block from the pool
    {
        if (blockPool.Count > 0)
        {
            Block block = blockPool.Dequeue();
            block.gameObject.SetActive(true);
            return block;
        }
        else
        {
            GameObject obj = Instantiate(blockPrefab, transform);
            return obj.GetComponent<Block>();
        }
    }

    public void ReturnBlockToPool(Block block) //Returns a block to the pool
    {
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }
}