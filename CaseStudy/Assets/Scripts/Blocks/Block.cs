using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))] // 2D t�klama i�in gereken collider
public class Block : MonoBehaviour
{
    public BlockFlyweight flyweight;
    public int row;
    public int col;
    public bool isMatched = false;
    private SpriteRenderer spriteRenderer;

    // GridManager referans�n�, blok t�kland���nda haber verebilmek i�in tutuyoruz
    [HideInInspector]
    public BoardManager boardManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // BoxCollider2D ekli oldu�unu da varsay�yoruz (RequireComponent kulland�k)
    }

    // Pool�dan geldi�inde veya yeni olu�turuldu�unda ilk ayarlar
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

    // Grup boyutuna g�re sprite de�i�tiren metod (combo logic)
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

    // 2D�de t�klamay� bu �ekilde al�yoruz
    private void OnMouseDown()
    {
        if (boardManager != null)
        {
            boardManager.OnBlockClicked(this);
        }
    }
}