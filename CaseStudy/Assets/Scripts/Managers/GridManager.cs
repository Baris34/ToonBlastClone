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

        float gridW = cols * boardManager.cellSize.x;
        float gridH = rows * boardManager.cellSize.y;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = cam.aspect * camHeight;

        float scaleX = camWidth / gridW;
        float scaleY = camHeight / gridH;
        float finalScale = Mathf.Min(scaleX, scaleY);

        boardManager.BlockContainer.transform.localScale = new Vector3(finalScale, finalScale, 1f);

        float scaledGridW = gridW * finalScale;
        float scaledGridH = gridH * finalScale;

        Vector3 camPos = cam.transform.position;

        float extraOffsetX = boardManager.cellSize.x * finalScale / 2f;
        float extraOffsetY = boardManager.cellSize.y * finalScale / 2f;

        Vector3 bottomLeft = new Vector3(
            camPos.x - scaledGridW / 2f + extraOffsetX,
            camPos.y - scaledGridH / 2f + extraOffsetY,
            0f
        );
        boardManager.BlockContainer.transform.position = bottomLeft;
    }


}
