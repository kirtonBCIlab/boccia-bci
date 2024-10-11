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

        if (model.BarState)
        {
            // Save ball position and rotation right before it is dropped
            dropPosition = activeBall.transform.position; 
            dropRotation = activeBall.transform.rotation;
            StartCoroutine(DropBall()); // Start the bar movement animation
        }
    }

    private IEnumerator DropBall()
    {
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
        //Debug.Log("Checking ball speed");
        while (ballRigidbody.velocity.magnitude > 0.01f)
        {
            //Debug.Log("Ball velocity: " + ballRigidbody.velocity.magnitude);
            yield return new WaitForSecondsRealtime(0.1f); 
        }

        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        NewBocciaBall();
    }

    private void NewBocciaBall()
    {
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
        if (other.gameObject.CompareTag("BocciaBall"))
        {
            Debug.Log(other.gameObject.name + " out of bounds");
            // Disable the out of bounds ball
            other.gameObject.SetActive(false);
            NewBocciaBall();
        }
    }
}