using UnityEngine;

[CreateAssetMenu(fileName = "BlockFlyweight", menuName = "Scriptable Objects/BlockFlyweight")]
public class BlockFlyweight : ScriptableObject
{
    public Sprite sprite;
    public Sprite firstComboSprite;
    public Sprite secondComboSprite;
    public Sprite thirdComboSprite;

    public void SetData(Sprite sprite,Sprite firstComboSprite, Sprite secondComboSprite, Sprite thirdComboSprite)
    {
        this.sprite = sprite;
        this.firstComboSprite = firstComboSprite;
        this.secondComboSprite = secondComboSprite;
        this.thirdComboSprite = thirdComboSprite;
    }
}
