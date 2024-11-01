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
    private List<string> _calibratingMotors = new();
    private Dictionary<string, GameObject> _calibrationChecks = new Dictionary<string, GameObject>();

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
        _calibrationChecks = new Dictionary<string, GameObject>
        {
            { "Drop", dropCheck },
            { "Elevation", elevationCheck },
            { "Rotation", rotationCheck }
        };

        // Set all calibration checks to false
        UnsetCalibrationCheck();
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
        Debug.Log("COM port set to: " + _model.HardwareSettings.COMPort);
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

        Debug.Log("Connecting to serial port: " + _model.HardwareSettings.COMPort + " connection is " + _model.HardwareSettings.IsSerialPortConnected);
        if (_model.HardwareSettings.IsSerialPortConnected)
        {
            Debug.Log("Connected to serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disconnect";
            connectSerialPortButton.GetComponent<Image>().color = Color.red;
            serialPortDropdown.enabled = false;
        }
        else
        {
            Debug.Log("Failed to connect to serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Error";
            connectSerialPortButton.GetComponent<Image>().color = Color.yellow;
        }
    }

    private void DisconnectFromSerialPort()
    {
        _model.HardwareSettings.IsSerialPortConnected = _model.DisconnectFromSerialPort();
        
        if (_model.HardwareSettings.IsSerialPortConnected)
        {
            Debug.Log("Failed to disconnect from serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Error";
            connectSerialPortButton.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            Debug.Log("Disconnected from serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connect";
            connectSerialPortButton.GetComponent<Image>().color = Color.green;
            serialPortDropdown.enabled = true;
            doneButton.interactable = false;
        }
    }

    private void CalibrationHandler()
    {
        Button callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        _rampIsCalibrating = true;

        // Set calibration commands based on button pressed
        if (callingButton == calibrateAllButton)
        {
            _model.AddSerialCommandToList("dd-70");
            _model.AddSerialCommandToList("ec");
            _model.AddSerialCommandToList("rc");
            _model.SendSerialCommandList();

            _calibratingMotors.Add("Drop");
            _calibratingMotors.Add("Elevation");
            _calibratingMotors.Add("Rotation");   
        }
        else if (callingButton == recalibrateBallDropButton)
        {
            _model.AddSerialCommandToList("dd-70");
            _model.SendSerialCommandList();

            _calibratingMotors.Add("Drop");
        }
        else if (callingButton == recalibrateElevationButton)
        {
            _model.AddSerialCommandToList("ec");
            _model.SendSerialCommandList();

            _calibratingMotors.Add("Elevation");
        }
        else if (callingButton == recalibrateRotationButton)
        {
            _model.AddSerialCommandToList("rc");
            _model.SendSerialCommandList();

            _calibratingMotors.Add("Rotation");
        }

        Debug.Log("Calibration started");
        // Iterate through each motor and check if calibration is complete
        System.Threading.Tasks.Task.Run(async () => 
        {
            while (_rampIsCalibrating)
            {
                var message = await _model.ReadSerialCommandAsync();
                if (!string.IsNullOrEmpty(message))
                {
                    foreach (string motor in _calibratingMotors.ToList())
                    {
                        if (SetCalibrationCheck(message, motor))
                        {
                            _calibratingMotors.Remove(motor);
                        }
                    }

                    if (_calibratingMotors.Count == 0)
                    {
                        _rampIsCalibrating = false;
                        doneButton.interactable = true;
                    }
                }
            }
        });
    }

    private bool SetCalibrationCheck(string message, string motor)
    {
        if (message == $"{motor} calibration complete")
        {
            if (_calibrationChecks.ContainsKey(motor))
            {
                _calibrationChecks[motor].SetActive(true);
                return true;
            }
        }

        return false;
    }

    private void UnsetCalibrationCheck()
    {
        // If list is empty, reset to all motors
        if (_calibratingMotors.Count == 0)
        {
            _calibratingMotors.AddRange(new List<string> { "Drop", "Elevation", "Rotation" });
        }
        
        // Inactivate motors check marks
        foreach (string motor in _calibratingMotors)
        {   
            if (_calibrationChecks.ContainsKey(motor))
            {
                _calibrationChecks[motor].SetActive(false);
            }
        }

    }
}
