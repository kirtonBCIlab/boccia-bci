using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayScreenPresenter : MonoBehaviour
{
    public Button resetRampButton;
    public Button randomBallButton;
    public GameObject serialStatusIndicator;
    private bool lastConnectionStatus;
    private Coroutine _checkSerialCoroutine;
    private float _waitTime = 6f;

    private BocciaModel _model;

    private int _randomRotation;
    private int _randomElevation;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;

        // connect buttons to model
        resetRampButton.onClick.AddListener(_model.ResetRampPosition);
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

        CheckConnectionInPlayMode();
    }

    void OnDisable()
    {
        _model.WasChanged -= ModelChanged;
        _model.NavigationChanged -= NavigationChanged;
    }

    private void ModelChanged()
    {

    }

    private void NavigationChanged()
    {
        // Stop the serial connection coroutine if we leave play mode
        if (_model.GameMode != BocciaGameMode.Play)
        {
            if (_checkSerialCoroutine != null)
            {
                StopCoroutine(_checkSerialCoroutine);
                _checkSerialCoroutine = null;
            }
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


    private void CheckConnectionInPlayMode()
    {
        // Start checking the serial connection if we are in play mode
        if (_model.GameMode == BocciaGameMode.Play)
        {
            // Initialize the indicator
            lastConnectionStatus = IsPortConnected(_model.HardwareSettings.COMPort);
            IndicateSerialStatus(lastConnectionStatus);

            // Start the coroutine
            _checkSerialCoroutine = StartCoroutine(CheckSerialPortConnection());
        }
    }

    private IEnumerator CheckSerialPortConnection()
    {
        // Keep checking the serial connection
        while (true)
        {
            // Debug.Log("Checking serial connection");

            // Check the serial port connection
            bool currentStatus = IsPortConnected(_model.HardwareSettings.COMPort);

            // If the status has changed since the last check
            // update the indicator
            if (lastConnectionStatus != currentStatus)
            {
                lastConnectionStatus = currentStatus;

                IndicateSerialStatus(lastConnectionStatus);
            }

            // Check every 6 seconds to reduce computational load
            yield return new WaitForSecondsRealtime(_waitTime);
        }
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
}
