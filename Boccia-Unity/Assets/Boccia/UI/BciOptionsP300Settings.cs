using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;  // To enable LINQ methods like First()

public class BciOptionsP300Settings : MonoBehaviour
{
    // UI elements for Training settings
    public TMP_InputField trainNumFlashesInputField;
    public TMP_InputField trainNumTrainingWindowsInputField;
    public TMP_Dropdown trainTargetAnimationDropdown;
    public Toggle trainShamSelectionFeedbackToggle;
    public TMP_Dropdown trainShamSelectionAnimationDropdown;
    public TMP_InputField trainStimulusOnDurationInputField;
    public TMP_InputField trainStimulusOffDurationInputField;
    public TMP_Dropdown trainFlashColourDropdown;

    // UI elements for Testing settings
    public TMP_InputField testNumFlashesInputField;
    public Toggle testTargetSelectionFeedbackToggle;
    public TMP_Dropdown testTargetSelectionAnimationDropdown;
    public TMP_InputField testStimulusOnDurationInputField;
    public TMP_InputField testStimulusOffDurationInputField;
    public TMP_Dropdown testFlashColourDropdown;

    private BocciaModel model;

    private static readonly Dictionary<string, Color> colours = new Dictionary<string, Color>
    {
        {"Red", Color.red },
        {"Blue", Color.blue },
        {"Green", Color.green },
        {"Yellow", Color.yellow }
    };

    void Start()
    {
        model = BocciaModel.Instance;

        // Subscribe to BCI change events to keep the UI updated
        model.BciChanged += OnBciSettingsChanged;

        // Add listeners to UI elements
        AddListenersToUI();
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

        // Set training settings UI
        trainNumFlashesInputField.text = trainSettings.NumFlashes.ToString();
        trainNumTrainingWindowsInputField.text = trainSettings.NumTrainingWindows.ToString();
        trainTargetAnimationDropdown.value = (int)trainSettings.TargetAnimation;
        trainShamSelectionFeedbackToggle.isOn = trainSettings.ShamSelectionFeedback;
        trainShamSelectionAnimationDropdown.value = (int)trainSettings.ShamSelectionAnimation;
        trainStimulusOnDurationInputField.text = trainSettings.StimulusOnDuration.ToString();
        trainStimulusOffDurationInputField.text = trainSettings.StimulusOffDuration.ToString();
        trainFlashColourDropdown.value = GetColourDropdownIndex(trainSettings.FlashColour);

        // Set testing settings UI
        testNumFlashesInputField.text = testSettings.NumFlashes.ToString();
        testTargetSelectionFeedbackToggle.isOn = testSettings.TargetSelectionFeedback;
        testTargetSelectionAnimationDropdown.value = (int)testSettings.TargetSelectionAnimation;
        testStimulusOnDurationInputField.text = testSettings.StimulusOnDuration.ToString();
        testStimulusOffDurationInputField.text = testSettings.StimulusOffDuration.ToString();
        testFlashColourDropdown.value = GetColourDropdownIndex(testSettings.FlashColour);
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
        trainStimulusOnDurationInputField.onEndEdit.AddListener(OnChangeTrainStimulusOnDuration);
        trainStimulusOffDurationInputField.onEndEdit.AddListener(OnChangeTrainStimulusOffDuration);
        trainFlashColourDropdown.onValueChanged.AddListener(OnChangeTrainFlashColour);

        // Testing settings listeners
        testNumFlashesInputField.onEndEdit.AddListener(OnChangeTestNumFlashes);
        testTargetSelectionFeedbackToggle.onValueChanged.AddListener(OnChangeTestTargetSelectionFeedback);
        testTargetSelectionAnimationDropdown.onValueChanged.AddListener(OnChangeTestTargetSelectionAnimation);
        testStimulusOnDurationInputField.onEndEdit.AddListener(OnChangeTestStimulusOnDuration);
        testStimulusOffDurationInputField.onEndEdit.AddListener(OnChangeTestStimulusOffDuration);
        testFlashColourDropdown.onValueChanged.AddListener(OnChangeTestFlashColour);
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
            model.SetBciOption(ref model.P300Settings.Train.NumFlashes, numFlashes);
        }
    }

    private void OnChangeTrainNumTrainingWindows(string value)
    {
        if (int.TryParse(value, out int numTrainingWindows))
        {
            model.SetBciOption(ref model.P300Settings.Train.NumTrainingWindows, numTrainingWindows);
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
    }

    private void OnChangeTrainShamSelectionAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        model.SetBciOption(ref model.P300Settings.Train.ShamSelectionAnimation, selectedAnimation);
    }

    private void OnChangeTrainStimulusOnDuration(string value)
    {
        if (float.TryParse(value, out float duration))
        {
            model.SetBciOption(ref model.P300Settings.Train.StimulusOnDuration, duration);
        }
    }

    private void OnChangeTrainStimulusOffDuration(string value)
    {
        if (float.TryParse(value, out float duration))
        {
            model.SetBciOption(ref model.P300Settings.Train.StimulusOffDuration, duration);
        }
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
            model.SetBciOption(ref model.P300Settings.Test.NumFlashes, numFlashes);
        }
    }

    private void OnChangeTestTargetSelectionFeedback(bool isOn)
    {
        model.SetBciOption(ref model.P300Settings.Test.TargetSelectionFeedback, isOn);
    }

    private void OnChangeTestTargetSelectionAnimation(int index)
    {
        var selectedAnimation = (BocciaAnimation)index;
        model.SetBciOption(ref model.P300Settings.Test.TargetSelectionAnimation, selectedAnimation);
    }

    private void OnChangeTestStimulusOnDuration(string value)
    {
        if (float.TryParse(value, out float duration))
        {
            model.SetBciOption(ref model.P300Settings.Test.StimulusOnDuration, duration);
        }
    }

    private void OnChangeTestStimulusOffDuration(string value)
    {
        if (float.TryParse(value, out float duration))
        {
            model.SetBciOption(ref model.P300Settings.Test.StimulusOffDuration, duration);
        }
    }

    private void OnChangeTestFlashColour(int index)
    {
        var selectedColour = GetColourFromDropdownIndex(index);
        model.SetBciOption(ref model.P300Settings.Test.FlashColour, selectedColour);
    }
}