using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI.Table;

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

    public Block[,] grid;

    // State Pattern
    public BoardState currentState = BoardState.Idle;

    private float stateTimer;
    private List<Block> currentProcessingBlocks = new List<Block>();

    private HashSet<Block> blocksToCheck = new HashSet<Block>();

    private bool[,] visited;

    private void Start()
    {
        CreateGrid();
        UpdateAllCombosAndSprites();
        ScaleGridToFitScreen();
    }

    private void Update()
    {
        switch (currentState)
        {
            case BoardState.Removing:
                UpdateRemovingState();
                break;
            case BoardState.Gravity:
                UpdateGravityState();
                break;
            case BoardState.Refill:
                UpdateRefillState();
                break;
            case BoardState.Checking:
                UpdateCheckingState();
                break;
            case BoardState.Shuffling:
                UpdateShufflingState();
                break;
        }
    }

    // -----------------------------
    //  STATE MACHINE FUNCTIONS
    // -----------------------------
    private void UpdateRemovingState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            // Remove
            foreach (var b in currentProcessingBlocks)
            {
                grid[b.row, b.col] = null;
                b.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
                poolManager.ReturnBlockToPool(b);

                // Patlayan bloklarýn etrafýndaki bloklarý kontrol edilecekler listesine ekle
                AddNearbyBlocksToCheck(b.row, b.col, 1); // 2 hücre yarýçapýnda kontrol edilecek.
            }

            currentProcessingBlocks.Clear();
            TransitionToState(BoardState.Gravity);
        }
    }

    private void UpdateGravityState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            // Tüm animasyonlar tamamlandý
            TransitionToState(BoardState.Refill);
        }
    }

    private void UpdateRefillState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            // Tüm refill animasyonlarý tamam
            TransitionToState(BoardState.Checking);
            UpdateCombosAndSpritesForAffectedBlocks();
        }
    }

    private void UpdateCheckingState()
    {
        bool noMoves = deadlockSystem.CheckNoMoves(grid, this);
        if (noMoves)
        {
            TransitionToState(BoardState.Shuffling);
            deadlockSystem.StartShuffle(grid, this);
        }
        else
        {
            TransitionToState(BoardState.Idle);
        }
    }

    private void UpdateShufflingState()
    {
        if (!deadlockSystem.IsShuffling)
        {
            bool stillNoMoves = deadlockSystem.CheckNoMoves(grid, this);
            TransitionToState(stillNoMoves ? BoardState.Shuffling : BoardState.Idle);
        }
    }

    private void TransitionToState(BoardState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case BoardState.Removing:
                stateTimer = 0.3f; // Remove animasyon süresi

                break;

            case BoardState.Gravity:
                ApplyGravity();
                stateTimer = 0.3f; // Gravity animasyon süresi
                break;

            case BoardState.Refill:
                RefillGrid();
                stateTimer = 0.3f; // Refill animasyon süresi
                break;

            case BoardState.Shuffling:
                stateTimer = 0.3f;
                break;
        }
    }
    // -----------------------------
    //  CREATE GRID
    // -----------------------------
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

                Vector3 pos = CalculateBlockPosition(r, c);
                newBlock.transform.localPosition = pos;

                newBlock.Initialize(fw, r, c);
                newBlock.boardManager = this;

                grid[r, c] = newBlock;
            }
        }
        bool noMoves = deadlockSystem.CheckNoMoves(grid, this);
        if (noMoves)
        {
            deadlockSystem.StartShuffle(grid, this);
        }
    }
    private void InitializeVisitedArray()
    {
        visited = new bool[currentLevelData.rows, currentLevelData.cols];
    }
    // -----------------------------
    //  ON BLOCK CLICKED
    // -----------------------------
    public void OnBlockClicked(Block clickedBlock)
    {
        if (currentState != BoardState.Idle) return;
        bool[,] visited = new bool[currentLevelData.rows, currentLevelData.cols];
        List<Block> group = FindGroup(clickedBlock.row, clickedBlock.col, clickedBlock.flyweight,visited);
        if (group.Count < 2) return;


        currentProcessingBlocks = group;
        TransitionToState(BoardState.Removing);
    }

    private void ApplyGravity()
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
                        Vector3 newPos = CalculateBlockPosition(writeRow, c);
                        grid[writeRow, c].transform.DOLocalMove(newPos, 0.3f).SetEase(Ease.OutBounce);
                    }
                    writeRow++;
                }
            }
        }
    }

    private void RefillGrid()
    {
        float animTime = 0.3f;

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

                    Vector3 targetPos = CalculateBlockPosition(r, c);
                    Vector3 startPos = targetPos + new Vector3(0, 2f, 0);

                    newBlock.transform.localPosition = startPos;
                    newBlock.transform.DOLocalMove(targetPos, animTime).SetEase(Ease.OutBounce);

                    newBlock.Initialize(fw, r, c);
                    newBlock.boardManager = this;

                    grid[r, c] = newBlock;

                    // Yeni eklenen bloklarý kontrol edilecekler listesine ekle
                    blocksToCheck.Add(newBlock);
                    // Yeni eklenen bloklarýn ETRAFINDAKÝ bloklarý da kontrol edilecekler listesine ekle
                    AddNearbyBlocksToCheck(r, c, 0); // Yarýçapý 1 olarak ayarladým, ihtiyaca göre deðiþtirebilirsin
                }
            }
        }
    }
    public void UpdateCombosAndSpritesForAffectedBlocks()
    {
        // Kontrol edilecek bloklar listesini kopyala ve orijinal listeyi temizle
        HashSet<Block> blocksToProcess = new HashSet<Block>(blocksToCheck);
        blocksToCheck.Clear();

        InitializeVisitedArray();

        HashSet<Block> blocksAndTheirNeighbors = new HashSet<Block>();
        foreach (Block b in blocksToProcess)
        {
            if (b != null)
            {
                blocksAndTheirNeighbors.Add(b);
                AddNearbyBlocksToList(b.row, b.col, 1, blocksAndTheirNeighbors); // 1 yarýçapýndaki komþularý ekle
            }
        }
        foreach (Block block in blocksAndTheirNeighbors)
        {
            if (block == null || visited[block.row, block.col]) continue;

            List<Block> group = FindGroup(block.row, block.col, block.flyweight, visited);

            int groupSize = group.Count;
            foreach (Block b in group)
            {
                b.UpdateIconByGroupSize(groupSize, currentLevelData);
            }
        }
    }
    private void AddNearbyBlocksToCheck(int row, int col, int radius)
    {
        for (int r = Mathf.Max(0, row - radius); r <= Mathf.Min(currentLevelData.rows - 1, row + radius); r++)
        {
            for (int c = Mathf.Max(0, col - radius); c <= Mathf.Min(currentLevelData.cols - 1, col + radius); c++)
            {
                if (grid[r, c] != null)
                {
                    blocksToCheck.Add(grid[r, c]);
                }
            }
        }
    }
    private void AddNearbyBlocksToList(int row, int col, int radius, HashSet<Block> list)
    {
        for (int r = Mathf.Max(0, row - radius); r <= Mathf.Min(currentLevelData.rows - 1, row + radius); r++)
        {
            for (int c = Mathf.Max(0, col - radius); c <= Mathf.Min(currentLevelData.cols - 1, col + radius); c++)
            {
                if (grid[r, c] != null)
                {
                    list.Add(grid[r, c]);
                }
            }
        }
    }
    public void RemoveBlocks(List<Block> group)
    {
        float animTime = 0.3f;
        foreach (var b in group)
        {
            b.transform.DOScale(Vector3.zero, animTime).SetEase(Ease.InBack);
        }

        // Remove
        foreach (var b in group)
        {
            grid[b.row, b.col] = null;
            b.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            poolManager.ReturnBlockToPool(b);
        }
        UpdateAllCombosAndSprites();
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
                    List<Block> group = new List<Block>();
                    FloodFill(r, c, grid[r, c].flyweight, visited, group);
                    groups.Add(group);
                }
            }
        }
        return groups;
    }

    public void UpdateAllCombosAndSprites()
    {
        List<List<Block>> groups = FindAllGroupsIncludeSingles();
        foreach (List<Block> group in groups)
        {
            int groupSize = group.Count;
            foreach (Block b in group)
            {
                b.UpdateIconByGroupSize(groupSize, currentLevelData);
            }
        }
    }

    // -----------------------------
    //  BFS Tek grup
    // -----------------------------
    private List<Block> FindGroup(int row, int col, BlockFlyweight fw, bool[,] visited)
    {
        List<Block> group = new List<Block>();
        FloodFill(row, col, fw, visited, group);
        return group;
    }

    private void FloodFill(int r, int c, BlockFlyweight fw, bool[,] visited, List<Block> group)
    {
        if (r < 0 || r >= currentLevelData.rows || c < 0 || c >= currentLevelData.cols) return;
        if (visited[r, c] || grid[r, c] == null) return;
        if (grid[r, c].flyweight != fw) return;

        visited[r, c] = true;
        group.Add(grid[r, c]);

        FloodFill(r + 1, c, fw, visited, group);
        FloodFill(r - 1, c, fw, visited, group);
        FloodFill(r, c + 1, fw, visited, group);
        FloodFill(r, c - 1, fw, visited, group);
    }

    // -----------------------------
    //  UTILS
    // -----------------------------
    public Vector3 CalculateBlockPosition(int row, int col)
    {
        float xPos = col * cellSize.x;
        float yPos = row * cellSize.y;
        return new Vector3(xPos, yPos, 0f);
    }

    public void ScaleGridToFitScreen()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        // 1) Board’un ham geniþlik / yükseklik (col*cellSize, row*cellSize)
        float gridW = currentLevelData.cols * cellSize.x;
        float gridH = currentLevelData.rows * cellSize.y;

        // 2) Kameranýn yarý yüksekliði
        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.aspect * halfHeight;

        // 3) Ölçek
        float scaleX = (halfWidth * 2f) / gridW;
        float scaleY = (halfHeight * 2f) / gridH;
        float finalScale = Mathf.Min(scaleX, scaleY);

        // (isteðe baðlý) tabloda biraz kenar boþluðu býrakmak istiyorsanýz:
        // float safeMargin = 0.95f; // %95
        // finalScale *= safeMargin;

        // 4) Board’u ölçekle
        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // 5) Board’un “dünya” boyutu
        float boardWorldW = gridW * finalScale;
        float boardWorldH = gridH * finalScale;

        // 6) Kameranýn orta noktasý
        Vector3 camPos = cam.transform.position;
        // Genelde camPos = (0,0,-10) gibi

        // 7) Tabloyu ortalamak için: 
        // Board’un sol-alt köþesi = (cameraCenter.x - boardWorldW/2, cameraCenter.y - boardWorldH/2)
        float offsetX = -boardWorldW / 2f;
        float offsetY = -boardWorldH / 2f;

        // 8) Son position
        transform.position = new Vector3(
            camPos.x + offsetX+0.3f,
            camPos.y + offsetY,
            0f
        );
    }

    public void LoadLevel(LevelData data)
    {
        currentLevelData = data;
        // eski bloklarý havuza iade
        foreach (Transform child in transform)
        {
            Block b = child.GetComponent<Block>();
            if (b != null) poolManager.ReturnBlockToPool(b);
        }
        CreateGrid();
        ScaleGridToFitScreen();
    }
}
