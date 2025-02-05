using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class DeadlockSystem : MonoBehaviour
{
    public bool IsShuffling { get; private set; }
    private float shuffleTimer = 0.3f; // Animasyon s�resi

    #region Generic Fisher-Yates Shuffle
    private void FisherYatesShuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
    #endregion

    /// <summary>
    /// Board�da herhangi bir 2 veya daha fazla biti�ik (yatay/dikey) ayn� flyweight blo�u varsa,
    /// hamle yap�labilir demektir. Yoksa deadlock var.
    /// </summary>
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
                    FloodFill4WayIterative(r, c, grid[r, c].flyweight, visited, grid, bm, group);
                    if (group.Count >= 2) return false; // En az 2 bloklu bir grup bulundu => hamle var.
                }
            }
        }
        return true; // Hi�bir 2+ grup yok => deadlock.
    }

    private void FloodFill4WayIterative(int startRow, int startCol, BlockFlyweight fw,
                                      bool[,] visited, Block[,] grid, BoardManager bm,
                                      List<Block> group)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        // Ge�ersiz ba�lang�� kontrol�:
        if (startRow < 0 || startRow >= rows || startCol < 0 || startCol >= cols) return;
        if (visited[startRow, startCol]) return;
        if (grid[startRow, startCol] == null) return;
        if (grid[startRow, startCol].flyweight != fw) return;

        // �nceden yeterli kapasiteye sahip bir kuyruk olu�turuyoruz.
        Queue<Vector2Int> queue = new Queue<Vector2Int>(rows * cols);
        visited[startRow, startCol] = true;
        queue.Enqueue(new Vector2Int(startRow, startCol));

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            int r = cell.x;
            int c = cell.y;
            group.Add(grid[r, c]);

            // A�a�� y�ndeki kom�u:
            if (r + 1 < rows && !visited[r + 1, c] && grid[r + 1, c] != null && grid[r + 1, c].flyweight == fw)
            {
                visited[r + 1, c] = true;
                queue.Enqueue(new Vector2Int(r + 1, c));
            }
            // Yukar� y�ndeki kom�u:
            if (r - 1 >= 0 && !visited[r - 1, c] && grid[r - 1, c] != null && grid[r - 1, c].flyweight == fw)
            {
                visited[r - 1, c] = true;
                queue.Enqueue(new Vector2Int(r - 1, c));
            }
            // Sa� y�ndeki kom�u:
            if (c + 1 < cols && !visited[r, c + 1] && grid[r, c + 1] != null && grid[r, c + 1].flyweight == fw)
            {
                visited[r, c + 1] = true;
                queue.Enqueue(new Vector2Int(r, c + 1));
            }
            // Sol y�ndeki kom�u:
            if (c - 1 >= 0 && !visited[r, c - 1] && grid[r, c - 1] != null && grid[r, c - 1].flyweight == fw)
            {
                visited[r, c - 1] = true;
                queue.Enqueue(new Vector2Int(r, c - 1));
            }
        }
    }

    /// <summary>
    /// Ak�ll� Shuffle (Kar��t�rma) Algoritmas�:
    /// 
    /// Deadlock durumunda tahtadaki bloklar�n yeniden yerle�tirilmesini,
    /// garanti olarak en az bir ge�erli hamlenin (biti�ik ayn� renk iki blok)
    /// olu�turulaca�� �ekilde yapar.
    /// 
    /// Ad�mlar:
    /// 1. T�m bloklar� listeye aktar.
    /// 2. Her flyweight i�in grup (renk grubu) olu�tur.
    /// 3. En az 2 blo�a sahip bir flyweight se� (garantili hamle olu�turabilmek i�in).
    /// 4. Se�ilen renk grubundan 2 blok se�ilerek, tahtada biti�ik iki pozisyona yerle�tir.
    /// 5. Kalan bloklar� ve pozisyonlar� rastgele kar��t�r�p yerle�tir.
    /// 6. Bloklar�n transform pozisyonlar� animasyonla g�ncellenir.
    /// 
    /// B�ylece, �blindly N kez kar��t�r� yakla��m�na gerek kalmadan, garanti bir
    /// ge�erli hamle elde edilmi� olur.
    /// </summary>
    public void SmartShuffle(Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        // 1. Mevcut t�m bloklar� listeye aktar.
        List<Block> allBlocks = new List<Block>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    allBlocks.Add(grid[r, c]);
                }
            }
        }

        // 2. Flyweight (renk) gruplar�n� olu�tur.
        Dictionary<BlockFlyweight, List<Block>> groups = new Dictionary<BlockFlyweight, List<Block>>();
        foreach (Block block in allBlocks)
        {
            if (!groups.ContainsKey(block.flyweight))
            {
                groups[block.flyweight] = new List<Block>();
            }
            groups[block.flyweight].Add(block);
        }

        // 3. En az 2 blo�a sahip bir flyweight se� (garantili hamle i�in).
        BlockFlyweight chosenFlyweight = null;
        foreach (var kvp in groups)
        {
            if (kvp.Value.Count >= 2)
            {
                chosenFlyweight = kvp.Key;
                break;
            }
        }
        if (chosenFlyweight == null)
        {
            Debug.LogError("Shuffle hatas�: Hi�bir renk i�in yeterli blok bulunamad�.");
            // E�er m�mk�n de�ilse, t�m bloklar� rastgele kar��t�rarak yerle�tir.
            FisherYatesShuffle(allBlocks);
            FillGridWithBlocks(allBlocks, grid, bm);
            return;
        }

        // 4. Se�ilen renk grubundan 2 blok al (garantili e�le�tirme i�in).
        List<Block> chosenBlocks = groups[chosenFlyweight];
        Block pairBlock1 = chosenBlocks[0];
        Block pairBlock2 = chosenBlocks[1];

        // Bu iki blo�u ana listeden ��kar.
        allBlocks.Remove(pairBlock1);
        allBlocks.Remove(pairBlock2);

        // 5. T�m grid pozisyonlar�n� olu�tur.
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                positions.Add(new Vector2Int(r, c));
            }
        }

        // 6. Tabloda biti�ik (yatay ya da dikey) pozisyon �iftlerini belirle.
        List<(Vector2Int, Vector2Int)> adjacentPairs = new List<(Vector2Int, Vector2Int)>();
        foreach (Vector2Int pos in positions)
        {
            // Sa� kom�u kontrol�
            if (pos.y < cols - 1)
            {
                adjacentPairs.Add((pos, new Vector2Int(pos.x, pos.y + 1)));
            }
            // Alt kom�u kontrol�
            if (pos.x < rows - 1)
            {
                adjacentPairs.Add((pos, new Vector2Int(pos.x + 1, pos.y)));
            }
        }

        // Rastgele bir biti�ik pozisyon �ifti se�.
        var chosenPair = adjacentPairs[Random.Range(0, adjacentPairs.Count)];

        // Se�ilen pozisyon �iftini genel pozisyon listesinden ��kar.
        positions.Remove(chosenPair.Item1);
        positions.Remove(chosenPair.Item2);

        // 7. Kalan pozisyonlar� ve bloklar� kar��t�r.
        FisherYatesShuffle(positions);
        FisherYatesShuffle(allBlocks);

        // 8. Yeni grid olu�turup bloklar� yerle�tir.
        Block[,] newGrid = new Block[rows, cols];

        // Garantili e�le�tirme: se�ilen �ift pozisyona ayn� flyweight bloklar� yerle�tir.
        pairBlock1.row = chosenPair.Item1.x;
        pairBlock1.col = chosenPair.Item1.y;
        pairBlock2.row = chosenPair.Item2.x;
        pairBlock2.col = chosenPair.Item2.y;
        newGrid[chosenPair.Item1.x, chosenPair.Item1.y] = pairBlock1;
        newGrid[chosenPair.Item2.x, chosenPair.Item2.y] = pairBlock2;

        // Kalan pozisyonlara kalan bloklar� yerle�tir.
        int index = 0;
        foreach (Vector2Int pos in positions)
        {
            if (index < allBlocks.Count)
            {
                Block b = allBlocks[index];
                b.row = pos.x;
                b.col = pos.y;
                newGrid[pos.x, pos.y] = b;
                index++;
            }
        }

        // 9. Yeni grid�i mevcut grid�e kopyala ve bloklar�n pozisyonlar�n� g�ncelle.
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = newGrid[r, c];
                if (grid[r, c] != null)
                {
                    Vector3 targetPos = bm.CalculateBlockPosition(r, c);
                    // DOTween ile animasyonlu pozisyon g�ncellemesi:
                    grid[r, c].transform.DOLocalMove(targetPos, shuffleTimer).SetEase(Ease.OutBounce);
                }
            }
        }

        Debug.Log("Tahta, deadlock durumunu ��zmek i�in ak�ll�ca kar��t�r�ld�.");
    }

    /// <summary>
    /// Deadlock i�in ekstra bir kontrol: E�er yeterli e�le�me sa�layacak renk yoksa,
    /// t�m bloklar� rastgele kar��t�r�p grid�e yerle�tirir.
    /// </summary>
    private void FillGridWithBlocks(List<Block> blocks, Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                positions.Add(new Vector2Int(r, c));
            }
        }
        FisherYatesShuffle(positions);
        Block[,] newGrid = new Block[rows, cols];
        int index = 0;
        foreach (Vector2Int pos in positions)
        {
            if (index < blocks.Count)
            {
                Block b = blocks[index];
                b.row = pos.x;
                b.col = pos.y;
                newGrid[pos.x, pos.y] = b;
                index++;
            }
        }
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = newGrid[r, c];
                if (grid[r, c] != null)
                {
                    Vector3 targetPos = bm.CalculateBlockPosition(r, c);
                    grid[r, c].transform.DOLocalMove(targetPos, shuffleTimer).SetEase(Ease.OutBounce);
                }
            }
        }
    }
}
