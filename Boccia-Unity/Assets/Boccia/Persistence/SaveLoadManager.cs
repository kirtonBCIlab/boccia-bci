using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEditor.UI;
using UnityEngine;

// Example persistence manager that just loads the model data at start
// and saves it when the application quits.  The load/save methods could
// be attached to buttons, etc.
public class SaveLoadManager: MonoBehaviour
{
    private BocciaData bocciaData;
    private LocalRepository repository;


    public void Awake()
    {
        repository = new LocalRepository();
        LoadGame();
    }

    public void OnApplicationQuit()
    {
        SaveGame();
    }


    public void LoadGame()
    {
        if (repository.Load(out var json) && json.Length > 0)
        {
            bocciaData = JsonUtility.FromJson<BocciaData>(json);
            Debug.Log("SaveLoadManager: Loaded game data");
        }
        else
        {
            bocciaData = new BocciaData();
            Debug.Log("SaveLoadManager: Initializing game data");
        }

        // share new GameData with model, keep reference for saving
        BocciaModel.Instance?.Bind(bocciaData);
    }

    public void SaveGame()
    {
        repository.Store(JsonUtility.ToJson(bocciaData));
        Debug.Log("SaveLoadManager: Saved game data");
    }

}
