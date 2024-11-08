using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOptionsMenuPresenter : MonoBehaviour
{
    public TMP_Dropdown ballColorDropdown;
    public Slider elevationPrecisionSlider;
    public Slider elevationRangeSlider;
    public Slider rotationPrecisionSlider;
    public Slider rotationRangeSlider;
    public Slider elevationSpeedSlider;
    public Slider rotationSpeedSlider;

    public GameObject rampControlFan;

    private BocciaModel _model;
    public Button doneButton;
    public Button resetDefaultsButton;

    void Start()
    {
        _model = BocciaModel.Instance;
        PopulateColorDropdown();
        InitializeValues();

        // Add listeners for Game Options changes
        // User options
        ballColorDropdown.onValueChanged.AddListener(OnChangeBallColor);
        elevationPrecisionSlider.onValueChanged.AddListener(OnChangeElevationPrecision);
        elevationRangeSlider.onValueChanged.AddListener(OnChangeElevationRange);
        rotationPrecisionSlider.onValueChanged.AddListener(OnChangeRotationPrecision);
        rotationRangeSlider.onValueChanged.AddListener(OnChangeRotationRange);
        // Operator options
        elevationSpeedSlider.onValueChanged.AddListener(OnChangeElevationSpeed);
        rotationSpeedSlider.onValueChanged.AddListener(OnChangeRotationSpeed);

        // Add listeners for Reset and Done buttons
        resetDefaultsButton.onClick.AddListener(OnResetDefaultsClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);
    }

    void OnEnable()
    {
        // This check is to avoid NullReferenceExceptions that happen when OnEnable() attempts to run before the game data that contains the _model is loaded
        if (_model == null)
        {
            // Debug.LogError("Model is not initialized yet in OnEnable.");
            return; // Avoid running further code if the _model is not ready
        }

        PopulateColorDropdown();
        InitializeValues();
    }

    private void InitializeValues()
    {
        // Ball Color
        // Ensure the dropdown reflects the current ball color from the _model
        ballColorDropdown.value = ballColorDropdown.options.FindIndex(option => option.text == GetColorName(_model.GetCurrentBallColor()));


        // Initialize other variables from BocciaModel
        elevationPrecisionSlider.value = _model.GameOptions.ElevationPrecision;
        elevationRangeSlider.value = _model.GameOptions.ElevationRange;
        elevationSpeedSlider.value = _model.GameOptions.ElevationSpeed;
        rotationPrecisionSlider.value = _model.GameOptions.RotationPrecision;
        rotationRangeSlider.value = _model.GameOptions.RotationRange;
        rotationSpeedSlider.value = _model.GameOptions.RotationSpeed;
    }

    private void PopulateColorDropdown()
    {
        // Clear any existing options
        ballColorDropdown.ClearOptions();

        // Extract color names (keys) from the BallColorOptionsDict dictionary
        List<string> colorOptions = new List<string>(_model.GameOptions.BallColorOptionsDict.Keys);

        // Add color names to the dropdown
        ballColorDropdown.AddOptions(colorOptions);
    }

    // Helper method to get the color name from the dictionary, given a Color
    private string GetColorName(Color color)
    {
        foreach (var pair in _model.BallColorOptionsDict)
        {
            if (pair.Value.Equals(color))
            {
                return pair.Key;
            }
        }
        return "Red";  // Default to Blue if not found
    }

    // MARK: Event handlers for changes to game options
    
    // Event handler for when the user changes the ball color in the dropdown
    public void OnChangeBallColor(int valueIndex)
    {
        string selectedColorName = ballColorDropdown.options[valueIndex].text;
        if (_model.BallColorOptionsDict.TryGetValue(selectedColorName, out Color selectedColor))
        {
            _model.SetBallColor(selectedColor);
        }
    }

    public void OnChangeElevationPrecision(float value)
    {
        _model.SetGameOption(ref _model.GameOptions.ElevationPrecision, Mathf.RoundToInt(value));
        GenerateFanForOptions();
    }

    public void OnChangeElevationRange(float value)
    {
        _model.SetGameOption(ref _model.GameOptions.ElevationRange, Mathf.RoundToInt(value));
        GenerateFanForOptions();
    }

    public void OnChangeRotationPrecision(float value)
    {
        _model.SetGameOption(ref _model.GameOptions.RotationPrecision, Mathf.RoundToInt(value));
        GenerateFanForOptions();
    }

    public void OnChangeRotationRange(float value)
    {
        _model.SetGameOption(ref _model.GameOptions.RotationRange, Mathf.RoundToInt(value));
        GenerateFanForOptions();
    }

    public void OnChangeElevationSpeed(float value)
    {
        _model.SetGameOption(ref _model.GameOptions.ElevationSpeed, Mathf.RoundToInt(value));
    }

    public void OnChangeRotationSpeed(float value)
    {
        _model.SetGameOption(ref _model.GameOptions.RotationSpeed, Mathf.RoundToInt(value));
    }

    // Reset game options to defaults
    public void OnResetDefaultsClicked()
    {
        _model.ResetGameOptionsToDefaults();
        InitializeValues();
    }

    public void OnDoneButtonClicked()
    {
        _model.ShowPreviousScreen();
    }

   // Method to Generate the fine fan again whenever the options are updated
    private void GenerateFanForOptions()
    {
        // Debug.Log("Running GenerateFanForOptions() method");
        if (rampControlFan != null && rampControlFan.activeInHierarchy)
        {
            FanPresenter fanPresenter = rampControlFan.GetComponent<FanPresenter>();
            if (fanPresenter != null)
            {
                // Debug.Log("Running fanPresenter.GenerateFanWorkflow()");
                fanPresenter.GenerateFanWorkflow();
            }
            else
            {
                Debug.LogError("FanPresenter component is missing on RampControlFan.");
            }
        }
        else
        {
            Debug.LogWarning("RampControlFan is inactive or null when trying to generate fan.");
        }
    }
}
