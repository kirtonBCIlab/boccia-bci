using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayPresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public Button rotateStartButton;
    public Button rotateStopButton;
    public Button colorButton;

    private BocciaModel model;


    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // connect buttons to model
        rotateStartButton.onClick.AddListener(model.StartRotation);
        rotateStopButton.onClick.AddListener(model.StopRotation);
        colorButton.onClick.AddListener(model.RandomColor);
    }


    private void OnDisable()
    {
        BocciaModel.WasChanged -= ModelChanged;
    }

    void Update()
    {
    }

    private void ModelChanged()
    {
        // update the view accorting to new model state
        // in this case, there's nothing to update as there's just two buttons
    }

}
