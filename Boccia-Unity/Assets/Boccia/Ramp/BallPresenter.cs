using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    public GameObject ball; // The ball prefab
    private GameObject activeBall; // Refers the the ball currently in use for each shot
    private int ballCount = 1;
    private Rigidbody ballRigidbody;

    private Animator barAnimation;
    public GameObject dropBar;

    private BocciaModel model;

    private Vector3 dropPosition;
    private Quaternion dropRotation;

    private Coroutine checkBallCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // Initialize ball
        activeBall = ball; // The ball already in the scene
        InitializeBall();

        // Initialize bar animation
        barAnimation = dropBar.GetComponent<Animator>();

        // initialize ball to saved data
        ModelChanged();
    }

    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
    }

    private void InitializeBall()
    {
        // Make sure the ball is enabled
        if (!activeBall.activeSelf)
        {
            activeBall.SetActive(true);
        }

        // Initialize rigidbody 
        ballRigidbody = activeBall.GetComponent<Rigidbody>();
        // Set sleep threshold to minimum so the ball is ready to roll
        ballRigidbody.sleepThreshold = 0.0f;

        // Name the ball GameObject in the hierarchy
        activeBall.name = "Ball " + ballCount;
    }

    private void ModelChanged()
    {
        // For lower rate changes, update when model sends change event
        activeBall.GetComponent<Renderer>().material.color = model.BallColor;

        // If model.BarState is true, it means the bar is opened
        if (model.BarState)
        {
            // Save ball position and rotation right before it is dropped
            dropPosition = activeBall.transform.position; 
            dropRotation = activeBall.transform.rotation;

            // Start the bar movement animation
            StartCoroutine(DropBall());
        }
    }

    private IEnumerator DropBall()
    {
        // Bar opening and closing animation
        barAnimation.SetBool("isOpening", true);
        yield return new WaitForSecondsRealtime(1f);
        barAnimation.SetBool("isOpening", false);
        model.ResetBar(); // Call the method to reset the bar state to false

        // Wait to check speed of ball to avoid NewBocciaBall() happening too early
        yield return new WaitForSecondsRealtime(1f); 
        checkBallCoroutine = StartCoroutine(CheckBallSpeed());

        yield return null;
    }

    private IEnumerator CheckBallSpeed()
    {
        // Velocity threshold
        while (ballRigidbody.velocity.magnitude > 0.01f)
        {
            yield return new WaitForSecondsRealtime(0.1f); 
        }

        // Stop the ball
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        // Create a new ball
        NewBocciaBall();
    }

    private void NewBocciaBall()
    {
        // Make sure the CheckBallSpeed coroutine is stopped
        if (checkBallCoroutine != null)
        {
            StopCoroutine(checkBallCoroutine);
            checkBallCoroutine = null;
        }

        // Increment ball count
        ballCount++;

        // Create new ball at the previous ball's drop position and rotation
        activeBall = Instantiate(ball, dropPosition, dropRotation, transform);
        InitializeBall();
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if any boccia ball currently in the scene rolls out of bounds
        if (other.gameObject.CompareTag("BocciaBall"))
        {
            Debug.Log(other.gameObject.name + " out of bounds");
            other.gameObject.SetActive(false); // Disable the out of bounds ball
            // Destroy(other.gameObject);

            // Create new boccia ball
            NewBocciaBall();
        }
    }
}