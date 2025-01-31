using UnityEngine;
using System.Collections.Generic;

public class DeadlockSystem : MonoBehaviour
{
    public void ShuffleToGuaranteeOneGroup(Block[,] grid, BoardManager boardManager)
    {
        int rows = boardManager.currentLevelData.rows;
        int cols = boardManager.currentLevelData.cols;

        // 1) Tüm bloklarý listeye al
        List<Block> allBlocks = new List<Block>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    allBlocks.Add(grid[r, c]);
                    grid[r, c] = null; // tabloyu temizle
                }
            }
        }

        // 2) Renk -> block listesi dictionary
        Dictionary<BlockFlyweight, List<Block>> colorDict = new Dictionary<BlockFlyweight, List<Block>>();
        foreach (Block b in allBlocks)
        {
            if (!colorDict.ContainsKey(b.flyweight))
            {
                colorDict[b.flyweight] = new List<Block>();
            }
            colorDict[b.flyweight].Add(b);
        }

        // 3) Bir renkten en az 2 bloðu bul
        BlockFlyweight chosenFlyweight = null;
        Block b1 = null, b2 = null;
        foreach (var kvp in colorDict)
        {
            if (kvp.Value.Count >= 2)
            {
                chosenFlyweight = kvp.Key;
                // Mesela ilk 2 bloðu alalým
                b1 = kvp.Value[0];
                b2 = kvp.Value[1];
                break;
            }
        }

        if (chosenFlyweight == null)
        {
            // Demek ki her blok farklý renk => hiçbir þekilde ikili grup oluþturulamaz
            Debug.LogWarning("No color has >=2 blocks => cannot form any group. Puzzle unsolvable or special case.");
            // Burada istersen "Game Over" diyebilirsin ya da tabloyu yine de random karýþtýr.
            // Tek satýrda "Return" ediyorum.
            return;
        }

        // 4) Seçtiðimiz 2 bloðu tabloda yan yana koy
        //    Örnek: [0,0] ve [0,1]. (Eðer 1 satýr yoksa [0,0] & [1,0] gibi dikey de olur)
        grid[0, 0] = b1;
        b1.row = 0; b1.col = 0;
        b1.transform.localPosition = boardManager.CalculateBlockPosition(0, 0);

        // Yanýna [0,1]
        if (cols > 1)
        {
            grid[0, 1] = b2;
            b2.row = 0; b2.col = 1;
            b2.transform.localPosition = boardManager.CalculateBlockPosition(0, 1);
        }
        else
        {
            // Tek sütun varsa mecbur dikey koyarýz
            grid[1, 0] = b2;
            b2.row = 1; b2.col = 0;
            b2.transform.localPosition = boardManager.CalculateBlockPosition(1, 0);
        }

        // 5) Geri kalan bloklarý karýþtýrýp kalan hücrelere diz
        //    (2 bloðumuzu tabloya yerleþtirdik, geri kalan = allBlocks - {b1,b2})
        allBlocks.Remove(b1);
        allBlocks.Remove(b2);

        ShuffleList(allBlocks); // Rastgele sýraya koy

        int idx = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == null)
                {
                    grid[r, c] = allBlocks[idx];
                    allBlocks[idx].row = r;
                    allBlocks[idx].col = c;

                    Vector2 pos = boardManager.CalculateBlockPosition(r, c);
                    allBlocks[idx].transform.localPosition = pos;
                    idx++;
                }
            }
        }

        Debug.Log("ShuffleToGuaranteeOneGroup complete. At least 1 group formed.");
    }

    // Basit Fisher-Yates karýþtýrma
    private void ShuffleList(List<Block> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}
