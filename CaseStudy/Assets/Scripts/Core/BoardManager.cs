using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
public class BoardManager : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData currentLevelData;

    [Header("Flyweights")]
    public List<BlockFlyweight> blockFlyweights;

    [Header("Cell & Grid Size")]
    public Vector2 cellSize = new Vector2(1f, 1f);

    [Header("References")]
    public BlockPoolManager poolManager;
    public DeadlockSystem deadlockSystem;

    // Asýl tablo
    public Block[,] grid;

    private void Start()
    {
        CreateGrid();
        ScaleGridToFitScreen();
        UpdateBoardState(); // tabloyu BFS ile tarayýp ikon/deadlock bak
    }

    // ------------------------------------------------
    //  GRID OLUÞTURMA
    // ------------------------------------------------
    public void CreateGrid()
    {
        grid = new Block[currentLevelData.rows, currentLevelData.cols];

        for (int r = 0; r < currentLevelData.rows; r++)
        {
            for (int c = 0; c < currentLevelData.cols; c++)
            {
                int rand = Random.Range(0, blockFlyweights.Count);
                BlockFlyweight fw = blockFlyweights[rand];

                Block newBlock = poolManager.GetBlockFromPool();
                newBlock.transform.SetParent(transform, false);

                Vector2 pos = CalculateBlockPosition(r, c);
                newBlock.transform.localPosition = pos;

                newBlock.Initialize(fw, r, c);
                newBlock.boardManager = this;

                grid[r, c] = newBlock;
            }
        }
    }

    // ------------------------------------------------
    //  TIKLAMA
    // ------------------------------------------------
    public void OnBlockClicked(Block clickedBlock)
    {
        // BFS ile týklanan bloðun grubunu bul
        List<Block> group = FindSingleGroup(clickedBlock.row, clickedBlock.col, clickedBlock.flyweight);
        if (group.Count < 2) return;

        // Patlamadan önce istersen icon güncelle
        foreach (var b in group)
        {
            b.UpdateIconByGroupSize(group.Count, currentLevelData);
        }

        // 0.2 sn bekleyip patlat
        StartCoroutine(RemoveBlocksAfterDelay(group, 0.2f));
    }

    private IEnumerator RemoveBlocksAfterDelay(List<Block> group, float delay)
    {
        yield return new WaitForSeconds(delay);

        float animDuration = 0.3f;
        foreach (Block b in group)
        {
            // Örneðin scale’i 1’den 0’a animasyon
            b.transform.DOScale(Vector3.zero, animDuration).SetEase(Ease.InBack);
        }



        yield return new WaitForSeconds(animDuration);

        // Grubu kaldýr
        foreach (var b in group)
        {
            grid[b.row, b.col] = null;
            b.transform.localScale = new Vector2(.32f, .32f);
            poolManager.ReturnBlockToPool(b);
        }

        ApplyGravity();
        RefillGrid();

        // Tabloyu yeniden tara => ikonlarý ve deadlock'u kontrol et
        UpdateBoardState();
    }

    // Tek seferde tek bir grubu bulmak
    private List<Block> FindSingleGroup(int row, int col, BlockFlyweight fw)
    {
        List<Block> result = new List<Block>();
        bool[,] visited = new bool[currentLevelData.rows, currentLevelData.cols];
        FloodFill(row, col, fw, visited, result);
        return result;
    }

    private void FloodFill(int r, int c, BlockFlyweight fw, bool[,] visited, List<Block> group)
    {
        if (r < 0 || r >= currentLevelData.rows ||
            c < 0 || c >= currentLevelData.cols ||
            visited[r, c] || grid[r, c] == null ||
            grid[r, c].flyweight != fw)
        {
            return;
        }
        visited[r, c] = true;
        group.Add(grid[r, c]);

        FloodFill(r + 1, c, fw, visited, group);
        FloodFill(r - 1, c, fw, visited, group);
        FloodFill(r, c + 1, fw, visited, group);
        FloodFill(r, c - 1, fw, visited, group);
    }

    // ------------------------------------------------
    //  GRAVITY & REFILL
    // ------------------------------------------------
    public void ApplyGravity()
    {
        for (int c = 0; c < currentLevelData.cols; c++)
        {
            int writeRow = 0;
            for (int r = 0; r < currentLevelData.rows; r++)
            {
                if (grid[r, c] != null)
                {
                    if (r != writeRow)
                    {
                        grid[writeRow, c] = grid[r, c];
                        grid[r, c] = null;

                        grid[writeRow, c].row = writeRow;
                        Vector2 newPos = CalculateBlockPosition(writeRow, c);

                        grid[writeRow, c].transform.DOLocalMove(newPos, 0.3f).SetEase(Ease.OutBounce);
                    }
                    writeRow++;
                }
            }
        }
    }

    public void RefillGrid()
    {
        for (int r = 0; r < currentLevelData.rows; r++)
        {
            for (int c = 0; c < currentLevelData.cols; c++)
            {
                if (grid[r, c] == null)
                {
                    int rand = Random.Range(0, blockFlyweights.Count);
                    BlockFlyweight fw = blockFlyweights[rand];

                    Block newBlock = poolManager.GetBlockFromPool();
                    newBlock.transform.SetParent(transform, false);

                    Vector2 targetPos = CalculateBlockPosition(r, c);

                    // Blok baþlangýç pozisyonunu hedef pozisyonun yukarýsýna ayarla
                    Vector2 startPos = new Vector2(targetPos.x, targetPos.y + 2f); // Daha yukarýdan baþlat

                    // Baþlangýç pozisyonunu ayarla
                    newBlock.transform.localPosition = startPos;

                    // Animasyonla bloklarý hedef pozisyona düþür
                    newBlock.transform.DOLocalMove(targetPos, 0.5f).SetEase(Ease.OutBounce);     // Yumuþak iniþ efekti

                    newBlock.Initialize(fw, r, c);
                    newBlock.boardManager = this;

                    grid[r, c] = newBlock;
                }
            }
        }
    }

    // ------------------------------------------------
    //  UPDATE BOARD STATE => BFS (tekli dahil)
    // ------------------------------------------------
    // Tüm tabloyu tarar, 1’li dahil her grubu bulur,
    // hangi grupta kaç blok var => icon güncelle
    // eðer >=2 grup yoksa => deadlock => shuffle
    public void UpdateBoardState()
    {
        List<List<Block>> allGroups = FindAllGroupsIncludeSingles();
        bool hasBlastableGroup = false;
        // Gruplarý tek tek tara
        foreach (var group in allGroups)
        {
            int groupSize = group.Count;
            // 2’den büyükse patlatýlabilir
            if (groupSize >= 2)
            {
                hasBlastableGroup = true;
            }

            // Icon güncelle
            foreach (Block b in group)
            {
                b.UpdateIconByGroupSize(groupSize, currentLevelData);
            }
        }

        // Deadlock?
        if (!hasBlastableGroup)
        {
            Debug.Log("No moves => Deadlock => Shuffle");
            if (deadlockSystem != null)
            {
                deadlockSystem.ShuffleToGuaranteeOneGroup(grid, this);
            }
        }
    }
    
    private List<List<Block>> FindAllGroupsIncludeSingles()
    {
        List<List<Block>> groups = new List<List<Block>>();
        bool[,] visited = new bool[currentLevelData.rows, currentLevelData.cols];

        for (int r = 0; r < currentLevelData.rows; r++)
        {
            for (int c = 0; c < currentLevelData.cols; c++)
            {
                if (!visited[r, c] && grid[r, c] != null)
                {
                    List<Block> currentGroup = new List<Block>();
                    FloodFill(r, c, grid[r, c].flyweight, visited, currentGroup);
                    groups.Add(currentGroup);
                }
            }
        }
        return groups;
    }

    // ------------------------------------------------
    //  UTILITY: HESAPLAMALAR
    // ------------------------------------------------
    public Vector2 CalculateBlockPosition(int row, int col)
    {
        float xPos = col * cellSize.x;
        float yPos = row * cellSize.y;
        return new Vector2(xPos, yPos);
    }

    public void ScaleGridToFitScreen()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float gridW = currentLevelData.cols * cellSize.x;
        float gridH = currentLevelData.rows * cellSize.y;

        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.aspect * halfHeight;

        float scaleX = (halfWidth * 2f) / gridW;
        float scaleY = (halfHeight * 2f) / gridH;
        float finalScale = Mathf.Min(scaleX, scaleY);
        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    // ------------------------------------------------
    //  YENÝ LEVEL YÜKLEME
    // ------------------------------------------------
    public void LoadLevel(LevelData data)
    {
        currentLevelData = data;

        // Mevcut bloklarý pool’a iade
        foreach (Transform child in transform)
        {
            Block b = child.GetComponent<Block>();
            if (b != null)
            {
                poolManager.ReturnBlockToPool(b);
            }
        }

        // tabloyu baþtan kur
        CreateGrid();
        ScaleGridToFitScreen();
        UpdateBoardState();
    }
}
