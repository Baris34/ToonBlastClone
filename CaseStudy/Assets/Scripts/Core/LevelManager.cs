using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private string[] sceneNames =
    {
        "MainMenu",
        "Level1", 
        "Level2",   
        "Level3",   
        "Level4",   
        "Level5",   
        "Level6"    
    };

    public void LoadLevel(int levelNumber)
    {
        if (levelNumber >= 0 && levelNumber < sceneNames.Length)
        {
            SceneManager.LoadScene(sceneNames[levelNumber]);
        }
        else
        {
            Debug.LogWarning("Invalid level number: " + levelNumber);
        }
    }
}
