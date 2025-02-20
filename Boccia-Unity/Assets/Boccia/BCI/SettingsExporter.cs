using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FanNamespace;
using System.IO;

public class SettingsExporter : MonoBehaviour
{
    private BocciaModel _model;

    [Header("Fan settings")]
    [SerializeField]
    private FanSettings _fineFanSettings;

    [SerializeField]
    private FanSettings _coarseFanSettings;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;
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

        string path = Path.Combine(Application.persistentDataPath, "TrialSettings.json");

        File.WriteAllText(path, json);

        Debug.Log($"Exported coarse fan settings to {path}");
    }
}
