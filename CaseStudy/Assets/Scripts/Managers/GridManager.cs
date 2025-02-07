using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

//Manages the game grid, including block positioning and scaling.
public class GridManager
{
    #region Variables
    private BoardManager boardManager;
    public Block[,] grid { get; private set; }
    private int rows;
    private int cols;
    #endregion


    public GridManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
        this.grid = boardManager.grid;
        this.rows = boardManager.currentLevelData.rows;
        this.cols = boardManager.currentLevelData.cols;
    }
    public Vector3 CalculateBlockPosition(int row, int col) //Calculates the position of a block based on its row and column.
    {
        float xPos = col * boardManager.cellSize.x;
        float yPos = row * boardManager.cellSize.y;
        return new Vector3(xPos, yPos, 0f);
    }



    public void ScaleGridToFitScreen()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
            return;

        // 1. Grid'in orijinal boyutlarýný hesaplayýn (hücre boyutlarýna göre)
        float gridW = cols * boardManager.cellSize.x; // Toplam grid geniþliði
        float gridH = rows * boardManager.cellSize.y; // Toplam grid yüksekliði

        // 2. Kameranýn görünüm boyutlarýný hesaplayýn (dünya birimlerinde)
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = cam.aspect * camHeight;

        // 3. Ekraný dolduracak ölçek faktörlerini hesaplayýn
        float scaleX = camWidth / gridW;
        float scaleY = camHeight / gridH;
        float finalScale = Mathf.Min(scaleX, scaleY);

        // 4. BlockContainer üzerinde ölçeklemeyi uygulayýn
        boardManager.BlockContainer.transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // 5. Ölçeklendirilmiþ grid boyutlarýný hesaplayýn
        float scaledGridW = gridW * finalScale;
        float scaledGridH = gridH * finalScale;

        // 6. Kameranýn merkezini (dünya koordinatlarýnda) alýn
        Vector3 camPos = cam.transform.position;

        /* 
           Eðer bloklarýn konumlandýrýlmasýnda her hücre sol alt köþeye yerleþtiriliyorsa,
           fakat blok sprite'larýnýn pivotu merkezdeyse, görsel olarak grid’in merkezi
           (blok sprite'larýnýn merkezi) hücre boyutunun yarýsý kadar saða ve yukarý kayar.

           Bu durumda, grid’in görsel merkezini (yani, BlockContainer içindeki bloklarýn 
           gerçek merkezini) kamera merkezine denk getirmek için ek bir offset ekleyebilirsiniz.

           extraOffsetX ve extraOffsetY, bu ayarlamayý yapmak için kullanýlan deðerlerdir.
           Eðer blok sprite pivotlarýnýz farklýysa, bu deðerleri ayarlamanýz gerekebilir.
        */
        float extraOffsetX = boardManager.cellSize.x * finalScale / 2f;
        float extraOffsetY = boardManager.cellSize.y * finalScale / 2f;

        // 7. Grid'in sol alt köþesinin (pivot) konumunu hesaplayýn
        //    Grid'in merkezi, sol alt köþeden (scaledGridW/2, scaledGridH/2) kadar uzaklýkta olmalý.
        //    Eðer görsel olarak bloklar merkezdeyse, ek offset eklenir.
        Vector3 bottomLeft = new Vector3(
            camPos.x - scaledGridW / 2f + extraOffsetX,
            camPos.y - scaledGridH / 2f + extraOffsetY,
            0f
        );

        // 8. BlockContainer'ý bu konuma yerleþtirin
        boardManager.BlockContainer.transform.position = bottomLeft;
    }


}
