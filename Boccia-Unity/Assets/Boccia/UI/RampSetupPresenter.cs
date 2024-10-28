using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Reflection;

public class RampSetupPresenter : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeButton;
    public Button doneButton;

    [Header("Serial Port")]
    public TMP_Dropdown serialPortDropdown;
    public Button connectSerialPortButton;

    [Header("Calibrate")]
    public Button recalibrateBallDropButton;
    public Button recalibrateElevationButton;
    public Button recalibrateRotationButton;

    private BocciaModel _model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model
        _model = BocciaModel.Instance;

        // Connect buttons to model
        closeButton.onClick.AddListener(_model.PlayMenu);
        doneButton.onClick.AddListener(_model.VirtualPlay);

        // Populate the serial port dropdown options
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
        Debug.Log("Populating serial port dropdown");
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
