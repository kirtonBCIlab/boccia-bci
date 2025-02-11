using System;
using System.Collections;
using System.Collections.Generic;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
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

    private BocciaModel _model;

    public GameObject bciControllerManager;
    private P300ControllerBehavior p300ControllerBehavior;

    // Colour options for Flash Colour
    private static readonly Dictionary<string, Color> colours = new Dictionary<string, Color>
    {
        {"Red", Color.red },
        {"Blue", Color.blue },
        {"Green", Color.green },
        {"Yellow", Color.yellow }
    };

    // List of stimulus durations (used for stimulus on and off durations in training and testing)
    private static readonly List<float> durationOptions = new List<float> { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f}; // Example durations in milliseconds

    // List of animations
    private List<string> animationOptions = new List<string>(Enum.GetNames(typeof(BocciaAnimation)));

    void Awake()
    {
        _model = BocciaModel.Instance;
        bciControllerManager = GameObject.Find("ControllerManager");
        p300ControllerBehavior = bciControllerManager.GetComponentInChildren<P300ControllerBehavior>();
    }

    void Start()
    {
        // Subscribe to BCI change events to keep the UI updated
        _model.BciChanged += OnBciSettingsChanged;
        _model.NavigationChanged += OnNavigationChanged;

        // Add listeners to UI elements
        AddListenersToUI();

        // Populate dropdowns for the first time
        PopulateDropdowns();

        InitializeUI();
        UpdateP300ControllerTraining(); // Initialize P300ControllerBehavior with training settings
    }

    void OnEnable()
    {
        // This check is to avoid NullReferenceExceptions that happen when OnEnable() attempts to run before the game data that contains the model is loaded
        if (_model == null)
        {
            // Debug.LogError("Model is not initialized yet in OnEnable.");
            return; // Avoid running further code if the model is not ready
        }

        // Initialize UI with current model values every time the panel is enabled/active
        InitializeUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from events when this object is destroyed to prevent memory leaks
        _model.BciChanged -= OnBciSettingsChanged;
        _model.NavigationChanged -= OnNavigationChanged;
    }

    // This method will be called when the BCI settings are updated in the model
    private void OnBciSettingsChanged()
    {
        InitializeUI();
    }

    // Update P300ControllerBehavior depending on the screen
    private void OnNavigationChanged()
    {
        // If we are in Virtual Play or Play screens, enforce testing settings
        if (_model.CurrentScreen == BocciaScreen.VirtualPlay || _model.CurrentScreen == BocciaScreen.Play)
        {
            UpdateP300ControllerTesting();
            return;
        }

        // If we are in Training screen, enforce training settings
        if (_model.CurrentScreen == BocciaScreen.TrainingScreen)
        {
            UpdateP300ControllerTraining();
            return;
        }
    }

    // Initialize the UI elements with the current P300 settings
    public void InitializeUI()
    {
        var trainSettings = _model.P300Settings.Train;
        var testSettings = _model.P300Settings.Test;

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
            _model.SetBciOption(ref _model.P300Settings.Train.NumFlashes, numFlashes);
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
            _model.SetBciOption(ref _model.P300Settings.Train.NumTrainingWindows, numTrainingWindows);
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
        _model.SetBciOption(ref _model.P300Settings.Train.TargetAnimation, selectedAnimation);
    }

    private void OnChangeTrainShamSelectionFeedback(bool isOn)
    {
        _model.SetBciOption(ref _model.P300Settings.Train.ShamSelectionFeedback, isOn);
        UpdateShamSelectionAnimationInteractable(isOn);
    }

    private void OnChangeTrainShamSelectionAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        _model.SetBciOption(ref _model.P300Settings.Train.ShamSelectionAnimation, selectedAnimation);
    }

    private void OnChangeTrainStimulusOnDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Train.StimulusOnDuration, selectedDuration);
    }

    private void OnChangeTrainStimulusOffDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Train.StimulusOffDuration, selectedDuration);
    }

    private void OnChangeTrainFlashColour(int index)
    {
        var selectedColour = GetColourFromDropdownIndex(index);
        _model.SetBciOption(ref _model.P300Settings.Train.FlashColour, selectedColour);
    }

    // MARK: Testing Setting Change Handlers

    private void OnChangeTestNumFlashes(string value)
    {
        if (int.TryParse(value, out int numFlashes))
        {
            // Clamp value within min and max
            numFlashes = Mathf.Clamp(numFlashes, MIN_NUM_FLASHES_TEST, MAX_NUM_FLASHES_TEST);

            // Update the model and previous value
            _model.SetBciOption(ref _model.P300Settings.Test.NumFlashes, numFlashes);
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
        _model.SetBciOption(ref _model.P300Settings.Test.TargetSelectionFeedback, isOn);
        UpdateTargetSelectionAnimationInteractable(isOn);
    }

    private void OnChangeTestTargetSelectionAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        _model.SetBciOption(ref _model.P300Settings.Test.TargetSelectionAnimation, selectedAnimation);
    }

    private void OnChangeTestStimulusOnDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Test.StimulusOnDuration, selectedDuration);
    }

    private void OnChangeTestStimulusOffDuration(int index)
    {
        var selectedDuration = durationOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Test.StimulusOffDuration, selectedDuration);
    }

    private void OnChangeTestFlashColour(int index)
    {
        var selectedColour = GetColourFromDropdownIndex(index);
        _model.SetBciOption(ref _model.P300Settings.Test.FlashColour, selectedColour);
    }

    // MARK: Update P300 Controller
    // Set the relevant P300ControllerBehavior values from the current P300 TRAINING settings in the model
    private void UpdateP300ControllerTraining()
    {
        // Debug.Log("Update P300ControllerBehavior for TRAINING");

        // Number of flashes
        p300ControllerBehavior.numFlashesLowerLimit = _model.P300Settings.Train.NumFlashes;
        p300ControllerBehavior.numFlashesUpperLimit = _model.P300Settings.Train.NumFlashes;

        // Number of training windows
        p300ControllerBehavior.numTrainWindows = _model.P300Settings.Train.NumTrainingWindows;

        // Sham selection feedback
        p300ControllerBehavior.shamFeedback = _model.P300Settings.Train.ShamSelectionFeedback;

        // Stimulus on and off durations
        p300ControllerBehavior.onTime = _model.P300Settings.Train.StimulusOnDuration;
        p300ControllerBehavior.offTime = _model.P300Settings.Train.StimulusOffDuration;
    }

    // Set the relevant P300ControllerBehavior values from the current P300 TESTING settings in the model
    private void UpdateP300ControllerTesting()
    {
        // Debug.Log("Update P300ControllerBehavior for TESTING");

        // Number of flashes
        p300ControllerBehavior.numFlashesLowerLimit = _model.P300Settings.Test.NumFlashes;
        p300ControllerBehavior.numFlashesUpperLimit = _model.P300Settings.Test.NumFlashes;

        // Sham selection feedback
        p300ControllerBehavior.shamFeedback = false;

        // Stimulus on and off durations
        p300ControllerBehavior.onTime = _model.P300Settings.Test.StimulusOnDuration;
        p300ControllerBehavior.offTime = _model.P300Settings.Test.StimulusOffDuration;
    }
}