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

    // Start is called before the first frame update
    void Start()
    {
        // cache model
        _model = BocciaModel.Instance;

        // Connect buttons to model
        closeButton.onClick.AddListener(_model.PlayMenu);
        doneButton.onClick.AddListener(_model.VirtualPlay);

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
}
