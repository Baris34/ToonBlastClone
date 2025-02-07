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

        // 1. Grid'in orijinal boyutlar�n� hesaplay�n (h�cre boyutlar�na g�re)
        float gridW = cols * boardManager.cellSize.x; // Toplam grid geni�li�i
        float gridH = rows * boardManager.cellSize.y; // Toplam grid y�ksekli�i

        // 2. Kameran�n g�r�n�m boyutlar�n� hesaplay�n (d�nya birimlerinde)
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = cam.aspect * camHeight;

        // 3. Ekran� dolduracak �l�ek fakt�rlerini hesaplay�n
        float scaleX = camWidth / gridW;
        float scaleY = camHeight / gridH;
        float finalScale = Mathf.Min(scaleX, scaleY);

        // 4. BlockContainer �zerinde �l�eklemeyi uygulay�n
        boardManager.BlockContainer.transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // 5. �l�eklendirilmi� grid boyutlar�n� hesaplay�n
        float scaledGridW = gridW * finalScale;
        float scaledGridH = gridH * finalScale;

        // 6. Kameran�n merkezini (d�nya koordinatlar�nda) al�n
        Vector3 camPos = cam.transform.position;

        /* 
           E�er bloklar�n konumland�r�lmas�nda her h�cre sol alt k��eye yerle�tiriliyorsa,
           fakat blok sprite'lar�n�n pivotu merkezdeyse, g�rsel olarak grid�in merkezi
           (blok sprite'lar�n�n merkezi) h�cre boyutunun yar�s� kadar sa�a ve yukar� kayar.

           Bu durumda, grid�in g�rsel merkezini (yani, BlockContainer i�indeki bloklar�n 
           ger�ek merkezini) kamera merkezine denk getirmek i�in ek bir offset ekleyebilirsiniz.

           extraOffsetX ve extraOffsetY, bu ayarlamay� yapmak i�in kullan�lan de�erlerdir.
           E�er blok sprite pivotlar�n�z farkl�ysa, bu de�erleri ayarlaman�z gerekebilir.
        */
        float extraOffsetX = boardManager.cellSize.x * finalScale / 2f;
        float extraOffsetY = boardManager.cellSize.y * finalScale / 2f;

        // 7. Grid'in sol alt k��esinin (pivot) konumunu hesaplay�n
        //    Grid'in merkezi, sol alt k��eden (scaledGridW/2, scaledGridH/2) kadar uzakl�kta olmal�.
        //    E�er g�rsel olarak bloklar merkezdeyse, ek offset eklenir.
        Vector3 bottomLeft = new Vector3(
            camPos.x - scaledGridW / 2f + extraOffsetX,
            camPos.y - scaledGridH / 2f + extraOffsetY,
            0f
        );

        // 8. BlockContainer'� bu konuma yerle�tirin
        boardManager.BlockContainer.transform.position = bottomLeft;
    }


}
