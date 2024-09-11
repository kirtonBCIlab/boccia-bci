using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RampPresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public GameObject ramp;
    private BocciaModel model;


    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // initialize ramp to saved data
        ModelChanged();
    }


    private void OnDisable()
    {
        BocciaModel.WasChanged -= ModelChanged;
    }

    void Update()
    {
        // For high rate changes, we can poll the model
        if (model?.IsRotating ?? false)
        {
            ramp.transform.Rotate(model.RotationRates * Time.deltaTime);
        }
    }

    private void ModelChanged()
    {
        // For lower rate changes, update when model sends change event
        ramp.GetComponent<Renderer>().material.color = model.RampColor;
    }
}

