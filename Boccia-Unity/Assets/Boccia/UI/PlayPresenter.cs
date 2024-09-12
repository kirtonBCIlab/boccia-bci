using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// This is an example UI presenter.  This could be broken into several smaller scripts
// that each reference their own local UI element.  Typically there's interaction between
// UI elements, so can be more convenient to keep the logic in a single presenter.
public class PlayPresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button colorButton;

    private BocciaModel model;


    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // connect buttons to model
        rotateLeftButton.onClick.AddListener(model.RotateLeft);
        rotateRightButton.onClick.AddListener(model.RotateRight);
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
        // For a presenter, this is usually used to refresh the UI to reflect the
        // state of the model (UI does not store state, it just provides a way
        // to view and change it)
    }

}
