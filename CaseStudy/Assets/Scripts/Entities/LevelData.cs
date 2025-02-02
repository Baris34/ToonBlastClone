using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public int rows;
    public int cols;
    public int numColors;
    public int comboThresholdA;
    public int comboThresholdB;
    public int comboThresholdC;
}
