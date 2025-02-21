using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FanNamespace;
using System.IO;

public class SettingsExporter : MonoBehaviour
{
    private BocciaModel _model;

    [Header("Trial Settings")]
    [SerializeField]
    private int _trialNumber;
    private string _settingsDir;

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

        // Initialize trial number at 0
        _trialNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            _trialNumber++;
            ExportSettings();
        }
    }

    private void ExportSettings()
    {
        string currentDateTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string jsonCoarseFan = JsonUtility.ToJson(_coarseFanSettings, true);
        string jsonFineFan = JsonUtility.ToJson(_fineFanSettings, true);
        string jsonP300Settings = JsonUtility.ToJson(_model.P300Settings, true);

        string json = "{\"trialNumber\": " + _trialNumber + 
                    ", \"currentDateTime\": \"" + currentDateTime + "\"" +
                    ", \"coarseFanSettings\": " + jsonCoarseFan + 
                    ", \"fineFanSettings\": " + jsonFineFan + 
                    ", \"P300Settings\": " + jsonP300Settings + "}";

        string filename = $"{currentDateTime}_Trial_{_trialNumber}_Settings.json";
        string path = Path.Combine(_settingsDir, filename);

        File.WriteAllText(path, json);

        Debug.Log($"Exported Trial {_trialNumber} settings to {path}");
    }
}
