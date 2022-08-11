using System;
using System.IO;
using UnityEngine;

public static class FileManager
{
    public static bool WriteToFile(string fileName, string fileContents)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(fullPath, fileContents);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
        }

        return false;
    }

    public static bool ReadFromFile(string fileName, out string result)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log(fullPath);

        try
        {
            result = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            result = "";
        }

        return false;
    }
}
