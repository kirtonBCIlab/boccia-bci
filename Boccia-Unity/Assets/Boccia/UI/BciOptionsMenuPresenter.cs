using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BciOptionsMenuPresenter : MonoBehaviour
{
    public TMP_Dropdown paradigmDropdown;
    public GameObject p300SettingsPanel;  // Panel for P300 settings
    // public GameObject ssvepSettingsPanel;  // Panel for SSVEP settings (future implementation)
    public Button resetDefaultsButton;
    public Button doneButton;

    private BocciaModel model;

    void Start()
    {
        model = BocciaModel.Instance;

        // Initialize the paradigm dropdown listener
        paradigmDropdown.onValueChanged.AddListener(OnParadigmChanged);

        // Initialize button listeners
        resetDefaultsButton.onClick.AddListener(OnResetDefaultsClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        // Initialize the active paradigm UI
        InitializeActiveParadigmUI();
    }

    // When the paradigm is changed from the dropdown
    // private void OnParadigmChanged(int selectedIndex)
    // {
    //     // Update the paradigm in the model based on the selected dropdown option
    //     switch (selectedIndex)
    //     {
    //         case 0: // P300 selected
    //             model.SetBciOption(ref model.Paradigm, BocciaBciParadigm.P300);  // Use the public property instead of bocciaData
    //             break;
    //         // case 1: // SSVEP (future) selected
    //         //     model.SetBciOption(ref model.Paradigm, BocciaBciParadigm.SSVEP);  // Use the public property instead of bocciaData
    //         //     break;
    //     }

    //     // Update the UI to show only the relevant settings for the selected paradigm
    //     UpdateActiveParadigmUI();
    // }

    // When the paradigm is changed from the dropdown
    // Test new
    private void OnParadigmChanged(int selectedIndex)
    {
        // Update the paradigm in the model based on the selected dropdown option
        switch (selectedIndex)
        {
            case 0: // P300 selected
                model.Paradigm = BocciaBciParadigm.P300;  // Direct assignment without ref
                break;
            // case 1: // SSVEP (future) selected
            //     model.Paradigm = BocciaBciParadigm.SSVEP;  // Direct assignment without ref
            //     break;
        }

        // Update the UI to show only the relevant settings for the selected paradigm
        UpdateActiveParadigmUI();
    }

    // Initialize which paradigm's settings to show on UI load
    private void InitializeActiveParadigmUI()
    {
        switch (model.Paradigm)  // Use the public property
        {
            case BocciaBciParadigm.P300:
                paradigmDropdown.value = 0;
                p300SettingsPanel.SetActive(true);
                // ssvepSettingsPanel.SetActive(false);  // Future implementation
                break;
            // case BocciaBciParadigm.SSVEP:
            //     paradigmDropdown.value = 1;
            //     p300SettingsPanel.SetActive(false);
            //     ssvepSettingsPanel.SetActive(true);
            //     break;
        }
    }

    // Update the active paradigm settings on UI change
    private void UpdateActiveParadigmUI()
    {
        switch (model.Paradigm)  // Use the public property
        {
            case BocciaBciParadigm.P300:
                if (!p300SettingsPanel.activeSelf)
                {
                    p300SettingsPanel.SetActive(true);
                }
                // if (ssvepSettingsPanel.activeSelf)
                // {
                //     ssvepSettingsPanel.SetActive(false);
                // }
                break;

            // case BocciaBciParadigm.SSVEP:
            //     if (!ssvepSettingsPanel.activeSelf)
            //     {
            //         ssvepSettingsPanel.SetActive(true);
            //     }
            //     if (p300SettingsPanel.activeSelf)
            //     {
            //         p300SettingsPanel.SetActive(false);
            //     }
            //     break;
        }
    }

    // Resets the current active paradigm settings to their default
    private void OnResetDefaultsClicked()
    {
        // Reset the BCI settings to defaults for the active paradigm
        model.ResetBciSettingsToDefaults();

        // The UI will update automatically when the settings reset due to BciChanged event
    }

    // Navigate back to the previous screen when "Done" is clicked
    private void OnDoneButtonClicked()
    {
        model.ShowPreviousScreen();
    }
}