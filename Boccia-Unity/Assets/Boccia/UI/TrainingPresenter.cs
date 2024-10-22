using System.Collections;
using System.Collections.Generic;
using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingPresenter : MonoBehaviour
{
    private BocciaModel _model;

    public GameObject bciControllerManager;

    public GameObject instructionText;

    public FanPresenter fanPresenter;

    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.WasChanged += ModelChanged;
        _model.BciChanged += TrainingComplete;

        // Generate the fan
        //fanPresenter.GenerateFan();
    }


    void OnDisable()
    {
        _model.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        // For a presenter, this is usually used to refresh the UI to reflect the
        // state of the model (UI does not store state, it just provides a way
        // to view and change it)
    }

    private void TrainingComplete()
    {
        // Go back to Play Menu when training is done
        if (_model.BciTrained == true)
        {
            instructionText.GetComponent<TextMeshProUGUI>().text = "Training complete.";
            StartCoroutine(BackToPlayMenu());
        }
    }

    private IEnumerator BackToPlayMenu()
    {
        // Wait before switching to Play Menu so the user can see the text
        yield return new WaitForSecondsRealtime(5f);
        _model.PlayMenu();
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
