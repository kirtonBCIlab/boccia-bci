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

    private Vector3 dropPosition;

    private Coroutine checkBallCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // Initialize rigidbody 
        ballRigidbody = ball.GetComponent<Rigidbody>();
        // Set sleep threshold to minimum so the ball is ready to roll
        ballRigidbody.sleepThreshold = 0.0f;

        // Initialize bar animation
        barAnimation = dropBar.GetComponent<Animator>();

        // initialize ball to saved data
        ModelChanged();
    }

    void OnEnable()
    {
        model.WasChanged += ModelChanged;
    }

    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        // For lower rate changes, update when model sends change event
        ball.GetComponent<Renderer>().material.color = model.BallColor;

        if (model.BarState)
        {
            dropPosition = ball.transform.position; // Save ball position right before it is dropped
            StartCoroutine(DropBall()); // Start the bar movement animation
        }
    }

    private IEnumerator DropBall()
    {
        barAnimation.SetBool("isOpening", true);

        yield return new WaitForSecondsRealtime(1f);

        barAnimation.SetBool("isOpening", false);

        checkBallCoroutine = StartCoroutine(CheckBallSpeed());

        model.ResetBar(); // Call the method to reset the bar state to false

        yield return null;
    }

    private IEnumerator CheckBallSpeed()
    {
        while (ballRigidbody.velocity.magnitude > 0.01f)
        {
            //Debug.Log("Ball velocity: " + ballRigidbody.velocity.magnitude);
            yield return new WaitForSecondsRealtime(0.1f); 
        }

        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        ResetBall();
    }

    private void ResetBall()
    {
        //Debug.Log("Resetting ball to position: " + dropPosition);      
        ball.transform.position = dropPosition;

        if (checkBallCoroutine != null)
        {
            StopCoroutine(checkBallCoroutine);
            checkBallCoroutine = null;
        }
    }
}