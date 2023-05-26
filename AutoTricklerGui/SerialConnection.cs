using System.IO.Ports;

namespace AutoTricklerGui
{
    class SerialPortWrapper
    {
        private SerialPort _serialPort;
        private string _comPort;
        private int _baud;

        public SerialPort SerialConnection {
            get { return _serialPort; }
            private set { }
        }

        public SerialPortWrapper(string COMPort, int baud)
        {
            _comPort = COMPort;
            _baud = baud;
            createComPort();
        }

        public void changeComPort(string COMPort)
        {
            _serialPort.Close();
            _comPort = COMPort;
            createComPort();
        }

        private void createComPort()
        {
            _serialPort = new SerialPort(_comPort, _baud, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
        }
    }
}
