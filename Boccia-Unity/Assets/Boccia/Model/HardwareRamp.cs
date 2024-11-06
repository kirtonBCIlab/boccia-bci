using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class HardwareRamp : RampController
{
    public event Action RampChanged;

    public float Rotation { get; private set; }
    public float Elevation { get; private set; }
    private float MaxElevation { get;} = 100.0f;
    private float MinElevation { get; } = 0.0f;
    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }
    
    public string COMPort { get; private set; }
    public int BaudRate { get; private set; }
    
    private SerialPort _serial;
    public string SerialCommand { get; private set; }

    public bool SerialEnabled { get; private set; }
    
    private List<string> _serialCommands;

    public HardwareRamp()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
        SerialCommand = "";
        _serialCommands = new List<string>();
    }

    public void RotateBy(float degrees)
    {
        Rotation += degrees;
        _serialCommands.Add($"rr{degrees}");
        // Debug.Log($"Hardware rote by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        Rotation = degrees;
        _serialCommands.Add($"ra{degrees}");
        // Debug.Log($"Hardware rote to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float elevation)
    {
        //Old Way
        // Elevation += elevation;
        // Clamped to Max/Min Elevation
        Elevation = Mathf.Clamp(Elevation + elevation, MinElevation, MaxElevation);
        _serialCommands.Add($"er{elevation}");
        // Debug.Log($"Hardware elevate by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        //Old Way
        // Elevation = elevation;
        // Clamped to Max/Min Elevation
        Elevation = Mathf.Clamp(elevation, MinElevation, MaxElevation);
        _serialCommands.Add($"ea{elevation}");
        Debug.Log($"Hardware elevate to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
        _serialCommands.Add("ra0");
        _serialCommands.Add("ea50");
        SendChangeEvent();
    }

    public void DropBall()
    {
        IsBarOpen = true; // Toggle bar state
        _serialCommands.Add("dd-70");
        SendChangeEvent();
    }

    public void ResetBar()
    {
        IsBarOpen = false;
        SendChangeEvent();
    }

    public void ConnectToSerialPort()
    {
        try      
        {
            _serial = new SerialPort(COMPort, BaudRate)
            {
                Encoding = System.Text.Encoding.UTF8,
                DtrEnable = true
            };
            
            _serial.Open();
            // Debug.Log("Connected to port: " + COMPort);
            
            SerialEnabled = true;
        }

        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            SerialEnabled = false;
        }
    }

    public void DisconnectFromSerialPort()
    {
        if (_serial != null && _serial.IsOpen)
        {
            _serial.Close();
            // Debug.Log("Disconnected from port: " + COMPort);
        }
    }

    private void SendChangeEvent()
    {
        RampChanged?.Invoke();
    }

    public void sendSerialCommand()
    {
        SerialCommand = string.Join(">", _serialCommands);

        if (_serial != null && _serial.IsOpen)
        {
            _serial.WriteLine(SerialCommand);
            _serialCommands.Clear();
        }
    }
}