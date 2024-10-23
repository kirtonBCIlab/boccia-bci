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

        // Connect UI to model
        // ballColorDropdown.onValueChanged.AddListener(ChangeBallColor);
        // elevationPrecisionSlider.onValueChanged.AddListener(ChangeElevationPrecision);
        // elevationRangeSlider.onValueChanged.AddListener(ChangeElevationRange);
        // rotationPrecisionSlider.onValueChanged.AddListener(ChangeRotationPrecision);
        // rotationRangeSlider.onValueChanged.AddListener(ChangeRotationRange);
        // elevationSpeedSlider.onValueChanged.AddListener(ChangeElevationSpeed);
        // rotationSpeedSlider.onValueChanged.AddListener(ChangeRotationSpeed);

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
        // // Convert the color from the model to the corresponding dropdown value (string)
        // ballColorDropdown.value = ballColorDropdown.options.FindIndex(option => option.text == GetColorNameFromModel(model.GameOptions.BallColor));

        // // Ball color will not persist if this line is removed
        // OnChangeBallColor(ballColorDropdown.value);

        // Ball Color
        // Ensure the dropdown reflects the current ball color from the model
        ballColorDropdown.value = ballColorDropdown.options.FindIndex(option => option.text == GetColorName(model.GetCurrentBallColor()));


        // Initialize other variables from BocciaModel
        // elevationPrecisionSlider.value = model.ElevationPrecision;
        // elevationRangeSlider.value = model.ElevationRange;
        // elevationSpeedSlider.value = model.ElevationSpeed;
        // rotationPrecisionSlider.value = model.RotationPrecision;
        // rotationRangeSlider.value = model.RotationRange;
        // rotationSpeedSlider.value = model.RotationSpeed;
        elevationPrecisionSlider.value = model.GameOptions.ElevationPrecision;
        elevationRangeSlider.value = model.GameOptions.ElevationRange;
        elevationSpeedSlider.value = model.GameOptions.ElevationSpeed;
        rotationPrecisionSlider.value = model.GameOptions.RotationPrecision;
        rotationRangeSlider.value = model.GameOptions.RotationRange;
        rotationSpeedSlider.value = model.GameOptions.RotationSpeed;
    }

    // private void PopulateColorDropdown()
    // {
    //     // Clear any existing options
    //     ballColorDropdown.ClearOptions();

    //     // Extract color names (keys) from the dictionary
    //     List<string> colorOptions = new List<string>(colors.Keys);

    //     // Add color names to the dropdown
    //     ballColorDropdown.AddOptions(colorOptions);
    // }

    // // Populate the dropdown with color names
    // private void PopulateColorDropdown()
    // {
    //     // Clear any existing options
    //     ballColorDropdown.ClearOptions();

    //     // Extract color names (keys) from the BallColorOptionsDict dictionary
    //     List<string> colorOptions = new List<string>(model.GameOptions.BallColorOptionsDict.Keys);
    //     ballColorDropdown.AddOptions(colorOptions);
    // }

    private void PopulateColorDropdown()
    {
        // Clear any existing options
        ballColorDropdown.ClearOptions();

        // if (model.GameOptions.BallColorOptionsDict.Count == 0)
        // {
        //     Debug.LogError("BallColorOptionsDict is empty! Make sure InitializeBallColorOptions() is called.");
        //     return; // Early return if the dictionary is empty
        // }

        
        // Extract color names (keys) from the BallColorOptionsDict dictionary
        List<string> colorOptions = new List<string>(model.GameOptions.BallColorOptionsDict.Keys);

        // // Debug log to print the number of color options available
        // Debug.Log($"Number of color options available: {colorOptions.Count}");

        // // Print each color option to the console for verification
        // foreach (string color in colorOptions)
        // {
        //     Debug.Log($"Color option: {color}");
        // }

        // Add color names to the dropdown
        ballColorDropdown.AddOptions(colorOptions);

        // // Debug log to confirm options are added to the dropdown
        // if (ballColorDropdown.options.Count > 0)
        // {
        //     Debug.Log("Dropdown options successfully populated.");
        // }
        // else
        // {
        //     Debug.LogError("Dropdown options failed to populate.");
        // }
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

    // public void ChangeBallColor(int valueIndex)
    // {
    //     // Get the selected dropdown option as a string
    //     string selectedColorName = ballColorDropdown.options[valueIndex].text;

    //     // Find the corresponding Color from the dictionary and pass it to the model
    //     if (colors.TryGetValue(selectedColorName, out Color selectedColor))
    //     {
    //         model.ChangeBallColor(selectedColor);  // Pass the Color, not the string
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Color {selectedColorName} not found in the dictionary.");
    //     }
    // }

    // private string GetColorNameFromModel(Color color)
    // {
    //     // Find the color name in the dictionary based on the color value in the model
    //     foreach (var pair in colors)
    //     {
    //         if (pair.Value == color)
    //         {
    //             return pair.Key;
    //         }
    //     }
    //     return "Blue"; // Default if no match is found
    // }

    // public void ChangeElevationPrecision(float precisionPercent)
    // {
    //     model.SetElevationPrecision(precisionPercent);
    // }

    // public void ChangeElevationRange(float rangePercent)
    // {
    //     model.SetElevationRange(rangePercent);
    // }

    // public void ChangeRotationPrecision(float precisionPercent)
    // {
    //     model.SetRotationPrecision(precisionPercent);
    // }

    // public void ChangeRotationRange(float rangePercent)
    // {
    //     model.SetRotationRange(rangePercent);
    // }

    // public void ChangeElevationSpeed(float elevationSpeed)
    // {
    //     model.SetElevationSpeed(elevationSpeed);
    // }

    // public void ChangeRotationSpeed(float rotationSpeed)
    // {
    //     model.SetRotationSpeed(rotationSpeed);
    // }

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
    // public void OnChangeBallColor(int valueIndex)
    // {
    //     string selectedColorName = ballColorDropdown.options[valueIndex].text;
    //     if (colors.TryGetValue(selectedColorName, out Color selectedColor))
    //     {
    //         model.SetGameOption(ref model.GameOptions.BallColor, selectedColor);
    //     }
    // }

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
