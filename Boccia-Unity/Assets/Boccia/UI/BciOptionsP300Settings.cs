using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;  // To enable LINQ methods like First()

public class BciOptionsP300Settings : MonoBehaviour
{
    // Define Min, Max, and Default values for different fields
    // Training Number of Flashes
    private const int MIN_NUM_FLASHES_TRAIN = 1;
    private const int MAX_NUM_FLASHES_TRAIN = 20;

    // Training Number of Windows
    private const int MIN_NUM_TRAIN_WINDOWS = 1;
    private const int MAX_NUM_TRAIN_WINDOWS = 10;

    // Testing Number of flashes
    private const int MIN_NUM_FLASHES_TEST = 1;
    private const int MAX_NUM_FLASHES_TEST = 20;

    // Previous valid values to revert to in case of invalid input
    private int previousTrainNumFlashes;
    private int previousTrainNumTrainingWindows;
    private int previousTestNumFlashes;

    // UI elements for Training settings
    public TMP_InputField trainNumFlashesInputField;
    public TMP_InputField trainNumTrainingWindowsInputField;
    public TMP_Dropdown trainTargetAnimationDropdown;
    public Toggle trainShamSelectionFeedbackToggle;
    public TMP_Dropdown trainShamSelectionAnimationDropdown;
    public TMP_Text labelTrainShamSelectionAnimationText;  // Text element for the animation dropdown
    public TMP_Dropdown trainStimulusOnDurationDropdown;
    public TMP_Dropdown trainStimulusOffDurationDropdown;
    public TMP_Dropdown trainFlashColourDropdown;

    // UI elements for Testing settings
    public TMP_InputField testNumFlashesInputField;
    public Toggle testTargetSelectionFeedbackToggle;
    public TMP_Dropdown testTargetSelectionAnimationDropdown;
    public TMP_Text labelTestTargetSelectionAnimationText;  // Text label for the animation dropdown
    public TMP_Dropdown testStimulusOnDurationDropdown;
    public TMP_Dropdown testStimulusOffDurationDropdown;
    public TMP_Dropdown testFlashColourDropdown;

    private BocciaModel model;

    // Colour options for Flash Colour
    private static readonly Dictionary<string, Color> colours = new Dictionary<string, Color>
    {
        {"Red", Color.red },
        {"Blue", Color.blue },
        {"Green", Color.green },
        {"Yellow", Color.yellow }
    };

    // List of stimulus durations (used for stimulus on and off durations in training and testing)
    private static readonly List<float> durationOptions = new List<float> { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f }; // Example durations in seconds

    // List of animations (placeholder)
    private List<string> animationOptions = new List<string> { "None", "Bop it", "Twist it" };


    void Start()
    {
        model = BocciaModel.Instance;

        // Subscribe to BCI change events to keep the UI updated
        model.BciChanged += OnBciSettingsChanged;

        // Add listeners to UI elements
        AddListenersToUI();

        // Populate dropdowns for the first time
        PopulateDropdowns();

        InitializeUI();
    }

    void OnEnable()
    {
        // Initialize UI with current model values every time the panel is enabled/active
        InitializeUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from BCI events when this object is destroyed to prevent memory leaks
        model.BciChanged -= OnBciSettingsChanged;
    }

    // This method will be called when the BCI settings are updated in the model
    private void OnBciSettingsChanged()
    {
        InitializeUI();
    }

    // Initialize the UI elements with the current P300 settings
    public void InitializeUI()
    {
        var trainSettings = model.P300Settings.Train;
        var testSettings = model.P300Settings.Test;

        // Store previous valid values
        previousTrainNumFlashes = trainSettings.NumFlashes;
        previousTrainNumTrainingWindows = trainSettings.NumTrainingWindows;
        previousTestNumFlashes = testSettings.NumFlashes;

        // Set training settings UI
        trainNumFlashesInputField.text = trainSettings.NumFlashes.ToString();
        trainNumTrainingWindowsInputField.text = trainSettings.NumTrainingWindows.ToString();
        trainTargetAnimationDropdown.value = (int)trainSettings.TargetAnimation;
        trainShamSelectionFeedbackToggle.isOn = trainSettings.ShamSelectionFeedback;
        trainShamSelectionAnimationDropdown.value = (int)trainSettings.ShamSelectionAnimation;
        trainStimulusOnDurationDropdown.value = GetDurationDropdownIndex(trainSettings.StimulusOnDuration);
        trainStimulusOffDurationDropdown.value = GetDurationDropdownIndex(trainSettings.StimulusOffDuration);
        trainFlashColourDropdown.value = GetColourDropdownIndex(trainSettings.FlashColour);

        // Set testing settings UI
        testNumFlashesInputField.text = testSettings.NumFlashes.ToString();
        testTargetSelectionFeedbackToggle.isOn = testSettings.TargetSelectionFeedback;
        testTargetSelectionAnimationDropdown.value = (int)testSettings.TargetSelectionAnimation;
        testStimulusOnDurationDropdown.value = GetDurationDropdownIndex(testSettings.StimulusOnDuration);
        testStimulusOffDurationDropdown.value = GetDurationDropdownIndex(testSettings.StimulusOffDuration);
        testFlashColourDropdown.value = GetColourDropdownIndex(testSettings.FlashColour);

        // Ensure the animation dropdowns are correctly enabled/disabled based on the feedback toggles
        UpdateShamSelectionAnimationInteractable(trainShamSelectionFeedbackToggle.isOn);
        UpdateTargetSelectionAnimationInteractable(testTargetSelectionFeedbackToggle.isOn);

    }

    // MARK: Populate Dropdowns
    // Catch-all for all dropdowns that need to be populated
    private void PopulateDropdowns()
    {
        PopulateDurationDropdowns();
        PopulateAnimationDropdowns();
        PopulateFlashColourDropdown();
    }

    private void PopulateDurationDropdowns()
    // All stimulus on or off durations, training and testing
    {
        List<string> durationTextOptions = durationOptions.Select(d => d + " s").ToList();

        trainStimulusOnDurationDropdown.ClearOptions();
        trainStimulusOffDurationDropdown.ClearOptions();
        testStimulusOnDurationDropdown.ClearOptions();
        testStimulusOffDurationDropdown.ClearOptions();

        trainStimulusOnDurationDropdown.AddOptions(durationTextOptions);
        trainStimulusOffDurationDropdown.AddOptions(durationTextOptions);
        testStimulusOnDurationDropdown.AddOptions(durationTextOptions);
        testStimulusOffDurationDropdown.AddOptions(durationTextOptions);
    }

    private void PopulateAnimationDropdowns()
    {
        trainTargetAnimationDropdown.ClearOptions();
        trainShamSelectionAnimationDropdown.ClearOptions();
        testTargetSelectionAnimationDropdown.ClearOptions();

        trainTargetAnimationDropdown.AddOptions(animationOptions);
        trainShamSelectionAnimationDropdown.AddOptions(animationOptions);
        testTargetSelectionAnimationDropdown.AddOptions(animationOptions);
    }

    private void PopulateFlashColourDropdown()
    {
        List<string> colourOptions = new List<string>(colours.Keys);

        trainFlashColourDropdown.ClearOptions();
        testFlashColourDropdown.ClearOptions();

        trainFlashColourDropdown.AddOptions(colourOptions);
        testFlashColourDropdown.AddOptions(colourOptions);
    }

    // MARK: Helpers to toggle state of animation dropdowns based on whether feedback is enabled
    // Helper to update the state of Sham Selection Animation dropdown
    private void UpdateShamSelectionAnimationInteractable(bool isOn)
    {
        trainShamSelectionAnimationDropdown.interactable = isOn;
        labelTrainShamSelectionAnimationText.color = isOn ? Color.black : Color.gray;

        if (!isOn)
        {
            // Clear the dropdown's displayed text when disabled
            trainShamSelectionAnimationDropdown.captionText.text = "";
        }
        else
        {
            // Restore the previously selected value when enabled
            trainShamSelectionAnimationDropdown.captionText.text = animationOptions[trainShamSelectionAnimationDropdown.value];
        }
    }

    // Helper to update the state of Target Selection Animation dropdown
    private void UpdateTargetSelectionAnimationInteractable(bool isOn)
    {
        testTargetSelectionAnimationDropdown.interactable = isOn;
        labelTestTargetSelectionAnimationText.color = isOn ? Color.black : Color.gray;

        if (!isOn)
        {
            // Clear the dropdown's displayed text when disabled
            testTargetSelectionAnimationDropdown.captionText.text = "";
        }
        else
        {
            // Restore the previously selected value when enabled
            testTargetSelectionAnimationDropdown.captionText.text = animationOptions[testTargetSelectionAnimationDropdown.value];
        }
    }

    // Helper method to add listeners to all UI elements
    private void AddListenersToUI()
    {
        // Training settings listeners
        trainNumFlashesInputField.onEndEdit.AddListener(OnChangeTrainNumFlashes);
        trainNumTrainingWindowsInputField.onEndEdit.AddListener(OnChangeTrainNumTrainingWindows);
        trainTargetAnimationDropdown.onValueChanged.AddListener(OnChangeTrainTargetAnimation);
        trainShamSelectionFeedbackToggle.onValueChanged.AddListener(OnChangeTrainShamSelectionFeedback);
        trainShamSelectionAnimationDropdown.onValueChanged.AddListener(OnChangeTrainShamSelectionAnimation);
        trainStimulusOnDurationDropdown.onValueChanged.AddListener(OnChangeTrainStimulusOnDuration);
        trainStimulusOffDurationDropdown.onValueChanged.AddListener(OnChangeTrainStimulusOffDuration);
        trainFlashColourDropdown.onValueChanged.AddListener(OnChangeTrainFlashColour);

        // Testing settings listeners
        testNumFlashesInputField.onEndEdit.AddListener(OnChangeTestNumFlashes);
        testTargetSelectionFeedbackToggle.onValueChanged.AddListener(OnChangeTestTargetSelectionFeedback);
        testTargetSelectionAnimationDropdown.onValueChanged.AddListener(OnChangeTestTargetSelectionAnimation);
        testStimulusOnDurationDropdown.onValueChanged.AddListener(OnChangeTestStimulusOnDuration);
        testStimulusOffDurationDropdown.onValueChanged.AddListener(OnChangeTestStimulusOffDuration);
        testFlashColourDropdown.onValueChanged.AddListener(OnChangeTestFlashColour);
    }

    // Helper method to get the dropdown index based on duration
    private int GetDurationDropdownIndex(float duration)
    {
        return durationOptions.IndexOf(duration);  // Find the index of the duration in the list
    }

    // Helper to get the dropdown index for a colour
    private int GetColourDropdownIndex(Color colour)
    {
        foreach (var pair in colours)
        {
            if (pair.Value == colour)
            {
                return trainFlashColourDropdown.options.FindIndex(option => option.text == pair.Key);
            }
        }
        return 0; // Default to the first colour if not found
    }

    // Helper method to convert dropdown index back to Color
    private Color GetColourFromDropdownIndex(int index)
    {
        string selectedColourName = trainFlashColourDropdown.options[index].text;
        if (colours.TryGetValue(selectedColourName, out Color selectedColour))
        {
            return selectedColour;
        }

        return colours.First().Value;  // Return first color (red) if not found
    }

    // MARK: Training Setting Change Handlers

    private void OnChangeTrainNumFlashes(string value)
    {
        if (int.TryParse(value, out int numFlashes))
        {
            // Clamp value within min and max
            numFlashes = Mathf.Clamp(numFlashes, MIN_NUM_FLASHES_TRAIN, MAX_NUM_FLASHES_TRAIN);

            // Update the model and previous value
            model.SetBciOption(ref model.P300Settings.Train.NumFlashes, numFlashes);
            previousTrainNumFlashes = numFlashes;
        }
        else
        {
            // Revert to previous valid value
            trainNumFlashesInputField.text = previousTrainNumFlashes.ToString();
        }
    }

    private void OnChangeTrainNumTrainingWindows(string value)
    {
        if (int.TryParse(value, out int numTrainingWindows))
        {
            // Clamp value within min and max
            numTrainingWindows = Mathf.Clamp(numTrainingWindows, MIN_NUM_TRAIN_WINDOWS, MAX_NUM_TRAIN_WINDOWS);

            // Update the model and previous value
            model.SetBciOption(ref model.P300Settings.Train.NumTrainingWindows, numTrainingWindows);
            previousTrainNumTrainingWindows = numTrainingWindows;
        }
        else
        {
            // Revert to previous valid value
            trainNumTrainingWindowsInputField.text = previousTrainNumTrainingWindows.ToString();
        }
    }

    private void OnChangeTrainTargetAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        model.SetBciOption(ref model.P300Settings.Train.TargetAnimation, selectedAnimation);
    }

    private void OnChangeTrainShamSelectionFeedback(bool isOn)
    {
        model.SetBciOption(ref model.P300Settings.Train.ShamSelectionFeedback, isOn);
        UpdateShamSelectionAnimationInteractable(isOn);
    }

    private void OnChangeTrainShamSelectionAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        model.SetBciOption(ref model.P300Settings.Train.ShamSelectionAnimation, selectedAnimation);
    }

    private void OnChangeTrainStimulusOnDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        model.SetBciOption(ref model.P300Settings.Train.StimulusOnDuration, selectedDuration);
    }

    private void OnChangeTrainStimulusOffDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        model.SetBciOption(ref model.P300Settings.Train.StimulusOffDuration, selectedDuration);
    }

    private void OnChangeTrainFlashColour(int index)
    {
        var selectedColour = GetColourFromDropdownIndex(index);
        model.SetBciOption(ref model.P300Settings.Train.FlashColour, selectedColour);
    }

    // MARK: Testing Setting Change Handlers

    private void OnChangeTestNumFlashes(string value)
    {
        if (int.TryParse(value, out int numFlashes))
        {
            // Clamp value within min and max
            numFlashes = Mathf.Clamp(numFlashes, MIN_NUM_FLASHES_TEST, MAX_NUM_FLASHES_TEST);

            // Update the model and previous value
            model.SetBciOption(ref model.P300Settings.Test.NumFlashes, numFlashes);
            previousTestNumFlashes = numFlashes;
        }
        else
        {
            // Revert to previous valid value
            testNumFlashesInputField.text = previousTestNumFlashes.ToString();
        }
    }

    private void OnChangeTestTargetSelectionFeedback(bool isOn)
    {
        model.SetBciOption(ref model.P300Settings.Test.TargetSelectionFeedback, isOn);
        UpdateTargetSelectionAnimationInteractable(isOn);
    }

    private void OnChangeTestTargetSelectionAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        model.SetBciOption(ref model.P300Settings.Test.TargetSelectionAnimation, selectedAnimation);
    }

    private void OnChangeTestStimulusOnDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        model.SetBciOption(ref model.P300Settings.Test.StimulusOnDuration, selectedDuration);
    }

    private void OnChangeTestStimulusOffDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        model.SetBciOption(ref model.P300Settings.Test.StimulusOffDuration, selectedDuration);
    }

    private void OnChangeTestFlashColour(int index)
    {
        var selectedColour = GetColourFromDropdownIndex(index);
        model.SetBciOption(ref model.P300Settings.Test.FlashColour, selectedColour);
    }
}