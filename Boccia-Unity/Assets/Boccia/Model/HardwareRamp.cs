using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class HardwareRamp : RampController, ISerialController
{
    public event Action RampChanged;

    public float Rotation { get; private set; }
    public float Elevation { get; private set; }
    public bool IsBarOpen { get; private set;}
    public bool IsMoving { get; set; }
        
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
        Elevation += elevation;
        _serialCommands.Add($"er{elevation}");
        // Debug.Log($"Hardware elevate by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        Elevation = elevation;
        _serialCommands.Add($"ea{elevation}");
        // Debug.Log($"Hardware elevate to: {Elevation}");
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

    public bool ConnectToSerialPort(string comPort, int baudRate)
    {
        bool serialEnabled;

        try      
        {
            _serial = new SerialPort(comPort, baudRate)
            {
                Encoding = System.Text.Encoding.UTF8,
                DtrEnable = true
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

    private void SendChangeEvent()
    {
        RampChanged?.Invoke();
    }

    public void SendSerialCommand()
    {
        SerialCommand = string.Join(">", _serialCommands);

        if (_serial != null && _serial.IsOpen)
        {
            _serial.WriteLine(SerialCommand);
            _serialCommands.Clear();
        }
    }
}