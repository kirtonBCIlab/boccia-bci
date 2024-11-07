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
    //private float _ballHeightThreshold = 1.0f;

    // References for the ramp components
    public GameObject dropBar;
    public GameObject elevationPlate;
    private Animator _barAnimation;
    public GameObject rampBase;

    // Variables for storing the ball transform
    private Vector3 _dropPosition;
    private Quaternion _dropRotation;
    private Vector3 _defaultBallPosition;
    private Quaternion _defaultBallRotation;

    // Coroutines
    private Coroutine _checkBallCoroutine;

    // Flags
    private bool _firstBallDropped = false; // To check if at least one ball has been dropped

    // Game mode
    private BocciaGameMode _gameMode;

    
    // MARK: Initialization
    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.WasChanged += ModelChanged;
        _model.NavigationChanged += NavigationChanged;
        _model.BallResetChanged += ResetBocciaBalls;
        _model.BallFallingChanged += HandleBallFalling;

        // Initialize ball
        _activeBall = GameObject.FindWithTag("BocciaBall"); // The ball already in the scene
        InitializeBall();

        // Calculate default ball position and rotation
        _defaultBallPosition = elevationPlate.transform.InverseTransformPoint(_activeBall.transform.position);
        _defaultBallRotation = Quaternion.Inverse(elevationPlate.transform.rotation) * _activeBall.transform.rotation;

        // Initialize bar animation
        _barAnimation = dropBar.GetComponent<Animator>();

        // Initialize to saved data
        ModelChanged();

        // Initialize gameMode
        _gameMode = _model.GameMode;
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


    // MARK: Model event handler
    private void ModelChanged()
    {
        // Updates color if ball color button is pressed
        _activeBall.GetComponent<Renderer>().material.color = _model.GameOptions.BallColor;

        // If model.BarState is true, it means the drop ball button was pressed
        if (_model.BarState)
        {
            // Start the bar movement animation
            StartCoroutine(BarAnimation());

            // Only execute the ball drop code if the Model's ball state is ReadyToRelease
            if (_model.BallState == BocciaBallState.ReadyToRelease)
            {
                // Call the method to log the drop position and rotation
                // and the rest of the ball drop code
                DropBall();
            }
        }
    }

    // MARK: Bar Animation
    private IEnumerator BarAnimation()
    {
        // Wait for the ramp to stop moving
        while (_model.IsRampMoving)
        {
            yield return null;
        }

        _model.SetRampMoving(true);

        // Bar opening and closing animation
        _barAnimation.SetBool("isOpening", true);
        yield return new WaitForSecondsRealtime(1f);
        _barAnimation.SetBool("isOpening", false);

        // Call the method to reset the Model's bar state
        // This is different from the states in the animator
        _model.ResetBar();

        // Wait for the bar to fully close
        // Before setting the ramp to not moving
        yield return new WaitForSecondsRealtime(3f);
        _model.SetRampMoving(false);

        yield return null;
    }

    private bool IsBarClosed()
    {
        // Bool to check if the animation is complete
        return _barAnimation.GetCurrentAnimatorStateInfo(0).IsName("DropBarClosed");
    }

    
    // MARK: Ball Drop
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

        // If the bar is NOT closed, wait before creating a new ball
        while (!IsBarClosed())
        {
            yield return null;
        }

        // Create a new ball
        NewBocciaBall();
    }


    // MARK: Ball Instantiation
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

        // If this is Play mode, remove the previous ball
        if (_model.GameMode == BocciaGameMode.Play)
        {
            GameObject previousBall = _activeBall;
            Destroy(previousBall);
        }

        // Instantiate the new ball
        _activeBall = Instantiate(ball, newBallPosition, newBallRotation, transform);
        InitializeBall();

        // Set the ball state to ready
        _model.SetBallStateReady();
    }

    
    // MARK: Ball Reset
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

        // Call the method to create a new ball
        StartCoroutine(BallResetCoroutine());
    }

    private IEnumerator BallResetCoroutine()
    {
        // If the bar is NOT closed, wait before creating a new ball
        while (!IsBarClosed())
        {
            yield return null;
        }

        // Create a new ball
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

    private void NavigationChanged()
    {
        // Reset balls every time the game mode is changed
        BocciaGameMode currentGameMode = _model.GameMode;
        if (currentGameMode != _gameMode)
        {
            _gameMode = currentGameMode;
            ResetBocciaBalls();
        }
    }

    public void HandleBallFalling()
    {
        // Reset the ball back onto the ramp
        _activeBall.transform.position = elevationPlate.transform.TransformPoint(_defaultBallPosition);
        _activeBall.transform.rotation = elevationPlate.transform.rotation * _defaultBallRotation;
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