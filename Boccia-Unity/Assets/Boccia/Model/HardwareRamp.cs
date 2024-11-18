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
        // Debug.Log($"Hardware rotate by: {degrees}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        Rotation = degrees;
        // Rotation = Mathf.Clamp(degrees, MinRotation, MaxRotation);
        AddSerialCommandToList($"ra{Rotation.ToString("0")}");
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
        // Debug.Log($"Hardware elevate by: {elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        //Old Way
        Elevation = elevation;
        // Clamped to Max/Min Elevation
        // Elevation = Mathf.Clamp(elevation, _minElevation, _maxElevation);
        AddSerialCommandToList($"ea{Rotation.ToString("0")}");
        // Debug.Log($"Hardware elevate to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
        _serialCommandsList.Add($"ra{Rotation.ToString("0")}");
        _serialCommandsList.Add($"ea{Elevation.ToString("0")}");
        SendChangeEvent();
    }

    public float ScaleRotationSpeed(float speed)
    {
        float stepsPerRevolution = 800f;        // Steps per revolution: 800 steps/rev
        float RotationMotorMaxSpeed = 1000f;    // Max speed: 1000 steps/sec according to AccelStepper library
        float gearRatio = 3f;                   // Gear ratio: 3:1
        float speedPercentage = speed / (_model.RampSettings.RotationSpeedMax - _model.RampSettings.RotationSpeedMin);
        float scaledSpeed = speedPercentage * (RotationMotorMaxSpeed / stepsPerRevolution / gearRatio);
        return scaledSpeed;
    }

    public float ScaleElevationSpeed(float speed)
    {
        float elevationMotorMaxSpeed = 2.0f;    // Max speed: 2 inches/sec at 35 lbs
        float rampScaling = 3;                  // Scaling factor: 3 from the Boccia ramp model in the scene
        float speedPercentage = speed / (_model.RampSettings.ElevationSpeedMax - _model.RampSettings.ElevationSpeedMin);
        float scaledSpeed = speedPercentage * elevationMotorMaxSpeed / rampScaling;
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