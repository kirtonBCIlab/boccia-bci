using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using BCIEssentials.StimulusObjects;
using BCIEssentials.StimulusEffects;
using FanNamespace;

public class PlayScreenPresenter : MonoBehaviour
{
    [Header("Buttons")]
    public Button resetRampButton;
    public Button randomBallButton;
    public Button separateBackButton;
    public Button separateDropButton;
    private List<Button> _playButtons;

    [Header("Debug tools")]
    public bool echoSerialCommands = true;
    [SerializeField]
    // Set to false for development mode, true for production mode
    private bool _arduinoIsNeeded = false;

    [Header("Serial Connection")]
    public GameObject serialStatusIndicator;

    private bool connectionStatus;
    private Coroutine _checkSerialCoroutine;
    private Coroutine _readSerialCommandCoroutine;
    private float _waitTime = 6f;

    private BocciaModel _model;

    private int _randomRotation;
    private int _randomElevation;

    private FanPresenter _fanPresenter;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;
        _model.NavigationChanged += NavigationChanged;
        _model.FanChanged += FanTypeChanged;

        // Get the VirtualPlayFan's fan presenter component
        _fanPresenter = GameObject.Find("PlayControlFan").GetComponent<FanPresenter>();

        _playButtons = new List<Button>()
        {
            resetRampButton,
            randomBallButton,
        };

        if (_model.UseSeparateButtons)
        {
            InitializeSeparateButtons();
        }

        // Add listeners to Play buttons
        addListenersToPlayButtons();
    }

    private void InitializeSeparateButtons()
    {
        separateBackButton.gameObject.SetActive(false); // False since we start with coarse fan
        separateDropButton.gameObject.SetActive(true);

        _playButtons.Add(separateBackButton);
        _playButtons.Add(separateDropButton);

        addListenersToSeparateButtons();
    }

    private void addListenersToPlayButtons()
    {
        addListenerToButton(resetRampButton, ResetRamp);
        addListenerToButton(randomBallButton, SetRandomBallDropPosition);
    }

    private void addListenersToSeparateButtons()
    {
        addListenerToButton(separateBackButton, BackButtonClicked);
        addListenerToButton(separateDropButton, DropButtonClicked);
    }

    private void addListenerToButton(Button button, UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners(); // Remove any existing listeners
        button.onClick.AddListener(() => HandleButtonClick(button, action));

        SPO buttonSPO = button.GetComponent<SPO>();
        if (buttonSPO != null)
        {
            buttonSPO.OnSelectedEvent.AddListener(() => button.GetComponent<SPO>().StopStimulus());
            buttonSPO.OnSelectedEvent.AddListener(() => HandleButtonClick(button, action));
        }
    }

    private void HandleButtonClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Store the target button's SPO
            SPO buttonSPO = button.GetComponent<SPO>();
            _model.SetTargetElement(buttonSPO);
        }
        else
        {
            // Execute the normal button action
            action.Invoke();
        }
    }

    private void BackButtonClicked()
    {
        if (_model.IsRampMoving)
        {
            return;
        }

        if (_fanPresenter.positioningMode == FanPositioningMode.CenterToRails)
        {
            _fanPresenter.positioningMode = FanPositioningMode.CenterToBase;
            _fanPresenter.GenerateFanWorkflow();
        }

        separateBackButton.gameObject.SetActive(false); // False since it switches back to coarse
    }

    private void DropButtonClicked()
    {
        if (_model.IsRampMoving)
        {
            return;
        }
        
        if (_fanPresenter.positioningMode == FanPositioningMode.CenterToRails || _fanPresenter.positioningMode == FanPositioningMode.CenterToBase)
        {
            _model.DropBall();
        }
    }

    private void FanTypeChanged()
    {
        // Activate the separate back button (if in use) now that the fan changed to Fine Fan
        if (_model.UseSeparateButtons && (_fanPresenter.positioningMode == FanPositioningMode.CenterToRails))
        {
            separateBackButton.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        if (_model == null)
        {
            _model = BocciaModel.Instance;
        }
        _model.WasChanged += ModelChanged;
        _model.NavigationChanged += NavigationChanged;

        if (_model.GameMode == BocciaGameMode.Play && _arduinoIsNeeded)
        {
            _checkSerialCoroutine = StartCoroutine(CheckSerialPortConnection());
        }

        if (echoSerialCommands && _model.HardwareSettings.IsSerialPortConnected)
        {
            _readSerialCommandCoroutine = StartCoroutine(ReadSerialCommand());
        }
    }

    void OnDisable()
    {
        _model.WasChanged -= ModelChanged;
        _model.NavigationChanged -= NavigationChanged;

        if (_checkSerialCoroutine != null)
        {
            StopCoroutine(_checkSerialCoroutine);
            _checkSerialCoroutine = null;
        }

        if (_readSerialCommandCoroutine != null)
        {
            StopCoroutine(_readSerialCommandCoroutine);
            _readSerialCommandCoroutine = null;
        }
    }

    private void ModelChanged()
    {

    }

    private void NavigationChanged()
    {
        // Make sure the serial connection coroutine is stopped when leaving play mode
        if (_model.GameMode != BocciaGameMode.Play)
        {
            if (_checkSerialCoroutine != null)
            {
                StopCoroutine(_checkSerialCoroutine);
                _checkSerialCoroutine = null;
            }
        }
    }

    private IEnumerator ReadSerialCommand()
    {
        while(_model.GameMode == BocciaGameMode.Play)
        {
            var messageTask = _model.ReadSerialCommandAsync();
            yield return new WaitUntil(() => messageTask.IsCompleted);

            var message = messageTask.Result;
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log("Serial received: " + message);
            }        

            yield return new WaitForSecondsRealtime(1f);
        }
    }


    private void SetRandomBallDropPosition()
    {
        // Generate random values for the ball drop position
        // add +1 to max to maxe it inclusive
        // TODO: The min and max values should come from the model, not hard coded
        _randomRotation = Random.Range(-85, 85+1);
        _randomElevation = Random.Range(0, 100+1);

        _model.RandomBallDrop(_randomRotation, _randomElevation);

        StartCoroutine(WaitForStopBeforeRampReset());
    }

    private IEnumerator WaitForStopBeforeRampReset()
    { 
        while (_model.IsRampMoving)
        {
            yield return null;
        }
        
        ResetRamp();
    }


    private IEnumerator CheckSerialPortConnection()
    {
        // Initialize indicator
        connectionStatus = IsPortConnected(_model.HardwareSettings.COMPort);
        IndicateSerialStatus(connectionStatus);

        // Check every 6 seconds while the serial port is connected
        while (IsPortConnected(_model.HardwareSettings.COMPort))
        {
            // Check every 6 seconds to reduce computational load
            yield return new WaitForSecondsRealtime(_waitTime);
            // Debug.Log("Checking serial connection");
        }

        // If disconnected, update the indicator
        IndicateSerialStatus(false);

        // Wait a bit
        yield return new WaitForSecondsRealtime(_waitTime);

        // Navigate to the ramp setup screen which displays in play menu
        _model.PlayMenu();
        _model.HardwareSettings.IsSerialPortConnected = false;
        _model.ShowRampSetup();

        yield return null;
    }

    private bool IsPortConnected(string comPort)
    {
        // Return true if the serial port is available
        return System.Array.Exists(SerialPort.GetPortNames(), port => port == comPort);
    }

    // Method to update the serial connection status indicator
    private void IndicateSerialStatus(bool status)
    {
        if (status == true)
        {
            serialStatusIndicator.GetComponentInChildren<TextMeshProUGUI>().text = "Serial Connected";
            serialStatusIndicator.GetComponent<Image>().color = Color.green;
        }

        else if (status == false)
        {
            serialStatusIndicator.GetComponentInChildren<TextMeshProUGUI>().text = "Serial Disconnected";
            serialStatusIndicator.GetComponent<Image>().color = Color.red;
        }
    }

    private void ResetRamp()
    {
        _model.ResetRampPosition();
        _model.ResetFanWhenRampResets();
    }
}
