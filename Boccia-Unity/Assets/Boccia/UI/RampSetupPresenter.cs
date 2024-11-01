using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using TMPro;
using UnityEngine.Rendering;
using UnityEditor.VersionControl;
using System.Threading.Tasks;
using System.Linq;

public class RampSetupPresenter : MonoBehaviour
{
    private BocciaModel _model;

    public Button closeButton;
    public Button doneButton;

    [Header("Serial Port")]
    public TMP_Dropdown serialPortDropdown;
    public Button connectSerialPortButton;

    [Header("Calibration buttons")]
    public Button calibrateAllButton;
    public Button recalibrateBallDropButton;
    public Button recalibrateElevationButton;
    public Button recalibrateRotationButton;

    [Header("Calibration checks")]
    public GameObject dropCheck;
    public GameObject elevationCheck;
    public GameObject rotationCheck;

    private bool _rampIsCalibrating = false;
    private List<string> _motorsToCalibrate = new();
    private Dictionary<string, GameObject> _calibrationCheckmarks = new Dictionary<string, GameObject>();

    void Start()
    {
        // cache model
        _model = BocciaModel.Instance;

        // Connect buttons to model
        closeButton.onClick.AddListener(_model.PlayMenu);
        doneButton.onClick.AddListener(_model.Play);

        // Connect serial port buttons to model
        serialPortDropdown.onValueChanged.AddListener(delegate { SaveCOMPortToModel(); });
        connectSerialPortButton.onClick.AddListener(SerialConnectionHandler);
        
        // PopulateSerialPort on start
        PopulateSerialPortDropdown();

        // Connect calibration buttons to model
        calibrateAllButton.onClick.AddListener(CalibrationHandler);
        recalibrateBallDropButton.onClick.AddListener(CalibrationHandler);
        recalibrateElevationButton.onClick.AddListener(CalibrationHandler);
        recalibrateRotationButton.onClick.AddListener(CalibrationHandler);

        // Start with done button enabled until calibration done
        doneButton.interactable = false;

        // Initialize dictionaty to access check marks
        _calibrationCheckmarks = new Dictionary<string, GameObject>
        {
            { "Drop", dropCheck },
            { "Elevation", elevationCheck },
            { "Rotation", rotationCheck }
        };

        // Set all calibration checks to false
        ResetCalibrationStatus();
    }

    // TODO: add functionality to the buttons
    // TODO: add functionality to the indicator boxes

    // Update is called once per frame
    void Update()
    {        
        // if (_model.HardwareSettings.IsSerialPortConnected)
        // {
        //    Debug.Log( _model.ReadSerialCommand() );
        // }
    }

    public void PopulateSerialPortDropdown()
    {
        if (serialPortDropdown.enabled)
        {
            serialPortDropdown.ClearOptions();
            List<string> options = new(SerialPort.GetPortNames());
            if (options.Count == 0)
            {
                options.Add("No serial ports found");
            }
            else
            {
                serialPortDropdown.AddOptions(options);
            }
            
            serialPortDropdown.RefreshShownValue();    
        }
    }

    public void SaveCOMPortToModel()
    {
        _model.HardwareSettings.COMPort = serialPortDropdown.options[serialPortDropdown.value].text;
        // Debug.Log("COM port set to: " + _model.HardwareSettings.COMPort);
    }

    private void SerialConnectionHandler()
    {
        switch (_model.HardwareSettings.IsSerialPortConnected)
        {
            case true:
                DisconnectFromSerialPort();
                break;
            case false:
                ConnectToSerialPort();
                break;
        }
    }

    private void ConnectToSerialPort()
    {
        // _model.HardwareSettings.COMPort = serialPortDropdown.options[serialPortDropdown.value].text;
        SaveCOMPortToModel();
        _model.HardwareSettings.IsSerialPortConnected = _model.ConnectToSerialPort(
            _model.HardwareSettings.COMPort,
            _model.HardwareSettings.BaudRate
            );

        // Debug.Log("Connecting to serial port: " + _model.HardwareSettings.COMPort + " connection is " + _model.HardwareSettings.IsSerialPortConnected);
        // Debug.Log("Baud rate: " + _model.HardwareSettings.BaudRate);
        if (_model.HardwareSettings.IsSerialPortConnected)
        {
            // Debug.Log("Connected to serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disconnect";
            connectSerialPortButton.GetComponent<Image>().color = Color.red;
            serialPortDropdown.enabled = false;
            ResetCalibrationStatus();
        }
        else
        {
            // Debug.Log("Failed to connect to serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Error";
            connectSerialPortButton.GetComponent<Image>().color = Color.yellow;
        }
    }

    private void DisconnectFromSerialPort()
    {
        _model.HardwareSettings.IsSerialPortConnected = _model.DisconnectFromSerialPort();
        
        if (_model.HardwareSettings.IsSerialPortConnected)
        {
            // Debug.Log("Failed to disconnect from serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Error";
            connectSerialPortButton.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            // Debug.Log("Disconnected from serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
            connectSerialPortButton.GetComponent<Image>().color = Color.green;
            serialPortDropdown.enabled = true;
            doneButton.interactable = false;
            _rampIsCalibrating = false;
            
        }
    }

    private void CalibrationHandler()
    {
        _model.ResetSerialCommands();

        Button callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        _rampIsCalibrating = true;
        
        // Set calibration commands based on button pressed
        if (callingButton == calibrateAllButton)
        {
            _model.AddSerialCommandToList("dd-70");
            _model.AddSerialCommandToList("ec");
            _model.AddSerialCommandToList("rc");
            _model.SendSerialCommandList();

            _motorsToCalibrate.Add("Drop");
            _motorsToCalibrate.Add("Elevation");
            _motorsToCalibrate.Add("Rotation");   
        }
        else if (callingButton == recalibrateBallDropButton)
        {
            _model.AddSerialCommandToList("dd-70");
            _model.SendSerialCommandList();

            _motorsToCalibrate.Add("Drop");
        }
        else if (callingButton == recalibrateElevationButton)
        {
            _model.AddSerialCommandToList("ec");
            _model.SendSerialCommandList();

            _motorsToCalibrate.Add("Elevation");
        }
        else if (callingButton == recalibrateRotationButton)
        {
            _model.AddSerialCommandToList("rc");
            _model.SendSerialCommandList();

            _motorsToCalibrate.Add("Rotation");
        }

        // Debug.Log("Calibration started");
        ResetCalibrationStatus();
        StartCoroutine(WaitForMotorsToCalibrate());

    }

    private IEnumerator WaitForMotorsToCalibrate()
    {
        while (_rampIsCalibrating)
        {
            var messageTask = _model.ReadSerialCommandAsync();
            yield return new WaitUntil(() => messageTask.IsCompleted);

            var message = messageTask.Result;
            if (!string.IsNullOrEmpty(message))
            {
                // Debug.Log("Serial received: " + message);

                string motorToRemove = null;
                foreach (string motor in _motorsToCalibrate)
                {
                    if (SetCalibrationStatus(message, motor))
                    {
                        motorToRemove = motor;
                    }
                }

                if (motorToRemove != null)
                {
                    _motorsToCalibrate.Remove(motorToRemove);
                }

                if (_motorsToCalibrate.Count == 0)
                {
                    _rampIsCalibrating = false;
                    doneButton.interactable = true;
                    // Debug.Log("Calibration done");
                }
            }

            // Debug.Log("Calibrating...");
            yield return new WaitForSeconds(1);
        }
    }

    private bool SetCalibrationStatus(string message, string motor)
    {
        bool motorCalibrated = false;
        if (message == $"{motor} calibration complete")
        {
            if (_calibrationCheckmarks.ContainsKey(motor))
            {
                if (_calibrationCheckmarks[motor] != null)
                {
                    _model.HardwareSettings.IsRampCalibrationDone[motor] = true;
                    _calibrationCheckmarks[motor].SetActive(_model.HardwareSettings.IsRampCalibrationDone[motor]);
                }
                else
                {
                    Debug.LogError($"Calibration check GameObject for {motor} not found.");
                }
                motorCalibrated = true;
            }
        }

        return motorCalibrated;
    }

    private void ResetCalibrationStatus()
    {
        doneButton.interactable = false;

        // In case there are no motors to calibrate, reset all motors
        bool reset_all = false;
        if (_motorsToCalibrate.Count == 0)
        {
            reset_all = true;
            _motorsToCalibrate.AddRange(new List<string> { "Drop", "Elevation", "Rotation" });
        }
        
        // Inactivate motors check marks and reset their status in the model
        foreach (string motor in _motorsToCalibrate)
        {   
            if (_calibrationCheckmarks.ContainsKey(motor))
            {
                _model.HardwareSettings.IsRampCalibrationDone[motor] = false;
                _calibrationCheckmarks[motor].SetActive(_model.HardwareSettings.IsRampCalibrationDone[motor]);
            }
        }

        if (reset_all)
        {
            _motorsToCalibrate.Clear();
        }
    }
}
