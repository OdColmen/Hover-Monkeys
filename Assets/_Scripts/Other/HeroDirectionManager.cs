using System.IO;
using UnityEngine;

public class HeroDirectionManager : MonoBehaviour
{
	private string path = "";

    #region INITIALIZE DIRECTION
    public void InitializeDirection()
    {
        // Set path
        path = Path.Combine(Application.persistentDataPath, "HeroDirection.json");

        // If file exists: do nothing
        if (File.Exists(path))
        {
            return;
        }

        // If file doesn't exist: create it
        HeroDirectionObject heroDirection = new HeroDirectionObject()
        {
            // Start game by going right (positive number)
            directionX = 1
        };

        // Transform to json
        string newJson = JsonUtility.ToJson(heroDirection, true);
        // Save file
        File.WriteAllText(path, newJson);
    }
    #endregion

    #region SAVE NEW DIRECTION
    public bool SaveNewDirection(int currentDirection)
    {
        // Update heroDirection
        HeroDirectionObject heroDirection = new HeroDirectionObject()
        {
            directionX = currentDirection
        };

        // Transform to json
        string newJson = JsonUtility.ToJson(heroDirection, true);
        // Save file
        File.WriteAllText(path, newJson);

        return true;
    }
    #endregion

    #region GET DIRECTION
    public int GetDirection()
    {
        // Read direction from file
        string readJson = File.ReadAllText(path);
        HeroDirectionObject heroDirection = JsonUtility.FromJson<HeroDirectionObject>(readJson);

        return heroDirection.directionX;
    }
    #endregion
}
