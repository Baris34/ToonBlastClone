using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
//Stores the data for a level, including the number of rows, columns, colors, and combo thresholds.
public class LevelData : ScriptableObject
{
    public int rows;
    public int cols;
    public int numColors;
    public int comboThresholdA;
    public int comboThresholdB;
    public int comboThresholdC;
}
