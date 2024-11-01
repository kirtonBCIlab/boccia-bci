using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using System.Threading.Tasks;

public class HardwareRamp : RampController, ISerialController
{
    public event Action RampChanged;

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
        Rotation = 0.0f;
        Elevation = 50.0f;
        IsBarOpen = false; // Initialize the bar state as closed
        IsMoving = false;
        _serialCommand = "";
        _serialCommandsList = new List<string>();
    }

    public void RotateBy(float degrees)
    {
        Rotation += degrees;
        AddSerialCommandToList($"rr{degrees}");
        // Debug.Log($"Hardware rote by: {Rotation}");
        SendChangeEvent();
    }

    public void RotateTo(float degrees)
    {
        Rotation = degrees;
        AddSerialCommandToList($"ra{degrees}");
        // Debug.Log($"Hardware rote to: {Rotation}");
        SendChangeEvent();
    }

    public void ElevateBy(float elevation)
    {
        Elevation += elevation;
        AddSerialCommandToList($"er{elevation}");
        // Debug.Log($"Hardware elevate by: {Elevation}");
        SendChangeEvent();
    }

    public void ElevateTo(float elevation)
    {
        Elevation = elevation;
        AddSerialCommandToList($"ea{elevation}");
        // Debug.Log($"Hardware elevate to: {Elevation}");
        SendChangeEvent();
    }

    public void ResetRampPosition()
    {
        Rotation = 0.0f;
        Elevation = 50.0f;
        _serialCommandsList.Add("ra0");
        _serialCommandsList.Add("ea50");
        SendChangeEvent();
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
            
            Debug.Log("Serial port connected succesfully");
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