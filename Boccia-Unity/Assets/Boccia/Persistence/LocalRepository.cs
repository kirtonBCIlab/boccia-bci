using System;
using System.IO;
using UnityEngine;

/// <summary>
/// LocalRepository is responsible for reading/writing user data to the persistent
/// data path.  This data will persist between application installations, etc.
/// </summary>
public class LocalRepository
{
    readonly string dataFileName = "UserData.dat";

    public string GetPathToDataFile()
    {
        return Path.Combine(Application.persistentDataPath, dataFileName);
    }

    public bool Store(string json)
    {
        bool success = false;
        var fullPath = GetPathToDataFile();
        try
        {
            File.WriteAllText(fullPath, json);
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"LocalRepository: Failed to write to {fullPath} with exception {e}");
        }
        return success;
    }

    public bool Load(out string json)
    {
        json = "";
        bool success = false;
        var fullPath = GetPathToDataFile();
        try
        {
            if (File.Exists(fullPath))
            {
                json = File.ReadAllText(fullPath);
                success = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"LocalRepository: Failed to read from {fullPath} with exception {e}");
        }
        return success;
    }
}


