using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class GridManager : MonoBehaviour
{
    [Header("Object Pool")]
    public int initialPoolSize = 100;
    private Queue<Block> blockPool = new Queue<Block>();


    [Header("Grid Settings")]
    
    public GameObject blockPrefab;
    public GridLayoutGroup gridLayoutGroup; // GridArea objesinin GridLayoutGroup bile�enine referans

    [Header("Flyweight")]
    public List<BlockFlyweight> blockFlyweights; // BlockFlyweight objelerinin listesi
    public LevelData currentLevelData;

    private Block[,] grid; // Bloklar� tutacak 2D dizi

    private void Awake()
    {
        // Pool�u burada ba�lat�yoruz
        InitializePool();
    }
    private void Start()
    {
        grid = new Block[currentLevelData.rows, currentLevelData.cols];
        CreateGrid();
    }

    private void CreateGrid()
    {
        gridLayoutGroup.constraintCount = currentLevelData.cols; // S�tun say�s�n� ayarla
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        


        for (int r = 0; r < currentLevelData.rows; r++)
        {
            for (int c = 0; c < currentLevelData.cols; c++)
            {
                // Rastgele bir BlockFlyweight se�
                int randomIndex = Random.Range(0, blockFlyweights.Count);
                BlockFlyweight selectedFlyweight = blockFlyweights[randomIndex];

                // Blok objesini olu�tur ve ba�lat
                Block block = GetBlockFromPool();
                block.transform.SetParent(gridLayoutGroup.transform, false);
                grid[r, c] = block;
                grid[r, c].Initialize(selectedFlyweight, r, c);
            }
        }
        // Grid'i ekranda ortala
        float gridWidth = currentLevelData.cols * gridLayoutGroup.cellSize.x;
        float gridHeight = currentLevelData.rows * gridLayoutGroup.cellSize.y;
        gridLayoutGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(gridWidth, gridHeight);

        // GridArea'y� ortala
        RectTransform gridAreaRectTransform = gridLayoutGroup.GetComponent<RectTransform>();
        gridAreaRectTransform.anchoredPosition = Vector2.zero; // GridArea'y� Canvas'�n ortas�na yerle�tir
        MatchBlocks();
    }
    public void MatchBlocks()
    {
        for (int r = 0; r < currentLevelData.rows; r++)
    {
        for (int c = 0; c < currentLevelData.cols; c++)
        {
            if (grid[r, c] != null)
            {
                grid[r, c].isMatched = false;
            }
        }
    }

    // E�le�en bloklar�n ba�l� oldu�u gruplar�n b�y�kl���n� hesapla ve isMatched de�erlerini ayarla
    List<List<Block>> matchedGroups = FindMatchedGroups();

    foreach (List<Block> group in matchedGroups)
    {
        foreach (Block block in group)
        {
            block.isMatched = true; // isMatched de�erini true olarak ayarla
        }
    }

    // Her blo�un ikonunu g�ncelle
    foreach (List<Block> group in matchedGroups)
    {
        foreach (Block block in group)
        {
            block.UpdateIconByGroupSize(group.Count, currentLevelData);
        }
    }
    }

    private List<List<Block>> FindMatchedGroups()
    {
        List<List<Block>> matchedGroups = new List<List<Block>>();
        bool[,] visited = new bool[currentLevelData.rows, currentLevelData.cols];

        for (int r = 0; r < currentLevelData.rows; r++)
        {
            for (int c = 0; c < currentLevelData.cols; c++)
            {
                // Yaln�zca ziyaret edilmemi� ve null olmayan bloklar i�in FindGroupDFS'yi �a��r
                if (grid[r, c] != null && !visited[r, c])
                {
                    List<Block> currentGroup = new List<Block>();
                    FindGroupDFS(r, c, grid[r, c].flyweight, currentGroup, visited);

                    if (currentGroup.Count > 1)
                    {
                        matchedGroups.Add(currentGroup);
                    }
                }
            }
        }

        return matchedGroups;
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
    private Block GetBlockFromPool()
    {
        Block block;
        if (blockPool.Count > 0)
        {
            block = blockPool.Dequeue();
            block.gameObject.SetActive(true);
            return block;
        }
        return null;
    }
    private void ReturnBlockToPool(Block block)
    {
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }
    private void FindGroupDFS(int row, int col, BlockFlyweight flyweight, List<Block> currentGroup, bool[,] visited)
    {
        if (row < 0 || row >= currentLevelData.rows || col < 0 || col >= currentLevelData.cols || visited[row, col] || grid[row, col] == null || grid[row, col].flyweight != flyweight)
        {
            return;
        }

        // Blo�u ziyaret edildi olarak i�aretle
        visited[row, col] = true;

        // Ayn� renkte olmayan bloklar� gruba ekleme
        if (grid[row, col].flyweight != flyweight)
        {
            return;
        }

        // Blo�u gruba ekle
        currentGroup.Add(grid[row, col]);

        // Kom�u bloklar� kontrol et
        FindGroupDFS(row + 1, col, flyweight, currentGroup, visited);
        FindGroupDFS(row - 1, col, flyweight, currentGroup, visited);
        FindGroupDFS(row, col + 1, flyweight, currentGroup, visited);
        FindGroupDFS(row, col - 1, flyweight, currentGroup, visited);
    }

    // Yeni seviye y�klerken �a�r�lacak metod
    public void LoadLevel(LevelData levelData)
    {
        currentLevelData = levelData;

        foreach (Transform child in gridLayoutGroup.transform)
        {
            Block block = child.GetComponent<Block>();
            if (block != null)
            {
                ReturnBlockToPool(block);
            }
        }

        // Blok dizisini temizle
        for (int i = 0; i < grid.GetLength(currentLevelData.rows); i++)
        {
            for (int j = 0; j < grid.GetLength(currentLevelData.cols); j++)
            {
                grid[i, j] = null;
            }
        }
        grid = new Block[currentLevelData.rows, currentLevelData.cols];
        // Grid'i tekrar olu�tur
        CreateGrid();
    }
}