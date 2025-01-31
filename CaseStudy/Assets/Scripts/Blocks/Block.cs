using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))] // 2D týklama için gereken collider
public class Block : MonoBehaviour
{
    public BlockFlyweight flyweight;
    public int row;
    public int col;
    public bool isMatched = false;
    private SpriteRenderer spriteRenderer;

    // GridManager referansýný, blok týklandýðýnda haber verebilmek için tutuyoruz
    [HideInInspector]
    public BoardManager boardManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // BoxCollider2D ekli olduðunu da varsayýyoruz (RequireComponent kullandýk)
    }

    // Pool’dan geldiðinde veya yeni oluþturulduðunda ilk ayarlar
    public void Initialize(BlockFlyweight fw, int r, int c)
    {
        flyweight = fw;
        row = r;
        col = c;
        isMatched = false;

        if (spriteRenderer != null && fw != null)
        {
            spriteRenderer.sprite = fw.sprite; // Normal sprite
        }
    }

    // Grup boyutuna göre sprite deðiþtiren metod (combo logic)
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

    // 2D’de týklamayý bu þekilde alýyoruz
    private void OnMouseDown()
    {
        if (boardManager != null)
        {
            boardManager.OnBlockClicked(this);
        }
    }
}