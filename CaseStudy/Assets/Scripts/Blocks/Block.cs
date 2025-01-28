using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public BlockFlyweight flyweight;
    public int row;
    public int col;
    public bool isMatched = false; // Eþleþti mi?

    public void Initialize(BlockFlyweight flyweight, int row, int col)
    {
        this.flyweight = flyweight;
        this.row = row;
        this.col = col;
        GetComponent<Image>().sprite = flyweight.sprite;
        // GetComponent<Image>().color = flyweight.color; // Eðer renk bilgisini BlockFlyweight'te tutuyorsan
    }
    public void UpdateIconByGroupSize(int groupSize, LevelData levelData)
    {
        if (groupSize <= levelData.comboThresholdA)
        {
            GetComponent<Image>().sprite = flyweight.sprite;
        }
        else if (groupSize <= levelData.comboThresholdB)
        {
            GetComponent<Image>().sprite = flyweight.firstComboSprite;
        }
        else if(groupSize <= levelData.comboThresholdC)
        {
            GetComponent<Image>().sprite = flyweight.secondComboSprite;
        }
        else
        {
            GetComponent<Image>().sprite = flyweight.thirdComboSprite;
        }
    }
    void Start()
    {
        
    }
    
    void Update()
    {

    }
    
    
}
