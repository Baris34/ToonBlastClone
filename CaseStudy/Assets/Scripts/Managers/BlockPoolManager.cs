using UnityEngine;
using System.Collections.Generic;

public class BlockPoolManager : MonoBehaviour
{
    [Header("Block Pool Settings")]
    public GameObject blockPrefab;
    public int initialPoolSize = 100;

    private Queue<Block> blockPool = new Queue<Block>();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(blockPrefab, transform);
            obj.SetActive(false);
            blockPool.Enqueue(obj.GetComponent<Block>());
        }
    }
    public Block GetBlockFromPool()
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

    public void ReturnBlockToPool(Block block)
    {
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }
}