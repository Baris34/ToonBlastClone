using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class DeadlockSystem : MonoBehaviour
{
    public bool IsShuffling { get; private set; }
    private float shuffleTimer = 0.3f; // Animasyon süresi

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
    /// Board’da herhangi bir 2 veya daha fazla bitiþik (yatay/dikey) ayný flyweight bloðu varsa,
    /// hamle yapýlabilir demektir. Yoksa deadlock var.
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
        return true; // Hiçbir 2+ grup yok => deadlock.
    }

    private void FloodFill4WayIterative(int startRow, int startCol, BlockFlyweight fw,
                                      bool[,] visited, Block[,] grid, BoardManager bm,
                                      List<Block> group)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        // Geçersiz baþlangýç kontrolü:
        if (startRow < 0 || startRow >= rows || startCol < 0 || startCol >= cols) return;
        if (visited[startRow, startCol]) return;
        if (grid[startRow, startCol] == null) return;
        if (grid[startRow, startCol].flyweight != fw) return;

        // Önceden yeterli kapasiteye sahip bir kuyruk oluþturuyoruz.
        Queue<Vector2Int> queue = new Queue<Vector2Int>(rows * cols);
        visited[startRow, startCol] = true;
        queue.Enqueue(new Vector2Int(startRow, startCol));

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            int r = cell.x;
            int c = cell.y;
            group.Add(grid[r, c]);

            // Aþaðý yöndeki komþu:
            if (r + 1 < rows && !visited[r + 1, c] && grid[r + 1, c] != null && grid[r + 1, c].flyweight == fw)
            {
                visited[r + 1, c] = true;
                queue.Enqueue(new Vector2Int(r + 1, c));
            }
            // Yukarý yöndeki komþu:
            if (r - 1 >= 0 && !visited[r - 1, c] && grid[r - 1, c] != null && grid[r - 1, c].flyweight == fw)
            {
                visited[r - 1, c] = true;
                queue.Enqueue(new Vector2Int(r - 1, c));
            }
            // Sað yöndeki komþu:
            if (c + 1 < cols && !visited[r, c + 1] && grid[r, c + 1] != null && grid[r, c + 1].flyweight == fw)
            {
                visited[r, c + 1] = true;
                queue.Enqueue(new Vector2Int(r, c + 1));
            }
            // Sol yöndeki komþu:
            if (c - 1 >= 0 && !visited[r, c - 1] && grid[r, c - 1] != null && grid[r, c - 1].flyweight == fw)
            {
                visited[r, c - 1] = true;
                queue.Enqueue(new Vector2Int(r, c - 1));
            }
        }
    }

    /// <summary>
    /// Akýllý Shuffle (Karýþtýrma) Algoritmasý:
    /// 
    /// Deadlock durumunda tahtadaki bloklarýn yeniden yerleþtirilmesini,
    /// garanti olarak en az bir geçerli hamlenin (bitiþik ayný renk iki blok)
    /// oluþturulacaðý þekilde yapar.
    /// 
    /// Adýmlar:
    /// 1. Tüm bloklarý listeye aktar.
    /// 2. Her flyweight için grup (renk grubu) oluþtur.
    /// 3. En az 2 bloða sahip bir flyweight seç (garantili hamle oluþturabilmek için).
    /// 4. Seçilen renk grubundan 2 blok seçilerek, tahtada bitiþik iki pozisyona yerleþtir.
    /// 5. Kalan bloklarý ve pozisyonlarý rastgele karýþtýrýp yerleþtir.
    /// 6. Bloklarýn transform pozisyonlarý animasyonla güncellenir.
    /// 
    /// Böylece, “blindly N kez karýþtýr” yaklaþýmýna gerek kalmadan, garanti bir
    /// geçerli hamle elde edilmiþ olur.
    /// </summary>
    public void SmartShuffle(Block[,] grid, BoardManager bm)
    {
        int rows = bm.currentLevelData.rows;
        int cols = bm.currentLevelData.cols;

        // 1. Mevcut tüm bloklarý listeye aktar.
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

        // 2. Flyweight (renk) gruplarýný oluþtur.
        Dictionary<BlockFlyweight, List<Block>> groups = new Dictionary<BlockFlyweight, List<Block>>();
        foreach (Block block in allBlocks)
        {
            if (!groups.ContainsKey(block.flyweight))
            {
                groups[block.flyweight] = new List<Block>();
            }
            groups[block.flyweight].Add(block);
        }

        // 3. En az 2 bloða sahip bir flyweight seç (garantili hamle için).
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
            Debug.LogError("Shuffle hatasý: Hiçbir renk için yeterli blok bulunamadý.");
            // Eðer mümkün deðilse, tüm bloklarý rastgele karýþtýrarak yerleþtir.
            FisherYatesShuffle(allBlocks);
            FillGridWithBlocks(allBlocks, grid, bm);
            return;
        }

        // 4. Seçilen renk grubundan 2 blok al (garantili eþleþtirme için).
        List<Block> chosenBlocks = groups[chosenFlyweight];
        Block pairBlock1 = chosenBlocks[0];
        Block pairBlock2 = chosenBlocks[1];

        // Bu iki bloðu ana listeden çýkar.
        allBlocks.Remove(pairBlock1);
        allBlocks.Remove(pairBlock2);

        // 5. Tüm grid pozisyonlarýný oluþtur.
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                positions.Add(new Vector2Int(r, c));
            }
        }

        // 6. Tabloda bitiþik (yatay ya da dikey) pozisyon çiftlerini belirle.
        List<(Vector2Int, Vector2Int)> adjacentPairs = new List<(Vector2Int, Vector2Int)>();
        foreach (Vector2Int pos in positions)
        {
            // Sað komþu kontrolü
            if (pos.y < cols - 1)
            {
                adjacentPairs.Add((pos, new Vector2Int(pos.x, pos.y + 1)));
            }
            // Alt komþu kontrolü
            if (pos.x < rows - 1)
            {
                adjacentPairs.Add((pos, new Vector2Int(pos.x + 1, pos.y)));
            }
        }

        // Rastgele bir bitiþik pozisyon çifti seç.
        var chosenPair = adjacentPairs[Random.Range(0, adjacentPairs.Count)];

        // Seçilen pozisyon çiftini genel pozisyon listesinden çýkar.
        positions.Remove(chosenPair.Item1);
        positions.Remove(chosenPair.Item2);

        // 7. Kalan pozisyonlarý ve bloklarý karýþtýr.
        FisherYatesShuffle(positions);
        FisherYatesShuffle(allBlocks);

        // 8. Yeni grid oluþturup bloklarý yerleþtir.
        Block[,] newGrid = new Block[rows, cols];

        // Garantili eþleþtirme: seçilen çift pozisyona ayný flyweight bloklarý yerleþtir.
        pairBlock1.row = chosenPair.Item1.x;
        pairBlock1.col = chosenPair.Item1.y;
        pairBlock2.row = chosenPair.Item2.x;
        pairBlock2.col = chosenPair.Item2.y;
        newGrid[chosenPair.Item1.x, chosenPair.Item1.y] = pairBlock1;
        newGrid[chosenPair.Item2.x, chosenPair.Item2.y] = pairBlock2;

        // Kalan pozisyonlara kalan bloklarý yerleþtir.
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

        // 9. Yeni grid’i mevcut grid’e kopyala ve bloklarýn pozisyonlarýný güncelle.
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = newGrid[r, c];
                if (grid[r, c] != null)
                {
                    Vector3 targetPos = bm.CalculateBlockPosition(r, c);
                    // DOTween ile animasyonlu pozisyon güncellemesi:
                    grid[r, c].transform.DOLocalMove(targetPos, shuffleTimer).SetEase(Ease.OutBounce);
                }
            }
        }

        Debug.Log("Tahta, deadlock durumunu çözmek için akýllýca karýþtýrýldý.");
    }

    /// <summary>
    /// Deadlock için ekstra bir kontrol: Eðer yeterli eþleþme saðlayacak renk yoksa,
    /// tüm bloklarý rastgele karýþtýrýp grid’e yerleþtirir.
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
