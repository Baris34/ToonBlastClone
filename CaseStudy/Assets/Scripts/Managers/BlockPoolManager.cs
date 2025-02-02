using UnityEngine;
using System.Collections.Generic;

// Basit bir "Block" havuzu yöneticisi
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

    // Kullanýlabilir bir Block döndürür.
    // Eðer havuz boþ ise (optionel) yeni Instantiate yapar veya hata verebilir.
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
            // Havuz biterse yenisini yaratabilirsin veya limit koyabilirsin
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