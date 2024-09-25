using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This is an example UI presenter.  This could be broken into several smaller scripts
// that each reference their own local UI element.  Typically there's interaction between
// UI elements, so can be more convenient to keep the logic in a single presenter.
public class GameOptionsMenuPresenter : MonoBehaviour
{
    public TMPro.TMP_Dropdown ballColorDropdown;

    private BocciaModel model;

    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // connect UI to model
        ballColorDropdown.onValueChanged.AddListener(ChangeBallColor);
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

    public void ChangeBallColor(int valueIndex)
    {
        //get the selected dropdown option
        string selectedValue = ballColorDropdown.options[valueIndex].text;
        Debug.Log("Selected dropdown value: " + selectedValue);
        
        //send the selected color to the model
        model.ChangeBallColor(selectedValue);
    }
}
