using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPresenter : MonoBehaviour
{
    // Model reference
    private BocciaModel _model;

    // References for the Boccia balls
    public GameObject ball; // The ball prefab
    private GameObject _activeBall; // Refers the the ball currently in use for each shot
    private int _ballCount = 0;
    private Rigidbody _ballRigidbody;

    // References for the bar and elevation mechanism
    private Animator _barAnimation;
    public GameObject dropBar;
    public GameObject elevationPlate;

    // Variables for storing the ball transform
    private Vector3 _dropPosition;
    private Quaternion _dropRotation;

    // Coroutines
    private Coroutine _barCoroutine;
    private Coroutine _checkBallCoroutine;

    // Flags
    private bool _firstBallDropped = false; // To check if at least one ball has been dropped

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.WasChanged += ModelChanged;
        _model.BallResetChanged += ResetBocciaBalls;

        // Initialize ball
        _activeBall = GameObject.FindWithTag("BocciaBall"); // The ball already in the scene
        InitializeBall();

        // Initialize bar animation
        _barAnimation = dropBar.GetComponent<Animator>();

        // Initialize to saved data
        ModelChanged();
    }

    void OnDisable()
    {
        _model.WasChanged -= ModelChanged;
        _model.BallResetChanged -= ResetBocciaBalls;
    }

    private void InitializeBall()
    {
        // Make sure the ball objectis enabled
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

        // Make sure it is the right color
        _activeBall.GetComponent<Renderer>().material.color = _model.GameOptions.BallColor;
    }

    private void ModelChanged()
    {
        // Updates color if ball color button is pressed
        _activeBall.GetComponent<Renderer>().material.color = _model.GameOptions.BallColor;

        // If model.BarState is true, it means the bar opened (drop ball button was pressed)
        if (_model.BarState)
        {
            // Start the bar movement animation
            _barCoroutine = StartCoroutine(BarAnimation());

            // Only execute the ball drop code if the Model's ball state is Ready
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

        // Call the method to reset the Model's bar state
        // This is different from the states in the animator
        _model.ResetBar();

        // Wait for the bar to fully close
        yield return new WaitForSecondsRealtime(3f);
        _model.SetRampMoving(false);

        yield return null;
    }

    private bool IsBarClosed()
    {
        // Bool to check if the animation is complete
        return _barAnimation.GetCurrentAnimatorStateInfo(0).IsName("DropBarClosed");
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

        // Do not instantiate a new ball unless the bar is currently closed
        while (!IsBarClosed())
        {
            yield return null;
        }

        // Create a new ball
        NewBocciaBall();
    }

    private void NewBocciaBall()
    {
        // Stop the check ball coroutine if it is running
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

        // Stop the check ball coroutine if it is running
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