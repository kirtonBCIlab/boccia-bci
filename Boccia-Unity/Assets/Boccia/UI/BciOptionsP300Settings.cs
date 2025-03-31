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

    // Training Number of Selections
    private const int MIN_NUM_TRAIN_SELECTIONS = 1;
    private const int MAX_NUM_TRAIN_SELECTIONS = 10;

    // Testing Number of flashes
    private const int MIN_NUM_FLASHES_TEST = 1;
    private const int MAX_NUM_FLASHES_TEST = 20;

    // Previous valid values to revert to in case of invalid input
    private int previousTrainNumFlashes;
    private int previousTrainNumTrainingSelections;
    private int previousTestNumFlashes;

    // UI elements for Training settings
    public TMP_InputField trainNumFlashesInputField;
    public TMP_InputField trainNumTrainingSelectionsInputField;
    public TMP_Dropdown trainTargetAnimationDropdown;
    public Toggle trainShamSelectionFeedbackToggle;
    public TMP_Dropdown trainShamSelectionAnimationDropdown;
    public TMP_Text labelTrainShamSelectionAnimationText;  // Text element for the animation dropdown
    public TMP_Dropdown trainStimulusOnDurationDropdown;
    public TMP_Dropdown trainStimulusOffDurationDropdown;
    public TMP_Dropdown trainStimulusTypeDropdown;
    public GameObject trainFlashColourSetting;
    public TMP_Dropdown trainFlashColourDropdown;

    // UI elements for Testing settings
    public TMP_InputField testNumFlashesInputField;
    public Toggle testTargetSelectionFeedbackToggle;
    public TMP_Dropdown testTargetSelectionAnimationDropdown;
    public TMP_Text labelTestTargetSelectionAnimationText;  // Text label for the animation dropdown
    public TMP_Dropdown testStimulusOnDurationDropdown;
    public TMP_Dropdown testStimulusOffDurationDropdown;
    public TMP_Dropdown testStimulusTypeDropdown;
    public GameObject testFlashColourSetting;
    public TMP_Dropdown testFlashColourDropdown;
    public Toggle separateButtonsToggle;

    private BocciaModel _model;

    public GameObject bciControllerManager;
    private CustomP300ControllerBehavior p300ControllerBehavior;

    // Colour options for Flash Colour
    private static readonly Dictionary<string, Color> colours = new Dictionary<string, Color>
    {
        {"Red", Color.red },
        {"Blue", Color.blue },
        {"Green", Color.green },
        {"Yellow", Color.yellow }
    };

    // List of stimulus durations (used for stimulus on and off durations in training and testing)
    private static readonly List<float> stimulusOnOptions = new List<float>(); // Durations in seconds
    private static readonly List<float> stimulusOffOptions = new List<float>(); // Durations in seconds
    private const float STIMULUS_ON_MIN = 0.05f; // Minimum duration in seconds
    private const float STIMULUS_ON_MAX = 0.2f; // Maximum duration in seconds
    private const float STIMULUS_OFF_MIN = 0.05f; // Minimum duration in seconds
    private const float STIMULUS_OFF_MAX = 0.5f; // Maximum duration in seconds
    private const float STIMULUS_DURATION_STEP = 0.025f; // Step size for duration values

    // List of animations
    private List<string> animationOptions = new List<string>(Enum.GetNames(typeof(BocciaAnimation)));

    // List of stimulus types
    private List<string> stimulusTypeOptions = new List<string>(Enum.GetNames(typeof(BocciaStimulusType)));

    void Awake()
    {
        _model = BocciaModel.Instance;
        bciControllerManager = GameObject.Find("ControllerManager");
        p300ControllerBehavior = bciControllerManager.GetComponentInChildren<CustomP300ControllerBehavior>();
    }

    void Start()
    {
        // Subscribe to BCI change events to keep the UI updated
        _model.BciChanged += OnBciSettingsChanged;
        _model.NavigationChanged += OnNavigationChanged;

        // Add listeners to UI elements
        AddListenersToUI();

        // Initialize the stimulus duration lists
        InitializeStimulusDurations();

        // Populate dropdowns for the first time
        PopulateDropdowns();

        InitializeUI(); // Initialize UI with current model values
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

    private void InitializeStimulusDurations()
    {
        // Initialize the stimulus duration lists
        for (float i = STIMULUS_ON_MIN; i <= (STIMULUS_ON_MAX + STIMULUS_DURATION_STEP); i += STIMULUS_DURATION_STEP)
        {
            stimulusOnOptions.Add((float)Math.Round(i, 3));
        }

        for (float i = STIMULUS_OFF_MIN; i <= (STIMULUS_OFF_MAX + STIMULUS_DURATION_STEP); i += STIMULUS_DURATION_STEP)
        {
            stimulusOffOptions.Add((float)Math.Round(i, 3));
        }
    }

    // Initialize the UI elements with the current P300 settings
    public void InitializeUI()
    {
        var trainSettings = _model.P300Settings.Train;
        var testSettings = _model.P300Settings.Test;

        // Store previous valid values
        previousTrainNumFlashes = trainSettings.NumFlashes;
        previousTrainNumTrainingSelections = trainSettings.NumTrainingSelections;
        previousTestNumFlashes = testSettings.NumFlashes;

        // Set training settings UI
        trainNumFlashesInputField.text = trainSettings.NumFlashes.ToString();
        trainNumTrainingSelectionsInputField.text = trainSettings.NumTrainingSelections.ToString();
        trainTargetAnimationDropdown.value = (int)trainSettings.TargetAnimation;
        trainShamSelectionFeedbackToggle.isOn = trainSettings.ShamSelectionFeedback;
        trainShamSelectionAnimationDropdown.value = (int)trainSettings.ShamSelectionAnimation;
        trainStimulusOnDurationDropdown.value = GetOnDurationDropdownIndex(trainSettings.StimulusOnDuration);
        trainStimulusOffDurationDropdown.value = GetOffDurationDropdownIndex(trainSettings.StimulusOffDuration);
        trainStimulusTypeDropdown.value = (int)trainSettings.StimulusType;
        trainFlashColourDropdown.value = GetColourDropdownIndex(trainSettings.FlashColour);

        // Set testing settings UI
        testNumFlashesInputField.text = testSettings.NumFlashes.ToString();
        testTargetSelectionFeedbackToggle.isOn = testSettings.TargetSelectionFeedback;
        testTargetSelectionAnimationDropdown.value = (int)testSettings.TargetSelectionAnimation;
        testStimulusOnDurationDropdown.value = GetOnDurationDropdownIndex(testSettings.StimulusOnDuration);
        testStimulusOffDurationDropdown.value = GetOffDurationDropdownIndex(testSettings.StimulusOffDuration);
        testStimulusTypeDropdown.value = (int)testSettings.StimulusType;
        testFlashColourDropdown.value = GetColourDropdownIndex(testSettings.FlashColour);
        
        separateButtonsToggle.isOn = _model.P300Settings.SeparateButtons;

        // Ensure the animation dropdowns are correctly enabled/disabled based on the feedback toggles
        UpdateShamSelectionAnimationInteractable(trainShamSelectionFeedbackToggle.isOn);
        UpdateTargetSelectionAnimationInteractable(testTargetSelectionFeedbackToggle.isOn);

        UpdateTrainStimulusSettingsDropdown(trainSettings.StimulusType);
        UpdateTestStimulusSettingsDropdown(testSettings.StimulusType);
    }

    // MARK: Populate Dropdowns
    // Catch-all for all dropdowns that need to be populated
    private void PopulateDropdowns()
    {
        PopulateDurationDropdowns();
        PopulateAnimationDropdowns();
        PopulateStimulusTypeDropdowns();
        PopulateFlashColourDropdown();
    }

    private void PopulateDurationDropdowns()
    // All stimulus on or off durations, training and testing
    {
        List<string> stimulusOnTextOptions = stimulusOnOptions.Select(d => d + " s").ToList();
        List<string> stimulusOffTextOptions = stimulusOffOptions.Select(d => d + " s").ToList();

        trainStimulusOnDurationDropdown.ClearOptions();
        trainStimulusOffDurationDropdown.ClearOptions();
        testStimulusOnDurationDropdown.ClearOptions();
        testStimulusOffDurationDropdown.ClearOptions();

        trainStimulusOnDurationDropdown.AddOptions(stimulusOnTextOptions);
        trainStimulusOffDurationDropdown.AddOptions(stimulusOffTextOptions);
        testStimulusOnDurationDropdown.AddOptions(stimulusOnTextOptions);
        testStimulusOffDurationDropdown.AddOptions(stimulusOffTextOptions);
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

    private void PopulateStimulusTypeDropdowns()
    {
        trainStimulusTypeDropdown.ClearOptions();
        testStimulusTypeDropdown.ClearOptions();

        trainStimulusTypeDropdown.AddOptions(stimulusTypeOptions);
        testStimulusTypeDropdown.AddOptions(stimulusTypeOptions);
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

    // MARK: Helpers to update stimulus settings dropdown based on stimulus type
    private void UpdateTrainStimulusSettingsDropdown(BocciaStimulusType stimulusType)
    {
        if (stimulusType == BocciaStimulusType.FaceSprite)
        {
            trainFlashColourSetting.SetActive(false);
        }
        else
        {
            trainFlashColourSetting.SetActive(true);
        }
    }

    private void UpdateTestStimulusSettingsDropdown(BocciaStimulusType stimulusType)
    {
        if (stimulusType == BocciaStimulusType.FaceSprite)
        {
            testFlashColourSetting.SetActive(false);
        }
        else
        {
            testFlashColourSetting.SetActive(true);
        }
    }

    // Helper method to add listeners to all UI elements
    private void AddListenersToUI()
    {
        // Training settings listeners
        trainNumFlashesInputField.onEndEdit.AddListener(OnChangeTrainNumFlashes);
        trainNumTrainingSelectionsInputField.onEndEdit.AddListener(OnChangeTrainNumTrainingSelections);
        trainTargetAnimationDropdown.onValueChanged.AddListener(OnChangeTrainTargetAnimation);
        trainShamSelectionFeedbackToggle.onValueChanged.AddListener(OnChangeTrainShamSelectionFeedback);
        trainShamSelectionAnimationDropdown.onValueChanged.AddListener(OnChangeTrainShamSelectionAnimation);
        trainStimulusOnDurationDropdown.onValueChanged.AddListener(OnChangeTrainStimulusOnDuration);
        trainStimulusOffDurationDropdown.onValueChanged.AddListener(OnChangeTrainStimulusOffDuration);
        trainStimulusTypeDropdown.onValueChanged.AddListener(OnChangeTrainStimulusType);
        trainFlashColourDropdown.onValueChanged.AddListener(OnChangeTrainFlashColour);

        // Testing settings listeners
        testNumFlashesInputField.onEndEdit.AddListener(OnChangeTestNumFlashes);
        testTargetSelectionFeedbackToggle.onValueChanged.AddListener(OnChangeTestTargetSelectionFeedback);
        testTargetSelectionAnimationDropdown.onValueChanged.AddListener(OnChangeTestTargetSelectionAnimation);
        testStimulusOnDurationDropdown.onValueChanged.AddListener(OnChangeTestStimulusOnDuration);
        testStimulusOffDurationDropdown.onValueChanged.AddListener(OnChangeTestStimulusOffDuration);
        testStimulusTypeDropdown.onValueChanged.AddListener(OnChangeTestStimulusType);
        testFlashColourDropdown.onValueChanged.AddListener(OnChangeTestFlashColour);
        separateButtonsToggle.onValueChanged.AddListener(OnChangeSeparateButtons);
    }

    // Helper methods to get the dropdown index based on duration
    private int GetOnDurationDropdownIndex(float duration)
    {
        return stimulusOnOptions.IndexOf(duration);  // Find the index of the duration in the list
    }

    private int GetOffDurationDropdownIndex(float duration)
    {
        return stimulusOffOptions.IndexOf(duration);  // Find the index of the duration in the list
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

    private void OnChangeTrainNumTrainingSelections(string value)
    {
        if (int.TryParse(value, out int numTrainingSelections))
        {
            // Clamp value within min and max
            numTrainingSelections = Mathf.Clamp(numTrainingSelections, MIN_NUM_TRAIN_SELECTIONS, MAX_NUM_TRAIN_SELECTIONS);

            // Update the model and previous value
            _model.SetBciOption(ref _model.P300Settings.Train.NumTrainingSelections, numTrainingSelections);
            previousTrainNumTrainingSelections = numTrainingSelections;
        }
        else
        {
            // Revert to previous valid value
            trainNumTrainingSelectionsInputField.text = previousTrainNumTrainingSelections.ToString();
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
        var selectedDuration = stimulusOnOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Train.StimulusOnDuration, selectedDuration);
    }

    private void OnChangeTrainStimulusOffDuration(int index)
    {
        var selectedDuration = stimulusOffOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Train.StimulusOffDuration, selectedDuration);
    }

    private void OnChangeTrainStimulusType(int index)
    {
        var selectedStimulusType = (BocciaStimulusType)index;
        _model.SetBciOption(ref _model.P300Settings.Train.StimulusType, selectedStimulusType);
        UpdateTrainStimulusSettingsDropdown(selectedStimulusType);
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
        var selectedDuration = stimulusOnOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Test.StimulusOnDuration, selectedDuration);
    }

    private void OnChangeTestStimulusOffDuration(int index)
    {
        var selectedDuration = stimulusOffOptions[index];
        _model.SetBciOption(ref _model.P300Settings.Test.StimulusOffDuration, selectedDuration);
    }

    private void OnChangeTestStimulusType(int index)
    {
        var selectedStimulusType = (BocciaStimulusType)index;
        _model.SetBciOption(ref _model.P300Settings.Test.StimulusType, selectedStimulusType);
        UpdateTestStimulusSettingsDropdown(selectedStimulusType);
    }

    private void OnChangeTestFlashColour(int index)
    {
        var selectedColour = GetColourFromDropdownIndex(index);
        _model.SetBciOption(ref _model.P300Settings.Test.FlashColour, selectedColour);
    }

    private void OnChangeSeparateButtons(bool isOn)
    {
        _model.SetBciOption(ref _model.P300Settings.SeparateButtons, isOn);
    }

    // MARK: Update P300 Controller
    // ================================
    // Documentation for setting P300ControllerBehavior settings:
    // --------------------------------
    // 1. Modifying settings in the BCI Options menu UI updates the P300 training or test settings in the model.
    //
    // 2. Clicking the "Reset to Defaults" button will set all P300 Settings to their default values in the model.
    //
    // 3. The P300ControllerBehavior script is updated when entering Training, Play, or Virtual Play screens.
    //    This script is subscribed to the NavigationChanged event and calls the appropriate method to update the P300ControllerBehavior:
    //    - For Training, UpdateP300ControllerTraining() updates P300ControllerBehavior with the model's current TRAIN settings.
    //    - For Play & Virtual Play, UpdateP300ControllerTesting() updates P300ControllerBehavior with the model's current TEST settings.
    // ================================

    // Set the relevant P300ControllerBehavior values from the current P300 TRAINING settings in the model
    private void UpdateP300ControllerTraining()
    {
        // Debug.Log("Update P300ControllerBehavior for TRAINING");

        // Number of flashes
        p300ControllerBehavior.numFlashesLowerLimit = _model.P300Settings.Train.NumFlashes;
        p300ControllerBehavior.numFlashesUpperLimit = _model.P300Settings.Train.NumFlashes;

        // Number of training Selections
        p300ControllerBehavior.numTrainingSelections = _model.P300Settings.Train.NumTrainingSelections;

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