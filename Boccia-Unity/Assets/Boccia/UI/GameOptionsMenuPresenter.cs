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

    private static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>
    {
        {"Blue", Color.blue },
        {"Red", Color.red },
        {"Green", Color.green },
        {"Yellow", Color.yellow },
        {"Black", Color.black },
        {"Magenta", Color.magenta},
        {"Grey", Color.grey},
        {"Cyan", Color.cyan}
    };

    void Start()
    {
        model = BocciaModel.Instance;
        PopulateColorDropdown();
        InitializeValues();

        // Connect UI to model
        ballColorDropdown.onValueChanged.AddListener(ChangeBallColor);
        elevationPrecisionSlider.onValueChanged.AddListener(ChangeElevationPrecision);
        elevationRangeSlider.onValueChanged.AddListener(ChangeElevationRange);
        rotationPrecisionSlider.onValueChanged.AddListener(ChangeRotationPrecision);
        rotationRangeSlider.onValueChanged.AddListener(ChangeRotationRange);
        elevationSpeedSlider.onValueChanged.AddListener(ChangeElevationSpeed);
        rotationSpeedSlider.onValueChanged.AddListener(ChangeRotationSpeed);
    }

    private void InitializeValues()
    {
        // Convert the color from the model to the corresponding dropdown value (string)
        ballColorDropdown.value = ballColorDropdown.options.FindIndex(option => option.text == GetColorNameFromModel(model.BallColor));

        // Ball color will not persist if this line is removed
        ChangeBallColor(ballColorDropdown.value);

        // Initialize other variables from BocciaModel
        elevationPrecisionSlider.value = model.ElevationPrecision;
        elevationRangeSlider.value = model.ElevationRange;
        elevationSpeedSlider.value = model.ElevationSpeed;
        rotationPrecisionSlider.value = model.RotationPrecision;
        rotationRangeSlider.value = model.RotationRange;
        rotationSpeedSlider.value = model.RotationSpeed;
    }

    private void PopulateColorDropdown()
    {
        // Clear any existing options
        ballColorDropdown.ClearOptions();

        // Extract color names (keys) from the dictionary
        List<string> colorOptions = new List<string>(colors.Keys);

        // Add color names to the dropdown
        ballColorDropdown.AddOptions(colorOptions);
    }

    public void ChangeBallColor(int valueIndex)
    {
        // Get the selected dropdown option as a string
        string selectedColorName = ballColorDropdown.options[valueIndex].text;

        // Find the corresponding Color from the dictionary and pass it to the model
        if (colors.TryGetValue(selectedColorName, out Color selectedColor))
        {
            model.ChangeBallColor(selectedColor);  // Pass the Color, not the string
        }
        else
        {
            Debug.LogWarning($"Color {selectedColorName} not found in the dictionary.");
        }
    }


    private string GetColorNameFromModel(Color color)
    {
        // Find the color name in the dictionary based on the color value in the model
        foreach (var pair in colors)
        {
            if (pair.Value == color)
            {
                return pair.Key;
            }
        }
        return "Blue"; // Default if no match is found
    }

    public void ChangeElevationPrecision(float precisionPercent)
    {
        model.SetElevationPrecision(precisionPercent);
    }

    public void ChangeElevationRange(float rangePercent)
    {
        model.SetElevationRange(rangePercent);
    }

    public void ChangeRotationPrecision(float precisionPercent)
    {
        model.SetRotationPrecision(precisionPercent);
    }

    public void ChangeRotationRange(float rangePercent)
    {
        model.SetRotationRange(rangePercent);
    }

    public void ChangeElevationSpeed(float elevationSpeed)
    {
        model.SetElevationSpeed(elevationSpeed);
    }

    public void ChangeRotationSpeed(float rotationSpeed)
    {
        model.SetRotationSpeed(rotationSpeed);
    }

    public void NavigatetoStart()
    {
        model.ShowPreviousScreen();
    }
}
