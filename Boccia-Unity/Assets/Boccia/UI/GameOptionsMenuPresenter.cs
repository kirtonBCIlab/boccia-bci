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
    private BocciaModel model;
    public Button doneButton;
    public Button resetDefaultsButton;

    void Start()
    {
        model = BocciaModel.Instance;
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
        // This check is to avoid NullReferenceExceptions that happen when OnEnable() attempts to run before the game data that contains the model is loaded
        if (model == null)
        {
            // Debug.LogError("Model is not initialized yet in OnEnable.");
            return; // Avoid running further code if the model is not ready
        }

        PopulateColorDropdown();
        InitializeValues();
    }

    private void InitializeValues()
    {
        // Ball Color
        // Ensure the dropdown reflects the current ball color from the model
        ballColorDropdown.value = ballColorDropdown.options.FindIndex(option => option.text == GetColorName(model.GetCurrentBallColor()));


        // Initialize other variables from BocciaModel
        elevationPrecisionSlider.value = model.GameOptions.ElevationPrecision;
        elevationRangeSlider.value = model.GameOptions.ElevationRange;
        elevationSpeedSlider.value = model.GameOptions.ElevationSpeed;
        rotationPrecisionSlider.value = model.GameOptions.RotationPrecision;
        rotationRangeSlider.value = model.GameOptions.RotationRange;
        rotationSpeedSlider.value = model.GameOptions.RotationSpeed;
    }

    private void PopulateColorDropdown()
    {
        // Clear any existing options
        ballColorDropdown.ClearOptions();

        // Extract color names (keys) from the BallColorOptionsDict dictionary
        List<string> colorOptions = new List<string>(model.GameOptions.BallColorOptionsDict.Keys);

        // Add color names to the dropdown
        ballColorDropdown.AddOptions(colorOptions);
    }

    // Helper method to get the color name from the dictionary, given a Color
    private string GetColorName(Color color)
    {
        foreach (var pair in model.BallColorOptionsDict)
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
        if (model.BallColorOptionsDict.TryGetValue(selectedColorName, out Color selectedColor))
        {
            model.SetBallColor(selectedColor);
        }
    }

    public void OnChangeElevationPrecision(float value)
    {
        model.SetGameOption(ref model.GameOptions.ElevationPrecision, value);
    }

    public void OnChangeElevationRange(float value)
    {
        model.SetGameOption(ref model.GameOptions.ElevationRange, value);
    }

    public void OnChangeRotationPrecision(float value)
    {
        model.SetGameOption(ref model.GameOptions.RotationPrecision, value);
    }

    public void OnChangeRotationRange(float value)
    {
        model.SetGameOption(ref model.GameOptions.RotationRange, value);
    }

    public void OnChangeElevationSpeed(float value)
    {
        model.SetGameOption(ref model.GameOptions.ElevationSpeed, value);
    }

    public void OnChangeRotationSpeed(float value)
    {
        model.SetGameOption(ref model.GameOptions.RotationSpeed, value);
    }

    // Reset game options to defaults
    public void OnResetDefaultsClicked()
    {
        model.ResetGameOptionsToDefaults();
        InitializeValues();
    }

    public void OnDoneButtonClicked()
    {
        model.ShowPreviousScreen();
    }
}
