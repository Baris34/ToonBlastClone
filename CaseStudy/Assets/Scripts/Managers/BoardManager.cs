using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI.Table;

public class BoardManager : MonoBehaviour
{
    #region Variables
    [Header("Level Data")]
    public LevelData currentLevelData;

    [Header("Flyweights")]
    public List<BlockFlyweight> blockFlyweights;

    [Header("Cell & Grid Size")]
    public Vector2 cellSize = new Vector2(0.32f, 0.32f);

    [Header("References")]
    public BlockPoolManager poolManager;
    public DeadlockSystem deadlockSystem;
    [Header("State Management")]
    public GameStateManager gameStateManager;

    public Block[,] grid;

    public BoardState currentState = BoardState.Idle;
    public HashSet<Block> blocksToCheck { get; set; }
    public RefillManager refillManager;
    public GridManager gridManager;
    public GameObject BlockContainer;
    public GroupManager groupManager;
    Queue<Block> queue = new Queue<Block>();
    #endregion

    #region Unity Methods
    private void Start()
    {
        gridManager = new GridManager(this);
        gameStateManager = new GameStateManager(this);
        CreateGrid();
        refillManager = new RefillManager(this);
        groupManager = new GroupManager(this);
        blocksToCheck = new HashSet<Block>();


        groupManager.UpdateAllCombosAndSprites();
        gridManager.ScaleGridToFitScreen();
    }

    private void Update()
    {
        gameStateManager.Update();
    }
    #endregion

    #region Grid Creation & Level Loading
    public void CreateGrid() //Creates the game grid at start or level load.
    {
        grid = new Block[currentLevelData.rows, currentLevelData.cols];

        for (int r = 0; r < currentLevelData.rows; r++)
        {
            for (int c = 0; c < currentLevelData.cols; c++)
            {
                int rand = Random.Range(0, blockFlyweights.Count);
                BlockFlyweight fw = blockFlyweights[rand];

                Block newBlock = poolManager.GetBlockFromPool();
                newBlock.transform.SetParent(BlockContainer.transform, false);

                Vector3 pos = gridManager.CalculateBlockPosition(r, c);
                newBlock.transform.localPosition = pos;

                newBlock.Initialize(fw, r, c);
                newBlock.boardManager = this;

                grid[r, c] = newBlock;
            }
        }
        bool noMoves = deadlockSystem.CheckNoMoves(grid, this);
        if (noMoves)
        {
            deadlockSystem.SmartShuffle(grid, this);
        }
    }
    public void LoadLevel(LevelData data) //Loads a level based on LevelData ScriptableObject.
    {
        currentLevelData = data;
        foreach (Transform child in transform)
        {
            Block b = child.GetComponent<Block>();
            if (b != null) poolManager.ReturnBlockToPool(b);
        }
        CreateGrid();
        groupManager.UpdateAllCombosAndSprites();
        gridManager.ScaleGridToFitScreen();
    }

    #endregion

    #region Block Interaction
    public void OnBlockClicked(Block clickedBlock) //Handles block click events from InputManager or Block class.
    {
        if (gameStateManager.CurrentState is not IdleState) return;
        groupManager.InitializeVisitedArray();
        List<Block> group = groupManager.FindGroup(clickedBlock.row, clickedBlock.col, clickedBlock.flyweight, groupManager.visited);

        if (group.Count < 2) return;

        gameStateManager.ChangeState(BoardState.Removing);
        ((RemovingState)gameStateManager.States[BoardState.Removing]).BlocksToRemove = group;
    }

    public void RemoveBlockFromGrid(Block b) //Removes a block from the grid and returns it to the block pool.
    {
        grid[b.row, b.col] = null;
        poolManager.ReturnBlockToPool(b);
    }
    public Block GetClickedBlock() //Casts a ray to get the clicked block under the mouse position.
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Block>();
        }

        return null;
    }
    #endregion
}
