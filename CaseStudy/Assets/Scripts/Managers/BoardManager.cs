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

    private void Start()
    {
        CreateGrid();
        UpdateAllCombosAndSprites();
        ScaleGridToFitScreen();
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
            StartCoroutine(deadlockSystem.ShuffleOneGroupAnimation(grid, this));
        }
    }

    // -----------------------------
    //  ON BLOCK CLICKED
    // -----------------------------
    public void OnBlockClicked(Block clickedBlock)
    {
        if (currentState != BoardState.Idle) return; // sadece Idle iken týklama

        // BFS bul
        List<Block> group = FindGroup(clickedBlock.row, clickedBlock.col, clickedBlock.flyweight);
        if (group.Count < 2) return; // patlatma yok

        // Durumu Removing yap
        currentState = BoardState.Removing;
        StartCoroutine(RemoveBlocksAnimation(group));
        
    }

    private IEnumerator RemoveBlocksAnimation(List<Block> group)
    {
        // 0.3s scale-down
        float animTime = 0.3f;
        foreach (var b in group)
        {
            b.transform.DOScale(Vector3.zero, animTime).SetEase(Ease.InBack);
        }
        yield return new WaitForSeconds(animTime);

        // Remove
        foreach (var b in group)
        {
            grid[b.row, b.col] = null;
            b.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            poolManager.ReturnBlockToPool(b);
        }

        // Patlatma bitti => devam puzzle flow
        StartCoroutine(FlowPuzzleSteps());
        UpdateAllCombosAndSprites();
    }

    // -----------------------------
    //  FLOW PUZZLE STEPS
    //  (Gravity -> Refill -> CheckDeadlock)
    // -----------------------------
    private IEnumerator FlowPuzzleSteps()
    {
        // 1) Gravity
        currentState = BoardState.Gravity;
        yield return StartCoroutine(ApplyGravityAnimation());

        // 2) Refill
        currentState = BoardState.Refill;
        yield return StartCoroutine(RefillAnimation());

        // 3) Deadlock Check
        currentState = BoardState.Checking;
        yield return StartCoroutine(CheckDeadlockRoutine());


        if (currentState == BoardState.Checking)
        {
            // means no shuffle triggered => moves var
            currentState = BoardState.Idle;
        }
    }

    private IEnumerator ApplyGravityAnimation()
    {
        float animTime = 0.3f;

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
                        grid[writeRow, c].transform.DOLocalMove(newPos, animTime).SetEase(Ease.OutBounce);
                    }
                    writeRow++;
                }
            }
        }

        yield return new WaitForSeconds(animTime);
    }

    private IEnumerator RefillAnimation()
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
                }
            }
        }
        UpdateAllCombosAndSprites();
        yield return new WaitForSeconds(animTime);
    }

    private IEnumerator CheckDeadlockRoutine()
    {
        bool noMoves = deadlockSystem.CheckNoMoves(grid, this);
        if (noMoves)
        {
            currentState = BoardState.Shuffling;
            yield return StartCoroutine(deadlockSystem.ShuffleOneGroupAnimation(grid, this));

            // Shuffle bitti => tekrar no moves check edelim
            bool stillNoMoves = deadlockSystem.CheckNoMoves(grid, this);
            if (stillNoMoves)
            {
                // Oyun sonlandýr veya sonsuz loop
            }
            // Shuffle ile moves varsa => tekrar Idle'a
            currentState = BoardState.Idle;
        }
        yield return null;
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
    private List<Block> FindGroup(int row, int col, BlockFlyweight fw)
    {
        List<Block> group = new List<Block>();
        bool[,] visited = new bool[currentLevelData.rows, currentLevelData.cols];
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
