using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public BlockFlyweight flyweight;
    public int row;
    public int col;
    public bool isMatched = false; // Eþleþti mi?
    private Image imageComp;

    public GridManager gridManager;
    private void Awake()
    {
        imageComp = GetComponent<Image>();
    }

    public void Initialize(BlockFlyweight flyweight, int row, int col)
    {
        this.flyweight = flyweight;
        this.row = row;
        this.col = col;
        imageComp.sprite = flyweight.sprite;
        // GetComponent<Image>().color = flyweight.color; // Eðer renk bilgisini BlockFlyweight'te tutuyorsan
    }
    public void UpdateIconByGroupSize(int groupSize, LevelData levelData)
    {
        if (groupSize <= levelData.comboThresholdA)
        {
            imageComp.sprite = flyweight.sprite;
        }
        else if (groupSize <= levelData.comboThresholdB)
        {
            imageComp.sprite = flyweight.firstComboSprite;
        }
        else if (groupSize <= levelData.comboThresholdC)
        {
            imageComp.sprite = flyweight.secondComboSprite;
        }
        else
        {
            imageComp.sprite = flyweight.thirdComboSprite;
        }
    }
}
