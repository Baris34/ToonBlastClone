using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class DeadlockSystem : MonoBehaviour
{
    public bool CheckNoMoves(Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;
        bool[,] visited = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!visited[r, c] && grid[r, c] != null)
                {
                    List<Block> group = new List<Block>();
                    FloodFill4Way(r, c, grid[r, c].flyweight, visited, grid, bm, group);
                    if (group.Count >= 2) return false; //?2 => moves var
                }
            }
        }
        return true; // no 2+ group => deadlock
    }

    private void FloodFill4Way(int r, int c, BlockFlyweight fw,
                               bool[,] visited, Block[,] grid, BoardManager bm,
                               List<Block> group)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;
        if (r < 0 || r >= rows || c < 0 || c >= cols) return;
        if (visited[r, c]) return;
        if (grid[r, c] == null) return;
        if (grid[r, c].flyweight != fw) return;

        visited[r, c] = true;
        group.Add(grid[r, c]);

        FloodFill4Way(r + 1, c, fw, visited, grid, bm, group);
        FloodFill4Way(r - 1, c, fw, visited, grid, bm, group);
        FloodFill4Way(r, c + 1, fw, visited, grid, bm, group);
        FloodFill4Way(r, c - 1, fw, visited, grid, bm, group);
    }

    /// <summary>
    /// "Hem yatay hem dikey" yerleþtirme:
    ///  - Eðer cols>=2 => [0,0]&[0,1] (yatay)
    ///  - else if rows>=2 => [0,0]&[1,0] (dikey)
    ///  - yoksa unsolvable
    ///  
    /// Tek seferde "?2 grup" garantisi.
    /// Basit animTime=0.2f ile block'larý yerleþtirilir.
    /// </summary>
    public IEnumerator ShuffleOneGroupAnimation(Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        // Tablodaki tüm bloklarý listede topla
        List<Block> allBlocks = new List<Block>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    allBlocks.Add(grid[r, c]);
                    grid[r, c] = null;
                }
            }
        }

        // color->block list
        Dictionary<BlockFlyweight, List<Block>> colorDict = new Dictionary<BlockFlyweight, List<Block>>();
        foreach (var b in allBlocks)
        {
            if (!colorDict.ContainsKey(b.flyweight))
                colorDict[b.flyweight] = new List<Block>();
            colorDict[b.flyweight].Add(b);
        }

        // >=2 block bul
        Block b1 = null, b2 = null;
        foreach (var kvp in colorDict)
        {
            if (kvp.Value.Count >= 2)
            {
                b1 = kvp.Value[0];
                b2 = kvp.Value[1];
                break;
            }
        }
        if (b1 == null)
        {
            yield break;
        }

        if (cols >= 2)
        {
            // Yatay => [0,0] & [0,1]
            grid[0, 0] = b1;
            b1.row = 0; b1.col = 0;
            Vector3 pos1 = bm.CalculateBlockPosition(0, 0);
            b1.transform.DOLocalMove(pos1, 0.2f);

            grid[0, 1] = b2;
            b2.row = 0; b2.col = 1;
            Vector3 pos2 = bm.CalculateBlockPosition(0, 1);
            b2.transform.DOLocalMove(pos2, 0.2f);
        }
        else
        {
            yield break;
        }

        // b1,b2 listeden çýkar
        allBlocks.Remove(b1);
        allBlocks.Remove(b2);

        // Geri kalan block'larý karýþtýr
        ShuffleList(allBlocks);

        // Boþ hücrelere doldur
        float animTime = 0.2f;
        int idx = 0;
        for (int rr = 0; rr < rows; rr++)
        {
            for (int cc = 0; cc < cols; cc++)
            {
                if (grid[rr, cc] == null)
                {
                    var block = allBlocks[idx];
                    idx++;
                    grid[rr, cc] = block;
                    block.row = rr; block.col = cc;
                    Vector3 p = bm.CalculateBlockPosition(rr, cc);
                    block.transform.DOLocalMove(p, animTime);
                }
            }
        }

        yield return new WaitForSeconds(animTime);
        bm.UpdateAllCombosAndSprites();
    }

    private void ShuffleList(List<Block> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            var tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }
}
