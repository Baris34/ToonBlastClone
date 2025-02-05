using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public BoardManager gridManager;
    public LevelData level1Data;
    public LevelData level2Data;

    private void Start()
    {
        LoadLevel(1);
    }

    public void LoadLevel(int levelNumber)
    {
        if (levelNumber == 1)
        {
            gridManager.LoadLevel(level1Data);
        }
        else if (levelNumber == 2)
        {
            gridManager.LoadLevel(level2Data);
        }
    }
}
