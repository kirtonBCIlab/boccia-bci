
// Interface for an object that manages serial port actions for the hardware ramp
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISerialController
{
    public event System.Action RampChanged;

    public bool ConnectToSerialPort(string comPort, int baudRate);
    public bool DisconnectFromSerialPort();
    public string ReadSerialCommand();
    public Task<string> ReadSerialCommandAsync();
    public void SendSerialCommandList();

    public void AddSerialCommandToList(string command);
    public void ResetSerialCommands();

    // add remaining methods like calibration, test, reset
}

