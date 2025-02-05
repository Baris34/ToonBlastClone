using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Block : MonoBehaviour
{
    public BlockFlyweight flyweight;
    public int row;
    public int col;
    public bool isMatched = false;

    private SpriteRenderer spriteRenderer;

    [HideInInspector] public BoardManager boardManager;

    [SerializeField] private Material commonMaterial;

    private MaterialPropertyBlock _propBlock;

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

    public void Initialize(BlockFlyweight fw, int r, int c)
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

    public void UpdateIconByGroupSize(int groupSize, LevelData levelData)
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

    private void OnMouseDown()
    {
        if (boardManager != null)
        {
            boardManager.OnBlockClicked(this);
        }
    }
}
