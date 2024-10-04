using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// This is an example UI presenter.  This could be broken into several smaller scripts
// that each reference their own local UI element.  Typically there's interaction between
// UI elements, so can be more convenient to keep the logic in a single presenter.
public class ExamplePresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button moveUpButton;
    public Button moveDownButton;
    public Button resetRampButton;
    public Button dropBallButton;
    public Button colorButton;
    public Button randomJackButton;

    private BocciaModel model;


    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // connect buttons to model
        rotateLeftButton.onClick.AddListener(RotateLeft);
        rotateRightButton.onClick.AddListener(RotateRight);
        moveUpButton.onClick.AddListener(MoveUp);
        moveDownButton.onClick.AddListener(MoveDown);
        resetRampButton.onClick.AddListener(model.ResetRampPosition);
        dropBallButton.onClick.AddListener(model.DropBall);
        colorButton.onClick.AddListener(model.RandomColor);
        randomJackButton.onClick.AddListener(model.RandomJackBall);
    }


    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
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

    // Somehow we'll have to figure out the amount of rotation by looking at what SPO was clicked
    // Perhaps the SPO onCLick can provide the value that's initialized when the fan is created?
    // Temporary hard-coded values for incrementing
    private void RotateRight()
    {
        model.RotateBy(6.0f);
    }

    private void RotateLeft()
    {
        model.RotateBy(-6.0f);
    }

    private void MoveUp()
    {
        model.ElevateBy(1.0f);
    }

    private void MoveDown()
    {
        model.ElevateBy(-1.0f);
    }

}
