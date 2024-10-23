using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    public GameObject ball; // The ball prefab
    private GameObject activeBall; // Refers the the ball currently in use for each shot
    private int ballCount = 0;
    private Rigidbody ballRigidbody;

    private Animator barAnimation;
    public GameObject dropBar;
    public GameObject elevationPlate;

    private BocciaModel model;

    private Vector3 dropPosition;
    private Quaternion dropRotation;

    private Coroutine dropBallCoroutine;
    private Coroutine checkBallCoroutine;

    private bool firstBallDropped = false; // To check if at least one ball has been dropped

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += RampChanged;
        model.BallResetChanged += ResetBocciaBalls;

        // Initialize ball
        activeBall = GameObject.FindWithTag("BocciaBall"); // The ball already in the scene
        InitializeBall();

        // Initialize bar animation
        barAnimation = dropBar.GetComponent<Animator>();

        // initialize to saved data
        RampChanged();
    }

    void OnDisable()
    {
        model.WasChanged -= RampChanged;
        model.BallResetChanged -= ResetBocciaBalls;
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

        // Increment ball count
        ballCount++;

        // Name the ball GameObject in the hierarchy
        activeBall.name = "Ball " + ballCount;

        // Make sure its the right color
        activeBall.GetComponent<Renderer>().material.color = model.BallColor;
    }

    private void RampChanged()
    {
        // Updates color if ball color is pressed
        activeBall.GetComponent<Renderer>().material.color = model.GameOptions.BallColor;

        // If model.BarState is true, it means the bar opened (drop ball was pressed)
        if (model.BarState)
        {
            // Save ball position and rotation right before it is dropped
            //dropPosition = activeBall.transform.position; 
            //dropRotation = activeBall.transform.rotation;

            // Convert ball position and rotation to local space of elevationPlate
            dropPosition = elevationPlate.transform.InverseTransformPoint(activeBall.transform.position);
            dropRotation = Quaternion.Inverse(elevationPlate.transform.rotation) * activeBall.transform.rotation;

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

        // Toggle the ball drop flag
        firstBallDropped = true;

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

        // Create a new ball at the previous ball's drop position and rotation
        // Convert the drop position and rotation back into to world space
        Vector3 newBallPosition = elevationPlate.transform.TransformPoint(dropPosition);
        Quaternion newBallRotation = elevationPlate.transform.rotation * dropRotation;

        // Instantiate the new ball
        activeBall = Instantiate(ball, newBallPosition, newBallRotation, transform);
        InitializeBall();
    }

    private void ResetBocciaBalls()
    {
        // Check if at least one ball has been dropped
        // since Start or the last reset
        if (!firstBallDropped)
        {
            return;
        }

        // Stop the coroutines if they are running
        if (dropBallCoroutine != null)
        {
            StopCoroutine(dropBallCoroutine);
            dropBallCoroutine = null;
        }

        if (checkBallCoroutine != null) 
        {
            StopCoroutine(checkBallCoroutine);
            checkBallCoroutine = null;
        }

        // Reset ball count and first drop flag
        ballCount = 0;
        firstBallDropped = false;

        // Create a new active ball
        NewBocciaBall();

        // Remove the old boccia balls
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("BocciaBall") && child.gameObject != activeBall)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /*
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
    */
}