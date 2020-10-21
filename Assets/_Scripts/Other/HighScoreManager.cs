using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    private string path = "";

    #region INITIALIZE HIGH SCORE
    public void InitializeHighScore()
    {
        // Set path
        path = Path.Combine(Application.persistentDataPath, "HighScore.json");

        // If file exists: do nothing
        if (File.Exists(path))
        {
            return;
        }

        // If file doesn't exist: create it
        HighScoreObject highScore = new HighScoreObject()
        {
            score = 0
        };

        // Transform to json
        string newJson = JsonUtility.ToJson(highScore, true);
        // Save file
        File.WriteAllText(path, newJson);
    }
    #endregion

    #region SAVE NEW HIGH SCORE
    public bool SaveNewHighScore(int currentScore)
    {
        // Read high score from file
        string readJson = File.ReadAllText(path);
        HighScoreObject highScore = JsonUtility.FromJson<HighScoreObject>(readJson);

        // If currentScore is lower than the file highScore
        if (currentScore <= highScore.score)
        {
            return false;
        }

        // If currentScore is higher than the file highScore

        // Update highscore
        highScore.score = currentScore;

        // Transform to json
        string newJson = JsonUtility.ToJson(highScore, true);
        // Save file
        File.WriteAllText(path, newJson);

        return true;
    }
    #endregion

    #region GET HIGH SCORE
    public int GetHighScore()
    {
        // Read high score from file
        string readJson = File.ReadAllText(path);
        HighScoreObject highScore = JsonUtility.FromJson<HighScoreObject>(readJson);

        return highScore.score;
    }
    #endregion
}
