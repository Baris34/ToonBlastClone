using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SpriteAtlasGet : MonoBehaviour
{
    public SpriteAtlas spriteAtlas;
    private void Start()
    {
        spriteAtlas.GetSprite("SpriteName");
    }
}
