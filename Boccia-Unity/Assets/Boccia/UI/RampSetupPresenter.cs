using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using TMPro;
using UnityEngine.Rendering;

public class RampSetupPresenter : MonoBehaviour
{
    private BocciaModel _model;

    public Button closeButton;
    public Button doneButton;

    [Header("Serial Port")]
    public TMP_Dropdown serialPortDropdown;
    public Button connectSerialPortButton;

    [Header("Calibrate")]
    public Button recalibrateBallDropButton;
    public Button recalibrateElevationButton;
    public Button recalibrateRotationButton;

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
    }

    // TODO: add functionality to the buttons
    // TODO: add functionality to the indicator boxes

    // Update is called once per frame
    void Update()
    {
        
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
        _model.HardwareSettings.COMPort = serialPortDropdown.options[serialPortDropdown.value].text;
        _model.HardwareSettings.IsSerialPortConnected = _model.ConnectToSerialPort(
            _model.HardwareSettings.COMPort,
            _model.HardwareSettings.BaudRate
            );

        if (_model.HardwareSettings.IsSerialPortConnected)
        {
            // Debug.Log("Connected to serial port: " + _model.HardwareSettings.COMPort);
            connectSerialPortButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disconnect";
            connectSerialPortButton.GetComponent<Image>().color = Color.red;
            serialPortDropdown.enabled = false;
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
        }
    }
}
