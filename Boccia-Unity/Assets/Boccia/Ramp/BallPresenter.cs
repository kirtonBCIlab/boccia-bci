using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    public GameObject ball;
    private Rigidbody ballRigidbody;

    private Animator barAnimation;
    public GameObject dropBar;

    private BocciaModel model;

    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // Initialize rigidbody 
        ballRigidbody = ball.GetComponent<Rigidbody>();
        // Initialize bar animation
        barAnimation = dropBar.GetComponent<Animator>();

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

        initialPosition = ball.transform.position;

        // Set sleep threshold to minimum so the ball is ready to roll
        ballRigidbody.sleepThreshold = 0.0f;

        if (model.BarState)
        {
             StartCoroutine(BarAnimation()); // Start the bar movement animation
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(ballRigidbody.velocity.magnitude);

        if (ballRigidbody.velocity.magnitude < 0.01f)
        {
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator BarAnimation()
    {
        barAnimation.SetBool("isOpening", true);

        yield return new WaitForSecondsRealtime(1f);

        barAnimation.SetBool("isOpening", false);

        yield return null;
    }
}
