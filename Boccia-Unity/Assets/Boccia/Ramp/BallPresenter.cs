using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    public GameObject ball;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // initialize ball to saved data
        ModelChanged();
    }

    private void OnDisable()
    {
        BocciaModel.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        // For lower rate changes, update when model sends change event
        ball.GetComponent<Renderer>().material.color = model.BallColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
