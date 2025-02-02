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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 1) Tüm bloklar ayný ‘commonMaterial’ kullansýn
        // (Örn. “URP 2D -> Sprite-Unlit-Default”)
        if (commonMaterial != null)
        {
            // sharedMaterial kullanarak orijinal instance’ý koruruz
            spriteRenderer.sharedMaterial = commonMaterial;
        }

        // 2) Ayný sorting layer + order:
        // “Default” layer, order=0 (deðiþtirmiyorsak)
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
            // Normal sprite: 
            // (Assuming all these sprites are in the same atlas)
            spriteRenderer.sprite = flyweight.sprite;
        }
    }

    // Grup boyutuna göre sprite deðiþtirmek (threshold logic)
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
