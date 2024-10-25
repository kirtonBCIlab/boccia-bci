using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RampSetupPresenter : MonoBehaviour
{
    private BocciaModel _model;

    public Button backButton;
    public Button doneButton;

    [Header("Serial Port")]
    //public Dropdown serialPortDropdown;
    // TODO: populate the serial port dropdown options
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
