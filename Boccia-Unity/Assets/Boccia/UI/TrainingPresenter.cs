using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingPresenter : MonoBehaviour
{
    private BocciaModel model;

    public GameObject instructionText;

    public FanPresenter fanPresenter;

    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // Generate the fan
        fanPresenter.GenerateFan();
    }


    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        // For a presenter, this is usually used to refresh the UI to reflect the
        // state of the model (UI does not store state, it just provides a way
        // to view and change it)
    }

    void Update()
    {
        // If t is pressed, update the instruction text
        if (Input.GetKeyDown(KeyCode.T))
        {
            instructionText.GetComponent<TextMeshProUGUI>().text = "Training triggered.";
        }
    }
}
