using UnityEngine;

[CreateAssetMenu(fileName = "BlockFlyweight", menuName = "Scriptable Objects/BlockFlyweight")]
//Stores the data for a block, including its sprite and combo sprites.
public class BlockFlyweight : ScriptableObject
{
    public Sprite sprite;
    public Sprite firstComboSprite;
    public Sprite secondComboSprite;
    public Sprite thirdComboSprite;

    public void SetData(Sprite sprite,Sprite firstComboSprite, Sprite secondComboSprite, Sprite thirdComboSprite) //Sets the data for the block.
    {
        this.sprite = sprite;
        this.firstComboSprite = firstComboSprite;
        this.secondComboSprite = secondComboSprite;
        this.thirdComboSprite = thirdComboSprite;
    }
}
