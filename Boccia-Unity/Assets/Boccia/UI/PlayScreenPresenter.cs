using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayScreenPresenter : MonoBehaviour
{
    [Header("Buttons")]
    public Button resetRampButton;
    public Button randomBallButton;

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

    

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;

        // connect buttons to model
        resetRampButton.onClick.AddListener(ResetRamp);
        randomBallButton.onClick.AddListener(SetRandomBallDropPosition);

        _model.NavigationChanged += NavigationChanged;
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

        _model.ResetRampPosition();
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
