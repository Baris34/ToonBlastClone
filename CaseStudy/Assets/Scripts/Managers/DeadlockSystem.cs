using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class DeadlockSystem : MonoBehaviour
{


    public bool IsShuffling { get; private set; }
    private float shuffleTimer=0.3f;
    private List<Block> shuffleBlocks;
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
    /// "Hem yatay hem dikey" yerle�tirme:
    ///  - E�er cols>=2 => [0,0]&[0,1] (yatay)
    ///  - else if rows>=2 => [0,0]&[1,0] (dikey)
    ///  - yoksa unsolvable
    ///  
    /// Tek seferde "?2 grup" garantisi.
    /// Basit animTime=0.2f ile block'lar� yerle�tirilir.
    /// </summary>
    public void StartShuffle(Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        // Tablodaki t�m bloklar� listede topla
        List<Block> allBlocks = new List<Block>();
        Dictionary<BlockFlyweight, int> colorCounts = new Dictionary<BlockFlyweight, int>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    allBlocks.Add(grid[r, c]);
                    if (!colorCounts.ContainsKey(grid[r, c].flyweight))
                    {
                        colorCounts[grid[r, c].flyweight] = 0;
                    }
                    colorCounts[grid[r, c].flyweight]++;
                    grid[r, c] = null; // Grid'deki bloklar� temizle
                }
            }
        }

        // En fazla renge sahip olan� bul
        BlockFlyweight mostCommonColor = null;
        int maxCount = 0;
        foreach (var kvp in colorCounts)
        {
            if (kvp.Value > maxCount)
            {
                mostCommonColor = kvp.Key;
                maxCount = kvp.Value;
            }
        }

        // En fazla renkten olan bloklar� ay�r
        List<Block> mostCommonColorBlocks = new List<Block>();
        List<Block> otherBlocks = new List<Block>();
        foreach (Block block in allBlocks)
        {
            if (block.flyweight == mostCommonColor)
            {
                mostCommonColorBlocks.Add(block);
            }
            else
            {
                otherBlocks.Add(block);
            }
        }

        // Di�er bloklar� Fisher-Yates algoritmas�yla kar��t�r
        FisherYatesShuffle(otherBlocks);

        // Grid'e yerle�tirme
        int index = 0;

        // �nce en fazla renkten olan bloklar� grid'in �st k�sm�na yatay olarak yerle�tir
        foreach (Block block in mostCommonColorBlocks)
        {
            int r = index / cols; // Sat�r hesaplama
            int c = index % cols; // S�tun hesaplama

            // E�er sat�r dolmu�sa bir alt sat�ra ge�
            if (c == 0 && r > 0)
            {
                index = r * cols;
            }

            grid[r, c] = block;
            block.row = r;
            block.col = c;
            Vector3 pos = bm.CalculateBlockPosition(r, c);
            block.transform.DOLocalMove(pos, 0.2f); // Bloklar� animasyonla ta��
            index++;
        }

        // Kalan bloklar� grid'in alt�na rastgele yerle�tir
        foreach (Block block in otherBlocks)
        {
            while (grid[index / cols, index % cols] != null)
            {
                index++; // Bo� bir h�cre bulana kadar devam et
            }

            int r = index / cols;
            int c = index % cols;

            grid[r, c] = block;
            block.row = r;
            block.col = c;
            Vector3 pos = bm.CalculateBlockPosition(r, c);
            block.transform.DOLocalMove(pos, 0.2f); // Bloklar� animasyonla ta��
            index++;
        }

        
    }


    private void FisherYatesShuffle(List<Block> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            Block temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}
