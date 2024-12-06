using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using System.Threading.Tasks;

public class HardwareRamp : RampController, ISerialController
{
    public event Action RampChanged;

    private BocciaModel _model;

    private bool _wasRotationClamped;  // flag to know if rotation was clamped
    private float _rotation;
    public float Rotation 
    { 
        get { return _rotation; } 
        set 
        { 
            _rotation = Math.Clamp(value, _model.RampSettings.RotationLimitMin, _model.RampSettings.RotationLimitMax); 
            _wasRotationClamped = _rotation != value;
        }
    }

    private bool _wasElevationClamped;  // flag to know if elevation was clamped
    private float _elevation;
    public float Elevation
    { 
        get {return _elevation; }
        set 
        { 
            _elevation = Math.Clamp(value, _model.RampSettings.ElevationLimitMin, _model.RampSettings.ElevationLimitMax); 
            _wasElevationClamped = _elevation != value;
        }
    }

    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }

    public bool SerialEnabled { get; private set; }    

    // Values from Hardware ramp firmware controller
    //private float _rampScaling = 3;                 // Scaling factor: 3 from the Boccia ramp model in the scene
    private float _stepsPerRevolution = 800f;       // Steps per revolution [steps/rev]
    private float _defaultAcceleration = 30f;       // Default acceleration [steps/sec^2]
    private float _gearRatio = 3f;                  // Gear ratio: 3:1

    const float MAX_IMPERIAL_SPEED = 2.0f;  // Max speed: 2 inches/sec at 35 lbs
    const float INCHES_TO_METERS = 0.0254f; 

    private SerialPort _serial;
    private List<string> _serialCommandsList;
    private string _serialCommand;

    public HardwareRamp()
    {
        _model = BocciaModel.Instance;

        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
        _serialCommand = "";
        _serialCommandsList = new List<string>();
    }

    public void RotateBy(float degrees)
    {
        Rotation += degrees;
        
        if (_wasRotationClamped) 
        { 
            degrees = 0; 
            _wasRotationClamped = false;
        }        
        AddSerialCommandToList($"rr{degrees.ToString("0")}");
        _model.SendSerialCommandList();
        // Debug.Log($"Hardware rotate by: {degrees}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        Rotation = degrees;
        // Rotation = Mathf.Clamp(degrees, MinRotation, MaxRotation);
        AddSerialCommandToList($"ra{Rotation.ToString("0")}");
        _model.SendSerialCommandList();
        // Debug.Log($"Hardware rotate to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float elevation)
    {
        //Old Way
        Elevation += elevation;
        
        if (_wasElevationClamped) 
        { 
            elevation = 0; 
            _wasElevationClamped = false;
        }
        AddSerialCommandToList($"er{elevation}");
        _model.SendSerialCommandList();
        // Debug.Log($"Hardware elevate by: {elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        //Old Way
        Elevation = elevation;
        // Clamped to Max/Min Elevation
        // Elevation = Mathf.Clamp(elevation, _minElevation, _maxElevation);
        AddSerialCommandToList($"ea{Elevation.ToString("0")}");
        _model.SendSerialCommandList();
        // Debug.Log($"Hardware elevate to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
        _serialCommandsList.Add($"ra{Rotation.ToString("0")}");
        _serialCommandsList.Add($"ea{Elevation.ToString("0")}");
        _model.SendSerialCommandList();
        SendChangeEvent();
    }

    /// <summary>
    /// Scales rotation speed for correct visualization
    /// </summary>
    /// <param name="speed">Rotation speed [steps/sec]</param>
    /// <returns>Rotation speed [deg/sec]</returns>
    public float ScaleRotationSpeed(float speed)
    {
        float degreesPerStep = 360f / (_stepsPerRevolution * _gearRatio);
        float scaledSpeed = speed * degreesPerStep;
        return scaledSpeed;
    }

    /// <summary>
    /// Scales acceleration for correct visualization
    /// </summary>
    /// <returns> Scaled acceleration [deg/sec^2]</returns>
    public float ScaleRotationAcceleration()
    {
        float degreesPerStep = 360f / (_stepsPerRevolution * _gearRatio);
        float scaledAcceleration = _defaultAcceleration * degreesPerStep;
        return scaledAcceleration;
    }

    /// <summary>
    /// Scales elevation speed for correct visualization
    /// </summary>
    /// <param name="speed">8-bit PWM speed </param>
    /// <returns>Scaled speed [m/s]</returns>
    public float ScaleElevationSpeed(float speed)
    {
        float maxMetricSpeed = MAX_IMPERIAL_SPEED * INCHES_TO_METERS;    // Convert to [m/sec]
        
        float speedPercentage = speed / (float)_model.RampSettings.ElevationSpeedMax;
        float scaledSpeed = speedPercentage * maxMetricSpeed;
        // float scaledSpeed = speedPercentage * maxMetricSpeed * _rampScaling; // Not sure if we need scaling here
        return scaledSpeed;
    }

    public void RandomBallDrop(int randomRotation, int randomElevation)
    {
        RotateTo(randomRotation);
        ElevateTo(randomElevation);
        DropBall();
    }

    public void DropBall()
    {
        IsBarOpen = true; // Toggle bar state
        _serialCommandsList.Add("dd-70");
        SendChangeEvent();
    }

    public void ResetBar()
    {
        IsBarOpen = false;
        SendChangeEvent();
    }

    public bool ConnectToSerialPort(string comPort, int baudRate)
    {
        bool serialEnabled;

        try      
        {
            _serial = new SerialPort(comPort, baudRate)
            {
                Encoding = System.Text.Encoding.UTF8,
                DtrEnable = true,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                RtsEnable = true,
            };
            
            _serial.Open();            
            serialEnabled = true;
        }

        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            serialEnabled = false;
        }

        return serialEnabled;
    }

    public bool DisconnectFromSerialPort()
    {
        bool serialEnabled = false;
        if (_serial != null && _serial.IsOpen)
        {
            try     
            {
                _serial.Close();
                serialEnabled = false;
            }

            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                serialEnabled = true;
            }
        }        

        return serialEnabled;
    }

    public string ReadSerialCommand()
    {
        string message = null;
        
        // Make sure we have a serial port and that the connection is open
        if (_serial != null && _serial.IsOpen)
        {
            message = _serial.ReadLine();
        }

        return message;
    }

    public async Task<string> ReadSerialCommandAsync()
    {
        string message = null;

        if (_serial != null && _serial.IsOpen)
        {
            try
            {
                message = await Task.Run(() => _serial.ReadLine());
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        return message;
    }
    
    public void SendSerialCommandList()
    {
        _serialCommand = string.Join(">", _serialCommandsList) + "\n";

        if (_serial != null && _serial.IsOpen)
        {
            _serial.WriteLine(_serialCommand);
            ResetSerialCommands();
        }
    }

    public void AddSerialCommandToList(string command)
    {
        _serialCommandsList.Add(command);
    }

    public void ResetSerialCommands()
    {
        _serialCommandsList.Clear();
        _serialCommand = "";
    }

    private void SendChangeEvent()
    {
        RampChanged?.Invoke();
    }
}