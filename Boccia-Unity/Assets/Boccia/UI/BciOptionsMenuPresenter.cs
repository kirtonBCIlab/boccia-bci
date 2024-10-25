using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BciOptionsMenuPresenter : MonoBehaviour
{
    public TMP_Dropdown paradigmDropdown;
    public GameObject p300SettingsPanel;  // Panel for P300 settings
    public Button resetDefaultsButton;
    public Button doneButton;

    private BocciaModel _model;

    void Start()
    {
        _model = BocciaModel.Instance;

        // Call the method to populate the paradigm dropdown
        PopulateParadigmDropdown();

        // Initialize the paradigm dropdown listener
        paradigmDropdown.onValueChanged.AddListener(OnParadigmChanged);

        // Initialize button listeners
        resetDefaultsButton.onClick.AddListener(OnResetDefaultsClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        // Initialize the active paradigm UI
        InitializeActiveParadigmUI();
    }

    void OnEnable()
    {
        // This check is to avoid NullReferenceExceptions that happen when OnEnable() attempts to run before the game data that contains the model is loaded
        if (_model == null)
        {
            // Debug.LogError("Model is not initialized yet in OnEnable.");
            return; // Avoid running further code if the model is not ready
        }

        // Initialize the active paradigm UI
        InitializeActiveParadigmUI();
    }

    // Populates the paradigm dropdown with enum values from BocciaBciParadigm
    private void PopulateParadigmDropdown()
    {
        // Clear any existing options in the dropdown
        paradigmDropdown.ClearOptions();

        // Get the names of the BocciaBciParadigm enum as a string list
        List<string> paradigmOptions = new List<string>(Enum.GetNames(typeof(BocciaBciParadigm)));

        // Add the paradigm options to the dropdown
        paradigmDropdown.AddOptions(paradigmOptions);

        // Set the default value in the dropdown to match the model's current paradigm
        paradigmDropdown.value = (int)_model.Paradigm;
    }

    // Initialize which paradigm's settings to show on UI load
    private void InitializeActiveParadigmUI()
    {
        switch (_model.Paradigm)  // Use the public property
        {
            case BocciaBciParadigm.P300:
                paradigmDropdown.value = 0;
                p300SettingsPanel.SetActive(true);
                break;
        }
    }

    // When the paradigm is changed from the dropdown
    private void OnParadigmChanged(int selectedIndex)
    {
        // Update the paradigm in the model based on the selected dropdown option
        _model.SetBciParadigm((BocciaBciParadigm)selectedIndex);

        // Update the UI to show only the relevant settings for the selected paradigm
        UpdateActiveParadigmUI();
    }

    // Update the active paradigm settings on UI change
    private void UpdateActiveParadigmUI()
    {
        switch (_model.Paradigm)  // Use the public property
        {
            case BocciaBciParadigm.P300:
                if (!p300SettingsPanel.activeSelf)
                {
                    p300SettingsPanel.SetActive(true);
                }
                // Put logic here to turn off active settings panels that are not the selected paradigm
                // if (otherParadigmSettingsPanel.activeSelf)
                // {
                //     otherParadigmSettingsPanel.SetActive(false);
                // }
                break;
        }
    }

    // Resets the current active paradigm settings to their default
    private void OnResetDefaultsClicked()
    {
        // Reset the BCI settings to defaults for the active paradigm
        _model.ResetBciSettingsToDefaults();

        // The UI will update automatically when the settings reset due to BciChanged event
    }

    // Navigate back to the previous screen when "Done" is clicked
    private void OnDoneButtonClicked()
    {
        _model.ShowPreviousScreen();
    }
}