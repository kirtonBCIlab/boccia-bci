using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    public GameObject ball;
    private Rigidbody ballRigidbody;
    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // Initialize rigidbody 
        ballRigidbody = ball.GetComponent<Rigidbody>();

        // initialize ball to saved data
        ModelChanged();
    }

    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        // For lower rate changes, update when model sends change event
        ball.GetComponent<Renderer>().material.color = model.BallColor;

        // Set sleep threshold to minimum so the ball is ready to roll
        ballRigidbody.sleepThreshold = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(ballRigidbody.velocity.magnitude);
    }
}
