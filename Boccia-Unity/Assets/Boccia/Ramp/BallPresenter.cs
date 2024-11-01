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

    private Coroutine _dropBallCoroutine;
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
            // Save ball position and rotation right before it is dropped
            //dropPosition = activeBall.transform.position; 
            //dropRotation = activeBall.transform.rotation;

            // Convert ball position and rotation to local space of elevationPlate
            _dropPosition = elevationPlate.transform.InverseTransformPoint(_activeBall.transform.position);
            _dropRotation = Quaternion.Inverse(elevationPlate.transform.rotation) * _activeBall.transform.rotation;

            // Start the bar movement animation
            StartCoroutine(DropBall());
        }
    }

    private IEnumerator DropBall()
    {
        _model.SetRampMoving(true);
        // Bar opening and closing animation
        _barAnimation.SetBool("isOpening", true);
        yield return new WaitForSecondsRealtime(1f);
        _barAnimation.SetBool("isOpening", false);
        _model.ResetBar(); // Call the method to reset the bar state to false

        // Wait to check speed of ball to avoid NewBocciaBall() happening too early
        yield return new WaitForSecondsRealtime(1f); 
        _checkBallCoroutine = StartCoroutine(CheckBallSpeed());

        // Toggle the ball drop flag
        _firstBallDropped = true;

        yield return null;
        _model.SetRampMoving(false);
    }

    private IEnumerator CheckBallSpeed()
    {
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

        // If this is Play mode, remove the previous ball
        if (_model.GameMode == BocciaGameMode.Play)
        {
            GameObject previousBall = _activeBall;
            Destroy(previousBall);
        }

        // Instantiate the new ball
        _activeBall = Instantiate(ball, newBallPosition, newBallRotation, transform);
        InitializeBall();
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
        if (_dropBallCoroutine != null)
        {
            StopCoroutine(_dropBallCoroutine);
            _dropBallCoroutine = null;
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