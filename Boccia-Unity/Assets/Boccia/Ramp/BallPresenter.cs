using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    public GameObject ball; // The ball prefab
    private GameObject _activeBall; // Refers the the ball currently in use for each shot
    private int _ballCount = 0;
    private Rigidbody _ballRigidbody;

    private Animator _barAnimation;
    public GameObject dropBar;
    public GameObject elevationPlate;

    private BocciaModel _model;

    private Vector3 _dropPosition;
    private Quaternion _dropRotation;

    private Coroutine _barCoroutine;
    private Coroutine _checkBallCoroutine;

    private bool _firstBallDropped = false; // To check if at least one ball has been dropped

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.WasChanged += RampChanged;
        _model.BallResetChanged += ResetBocciaBalls;

        // Initialize ball
        _activeBall = GameObject.FindWithTag("BocciaBall"); // The ball already in the scene
        InitializeBall();

        // Initialize bar animation
        _barAnimation = dropBar.GetComponent<Animator>();

        // initialize to saved data
        RampChanged();
    }

    void OnDisable()
    {
        _model.WasChanged -= RampChanged;
        _model.BallResetChanged -= ResetBocciaBalls;
    }

    private void InitializeBall()
    {
        // Make sure the ball is enabled
        if (!_activeBall.activeSelf)
        {
            _activeBall.SetActive(true);
        }

        // Initialize rigidbody 
        _ballRigidbody = _activeBall.GetComponent<Rigidbody>();
        // Set sleep threshold to minimum so the ball is ready to roll
        _ballRigidbody.sleepThreshold = 0.0f;

        // Increment ball count
        _ballCount++;

        // Name the ball GameObject in the hierarchy
        _activeBall.name = "Ball " + _ballCount;

        // Make sure its the right color
        _activeBall.GetComponent<Renderer>().material.color = _model.GameOptions.BallColor;
    }

    private void RampChanged()
    {
        // Updates color if ball color is pressed
        _activeBall.GetComponent<Renderer>().material.color = _model.GameOptions.BallColor;

        // If model.BarState is true, it means the bar opened (drop ball was pressed)
        if (_model.BarState)
        {
            // Start the bar movement animation
            _barCoroutine = StartCoroutine(BarAnimation());

            if (_model.BallState == BocciaBallState.Ready)
            {
                DropBall();
            }
        }
    }

    private void DropBall()
    {
        // Convert ball position and rotation to local space of elevationPlate
        _dropPosition = elevationPlate.transform.InverseTransformPoint(_activeBall.transform.position);
        _dropRotation = Quaternion.Inverse(elevationPlate.transform.rotation) * _activeBall.transform.rotation;

        // Set the ball state to released
        _model.SetBallStateReleased();

        // Start the coroutine to check the ball speed
        _checkBallCoroutine = StartCoroutine(CheckBallSpeed());

        // Toggle the ball drop flag
        _firstBallDropped = true;
    }

    private IEnumerator BarAnimation()
    {
        _model.SetRampMoving(true);
        // Bar opening and closing animation
        _barAnimation.SetBool("isOpening", true);
        yield return new WaitForSecondsRealtime(1f);
        _barAnimation.SetBool("isOpening", false);
        _model.ResetBar(); // Call the method to reset the bar state to false

        yield return null;
        _model.SetRampMoving(false);
    }

    private IEnumerator CheckBallSpeed()
    {
        // Wait a bit for the ball to fully get rolling
        yield return new WaitForSecondsRealtime(3f);

        // Velocity threshold
        while (_ballRigidbody.velocity.magnitude > 0.01f)
        {
            yield return new WaitForSecondsRealtime(0.1f); 
        }

        // Stop the ball
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;

        // Create a new ball
        NewBocciaBall();
    }

    private void NewBocciaBall()
    {
        // Make sure the CheckBallSpeed coroutine is stopped
        if (_checkBallCoroutine != null)
        {
            StopCoroutine(_checkBallCoroutine);
            _checkBallCoroutine = null;
        }

        // Create a new ball at the previous ball's drop position and rotation
        // Convert the drop position and rotation back into to world space
        Vector3 newBallPosition = elevationPlate.transform.TransformPoint(_dropPosition);
        Quaternion newBallRotation = elevationPlate.transform.rotation * _dropRotation;

        // Instantiate the new ball
        _activeBall = Instantiate(ball, newBallPosition, newBallRotation, transform);
        InitializeBall();

        // Set the ball state to ready
        _model.SetBallStateReady();
    }

    private void ResetBocciaBalls()
    {
        // Check if at least one ball has been dropped
        // since Start or the last reset
        if (!_firstBallDropped)
        {
            return;
        }

        // Stop the coroutines if they are running
        if (_barCoroutine != null)
        {
            StopCoroutine(_barCoroutine);
            _barCoroutine = null;
        }

        if (_checkBallCoroutine != null) 
        {
            StopCoroutine(_checkBallCoroutine);
            _checkBallCoroutine = null;
        }

        // Reset ball count and first drop flag
        _ballCount = 0;
        _firstBallDropped = false;

        // Create a new active ball
        NewBocciaBall();

        // Remove the old boccia balls
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("BocciaBall") && child.gameObject != _activeBall)
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