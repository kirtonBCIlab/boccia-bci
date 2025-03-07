using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FanNamespace;
using System.IO;
using System.Text.RegularExpressions;

public class SettingsExporter : MonoBehaviour
{
    private BocciaModel _model;

    [Header("Trial Settings Directory")]
    [SerializeField]
    [Tooltip("Will automatically be set to Documents folder")]
    private string _settingsDir;
    private int _trialNumber;

    [Header("Fan settings")]
    [SerializeField]
    private FanSettings _fineFanSettings;

    [SerializeField]
    private FanSettings _coarseFanSettings;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;

        // Set directory for saving trial settings
        _settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ExportSettings();
        }
    }

    private void ExportSettings()
    {
        _trialNumber = GetTrialNumber();

        // Get current date and time
        string currentDateTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // Convert fan settings and P300 settings to JSON
        string jsonCoarseFan = JsonUtility.ToJson(_coarseFanSettings, true);
        string jsonFineFan = JsonUtility.ToJson(_fineFanSettings, true);
        string jsonP300Settings = JsonUtility.ToJson(_model.P300Settings, true);

        // Create JSON string
        string json = "{\"trialNumber\": " + _trialNumber + 
                    ", \"currentDateTime\": \"" + currentDateTime + "\"" +
                    ", \"coarseFanSettings\": " + jsonCoarseFan + 
                    ", \"fineFanSettings\": " + jsonFineFan + 
                    ", \"P300Settings\": " + jsonP300Settings + "}";

        // Write JSON string to file
        string filename = $"{currentDateTime}_Trial_{_trialNumber}_Settings.json";
        string path = Path.Combine(_settingsDir, filename);

        File.WriteAllText(path, json);

        Debug.Log($"Exported Trial {_trialNumber} settings to {path}");
    }

    private int GetTrialNumber()
    {
        if (!Directory.Exists(_settingsDir))
        {
            return 1; // If the directory doesn't exist, start at 1
        }

        string[] files = Directory.GetFiles(_settingsDir, "*_Trial_*_Settings.json");

        int maxTrialNumber = 0;
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string nameFormat = @"_Trial_(\d+)_Settings";
            Match match = Regex.Match(fileName, nameFormat);
            
            if (match.Success && int.TryParse(match.Groups[1].Value, out int trialNumber))
            {
                maxTrialNumber = Mathf.Max(maxTrialNumber, trialNumber);
            }
        }

        return maxTrialNumber + 1;
    }
}
