using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using System.Threading.Tasks;

public class HardwareRamp : RampController, ISerialController
{
    public event Action RampChanged;

    private BocciaModel _model;

    public float Rotation { get; private set; }

    public float Elevation { get; private set; }

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
        // Clamped to Min/Max Rotation
        Rotation = Mathf.Clamp(Rotation+degrees, _model.RampSettings.RotationLimitMin, _model.RampSettings.RotationLimitMax);
        AddSerialCommandToList($"rr{degrees}");
        // Debug.Log($"Hardware rotate by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        // Clamped to Min/Max Rotation
        Rotation = Mathf.Clamp(degrees, _model.RampSettings.RotationLimitMin, _model.RampSettings.RotationLimitMax);
        AddSerialCommandToList($"ra{degrees}");
        // Debug.Log($"Hardware rotate to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float height)
    {
        // Clamped to Min/Max Elevation
        Elevation = Mathf.Clamp(Elevation + height, _model.RampSettings.ElevationLimitMin, _model.RampSettings.ElevationLimitMax);
        AddSerialCommandToList($"er{height}");
        // Debug.Log($"Hardware elevate by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float height)
    {
        // Clamped to Min/Max Elevation
        Elevation = Mathf.Clamp(height, _model.RampSettings.ElevationLimitMin, _model.RampSettings.ElevationLimitMax);
        AddSerialCommandToList($"ea{height}");
        // Debug.Log($"Hardware elevate to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = _model.RampSettings.RotationOrigin;
        Elevation = _model.RampSettings.ElevationOrigin;
        _serialCommandsList.Add($"ra{Rotation:D}");
        _serialCommandsList.Add($"ea{Elevation:D}");
        SendChangeEvent();
    }

    public float ScaleRotationSpeed(float speed)
    {
        float stepsPerRevolution = 800f;        // Steps per revolution: 800 steps/rev
        float RotationMotorMaxSpeed = 1000f;    // Max speed: 1000 steps/sec according to AccelStepper library
        float gearRatio = 3f;                   // Gear ratio: 3:1
        float scaledSpeed = (speed / 100f) * (RotationMotorMaxSpeed / stepsPerRevolution / gearRatio);
        return scaledSpeed;
    }

    public float ScaleElevationSpeed(float speed)
    {
        float elevationMotorMaxSpeed = 2.0f;    // Max speed: 2 inches/sec at 35 lbs
        float scaledSpeed = (speed / 100f) * elevationMotorMaxSpeed;
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