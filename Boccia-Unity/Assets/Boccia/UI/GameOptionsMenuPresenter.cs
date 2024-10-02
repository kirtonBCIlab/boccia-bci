using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This is an example UI presenter.  This could be broken into several smaller scripts
// that each reference their own local UI element.  Typically there's interaction between
// UI elements, so can be more convenient to keep the logic in a single presenter.
public class GameOptionsMenuPresenter : MonoBehaviour
{
    public TMPro.TMP_Dropdown ballColorDropdown;
    public Slider elevationPrecisionSlider;
    public Slider elevationRangeSlider;
    public Slider rotationPrecisionSlider;
    public Slider rotationRangeSlider;
    public Slider elevationSpeedSlider;
    public Slider rotationSpeedSlider;
    

    private BocciaModel model;

    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;

        InitializeValues();

        // connect UI to model
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
        // Convert the Color from the model to a corresponding dropdown value
        int colorIndex = GetColorIndexFromModel(model.BallColor);
        ballColorDropdown.value = colorIndex;

        // Initialize other variables from BocciaModel
        elevationPrecisionSlider.value = model.ElevationPrecision;
        elevationRangeSlider.value = model.ElevationRange;
        elevationSpeedSlider.value = model.ElevationSpeed;
        rotationPrecisionSlider.value = model.RotationPrecision;
        rotationRangeSlider.value = model.RotationRange;
        rotationSpeedSlider.value = model.RotationSpeed;
    }

    public void ChangeBallColor(int valueIndex)
    {
        //get the selected dropdown option
        string selectedValue = ballColorDropdown.options[valueIndex].text;
        
        //send the selected color to the model
        model.ChangeBallColor(selectedValue);
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

    private int GetColorIndexFromModel(Color color)
    {
        if (color == Color.blue)
        {
            return 0; // Blue option in the dropdown
        }
        else if (color == Color.red)
        {
            return 1; // Red option in the dropdown
        }
        else if (color == Color.green)
        {
            return 2; // Green option in the dropdown
        }
        else
        {
            return 0; // Default to blue if not recognized
        }
    }

}
