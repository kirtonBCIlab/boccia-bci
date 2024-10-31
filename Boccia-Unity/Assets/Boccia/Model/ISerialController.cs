
// Interface for an object that manages serial port actions for the hardware ramp
public interface ISerialController
{
    public event System.Action RampChanged;

    public bool ConnectToSerialPort(string comPort, int baudRate);
    public bool DisconnectFromSerialPort();
    public void sendSerialCommand();

    // add remaining methods like calibration, test, reset
}

