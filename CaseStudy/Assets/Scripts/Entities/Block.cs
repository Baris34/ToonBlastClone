using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]

//Represents a block in the game board
public class Block : MonoBehaviour
{
    #region Variables
    public BlockFlyweight flyweight;
    public int row;
    public int col;
    public bool isMatched = false;

    private SpriteRenderer spriteRenderer;

    [HideInInspector] public BoardManager boardManager;

    [SerializeField] private Material commonMaterial;
    #endregion

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (commonMaterial != null)
        {
            spriteRenderer.sharedMaterial = commonMaterial;
        }
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 0;
    }

    public void Initialize(BlockFlyweight fw, int r, int c) //Initializes the block with a BlockFlyweight and position in the grid
    {
        flyweight = fw;
        row = r;
        col = c;
        isMatched = false;

        if (spriteRenderer != null && flyweight != null)
        {
            spriteRenderer.sprite = flyweight.sprite;
        }
    }

    public void UpdateIconByGroupSize(int groupSize, LevelData levelData) //Updates the block's sprite based on the group size
    {
        if (spriteRenderer == null || flyweight == null) return;

        if (groupSize <= levelData.comboThresholdA)
        {
            spriteRenderer.sprite = flyweight.sprite;
        }
        else if (groupSize <= levelData.comboThresholdB)
        {
            spriteRenderer.sprite = flyweight.firstComboSprite;
        }
        else if (groupSize <= levelData.comboThresholdC)
        {
            spriteRenderer.sprite = flyweight.secondComboSprite;
        }
        else
        {
            spriteRenderer.sprite = flyweight.thirdComboSprite;
        }
    }

    private void OnMouseDown() //Handles block click events
    {
        if (boardManager != null)
        {
            boardManager.OnBlockClicked(this);
        }
    }
}
