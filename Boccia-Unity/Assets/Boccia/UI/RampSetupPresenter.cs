using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using TMPro;

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
        connectSerialPortButton.onClick.AddListener(_model.ConnectToSerialPort);
        
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

    public void SaveCOMPortToModel()
    {
        _model.HardwareSettings.COMPort = serialPortDropdown.options[serialPortDropdown.value].text;
    }

}
