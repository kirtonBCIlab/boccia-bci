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
        _settingsDir = @"C:\Users\Daniella\Documents\BocciaTrials";

        if (!Directory.Exists(_settingsDir))
        {
            Directory.CreateDirectory(_settingsDir);
        }

        // Initialize trial number at 0
        _trialNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ExportSettings();
        }
    }

    private void ExportSettings()
    {
        string jsonCoarseFan = JsonUtility.ToJson(_coarseFanSettings, true);
        string jsonFineFan = JsonUtility.ToJson(_fineFanSettings, true);
        string jsonP300Settings = JsonUtility.ToJson(_model.P300Settings, true);

        string json = "{\"coarseFanSettings\": " + jsonCoarseFan + ", \"fineFanSettings\": " + jsonFineFan + ", \"P300Settings\": " + jsonP300Settings + "}";

        string filename = $"TrialSettings_{_trialNumber}.json";
        string path = Path.Combine(_settingsDir, filename);

        File.WriteAllText(path, json);

        Debug.Log($"Exported trial {_trialNumber} settings to {path}");
    }
}
